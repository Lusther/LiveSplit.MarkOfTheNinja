using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace LiveSplit.MarkOfTheNinja
{
    public class MarkOfTheNinjaComponent : IComponent
    {
        public string ComponentName => "Mark of the Ninja Autosplitter";

        public float HorizontalWidth => 0;
        public float VerticalHeight => 0;
        public float MinimumWidth => 0;
        public float MinimumHeight => 0;
        public float PaddingTop => 0;
        public float PaddingBottom => 0;
        public float PaddingLeft => 0;
        public float PaddingRight => 0;

        public IDictionary<string, Action> ContextMenuControls => null;

        // The final split is confirmed by the game result, which lands 0.3 to 0.7 s after the
        // choice input; a control loss older than this is not the choice input.
        private static readonly TimeSpan MAX_CHOICE_TO_RESULT_DELAY = TimeSpan.FromSeconds(3);

        private readonly TimerModel _timer;
        private readonly LiveSplitState _state;
        private readonly MarkOfTheNinjaSettings _settings;
        private readonly MarkOfTheNinjaMemory _memory;

        private bool _hooked;
        // Set on every level load, cleared once the player first gets control in that level
        private bool _waitingForFirstControl;
        private bool _firstControlThisUpdate;
        // Timer value when the player lost control in the last level, i.e. the choice input
        private Time? _lastControlLossTime;

        public MarkOfTheNinjaComponent(LiveSplitState state)
        {
            _state = state;
            _timer = new TimerModel { CurrentState = state };
            _settings = new MarkOfTheNinjaSettings();
            _memory = new MarkOfTheNinjaMemory();
            Log.Info("Component loaded.");
        }

        public void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            if (!_memory.TryHook())
            {
                if (_hooked)
                {
                    Log.Info("Game process lost.");
                    _hooked = false;
                    UpdateVersionDisplay(state);
                }
                // A crashed or closed game does not count towards game time
                if (_settings.RemoveLoads && state.CurrentPhase == TimerPhase.Running)
                    state.IsGameTimePaused = true;
                return;
            }

            if (!_hooked)
            {
                Log.Info($"Hooked to process: {_memory.Game.ProcessName} (PID {_memory.Game.Id}), {_memory.Build}");
                _hooked = true;
                UpdateVersionDisplay(state);
            }

            _memory.Update();
            TrackLevelEvents(state);

            switch (state.CurrentPhase)
            {
                case TimerPhase.NotRunning:
                    if (ShouldStart())
                    {
                        Log.Info("Starting timer.");
                        _timer.Start();
                        _timer.InitializeGameTime();
                    }
                    break;

                case TimerPhase.Running:
                case TimerPhase.Paused:
                    if (ShouldReset())
                    {
                        Log.Info("Resetting timer.");
                        _timer.Reset();
                        break;
                    }
                    if (_settings.RemoveLoads)
                        state.IsGameTimePaused = _memory.IsLoading;
                    if (state.CurrentPhase == TimerPhase.Running)
                        CheckSplits(state);
                    break;
            }
        }

        private void TrackLevelEvents(LiveSplitState state)
        {
            if (_memory.Simulation.Changed)
            {
                _waitingForFirstControl = true;
                _lastControlLossTime = null;
            }

            var control = _memory.PlayerControlDisabled;

            // The control flag is released 0.5 s into the get-up animation, but inputs are only
            // taken once the animation reaches its branch time (or is left early by an input).
            var stateName = _memory.PlayerStateName;
            var stateTime = _memory.PlayerStateTime;
            bool leftGetUp = stateName.Old == MarkOfTheNinjaConstants.GetUpState
                && stateName.Current != MarkOfTheNinjaConstants.GetUpState;
            bool reachedBranch = stateName.Current == MarkOfTheNinjaConstants.GetUpState
                && stateTime.Current >= MarkOfTheNinjaConstants.GetUpBranchTime
                && stateTime.Old < MarkOfTheNinjaConstants.GetUpBranchTime;

            // A destroyed simulation also reads as empty, so the simulation has to be alive and stable
            _firstControlThisUpdate = _waitingForFirstControl
                && !_memory.Simulation.Changed && _memory.Simulation.Current != 0
                && control.Current == 0
                && (leftGetUp || reachedBranch);
            if (_firstControlThisUpdate)
                _waitingForFirstControl = false;

            if (_memory.LevelName.Current == MarkOfTheNinjaConstants.LastLevel
                && control.Old == 0 && control.Current == 1)
            {
                _lastControlLossTime = state.CurrentTime;
            }
        }

        private bool ShouldStart()
        {
            return _settings.AutoStart
                && _firstControlThisUpdate
                && _memory.LevelName.Current == MarkOfTheNinjaConstants.FirstLevel
                && _memory.AppState.Current == MarkOfTheNinjaConstants.AppStateInLevel;
        }

        private bool ShouldReset()
        {
            if (!_settings.AutoReset) return false;

            return _memory.LevelName.Current == MarkOfTheNinjaConstants.FirstLevel
                && _memory.AppState.Old == MarkOfTheNinjaConstants.AppStateInLevel
                && _memory.AppState.Current == MarkOfTheNinjaConstants.AppStateRestartingLevel;
        }

        private static bool BecameValue(ComponentUtil.MemoryWatcher<int> w, int value) => w.Current == value && w.Old != value;

        private void CheckSplits(LiveSplitState state)
        {
            if (_settings.SplitOnLevelComplete && BecameValue(_memory.GameResult, MarkOfTheNinjaConstants.ResultLevelComplete))
            {
                Log.Info($"Level {_memory.LevelName.Current} complete, splitting at index {state.CurrentSplitIndex}.");
                _timer.Split();
                return;
            }

            if (_settings.SplitOnFinalChoice
                && _memory.LevelName.Current == MarkOfTheNinjaConstants.LastLevel
                && BecameValue(_memory.GameResult, MarkOfTheNinjaConstants.ResultGameComplete))
            {
                SplitAtChoiceInput(state);
            }
        }

        // The game only confirms the ending once the outro starts, so the split happens then
        // and its time is moved back to the moment the player lost control (the choice input).
        private void SplitAtChoiceInput(LiveSplitState state)
        {
            int index = state.CurrentSplitIndex;
            Time now = state.CurrentTime;
            _timer.Split();
            if (state.CurrentSplitIndex != index + 1)
                return;

            if (_lastControlLossTime is Time input && now.RealTime - input.RealTime <= MAX_CHOICE_TO_RESULT_DELAY)
            {
                state.Run[index].SplitTime = input;
                Log.Info($"Final choice, split at index {index} moved back by {now.RealTime - input.RealTime} to the input.");
            }
            else
            {
                Log.Error($"Final choice, split at index {index} without a recent control loss, time not adjusted.");
            }
        }

        private const string VERSION_VARIABLE = "MarkOfTheNinja_Version";
        private const string BUILD_VARIABLE = "MarkOfTheNinja_Build";

        // Reflection is required because the CustomVariable API was added in a newer
        // LiveSplit.Core than the one available at compile time.
        private static MethodInfo _getOrAddCustomVariable;
        private static MethodInfo _setCustomVariable;
        private static bool _reflectionResolved;

        private void UpdateVersionDisplay(LiveSplitState state)
        {
            string versionText = _hooked ? $"{_memory.Build.Branch} (build {_memory.Build.BuildId})" : "";
            string buildText = _hooked ? _memory.Build.BuildId.ToString() : "";

            try
            {
                if (!_reflectionResolved)
                {
                    var metaType = state.Run.Metadata.GetType();
                    _getOrAddCustomVariable = metaType.GetMethod("GetOrAddCustomVariable");
                    _setCustomVariable = metaType.GetMethod("SetCustomVariable");
                    _reflectionResolved = true;
                }

                var metadata = state.Run.Metadata;
                _getOrAddCustomVariable?.Invoke(metadata, new object[] { VERSION_VARIABLE });
                _setCustomVariable?.Invoke(metadata, new object[] { VERSION_VARIABLE, versionText });
                _getOrAddCustomVariable?.Invoke(metadata, new object[] { BUILD_VARIABLE });
                _setCustomVariable?.Invoke(metadata, new object[] { BUILD_VARIABLE, buildText });
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set custom variable: {ex.Message}");
            }
        }

        public Control GetSettingsControl(LayoutMode mode) => _settings;
        public XmlNode GetSettings(XmlDocument document) => _settings.GetSettings(document);
        public void SetSettings(XmlNode settings) => _settings.SetSettings(settings);

        public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
        public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }

        public void Dispose()
        {
            _memory.Dispose();
        }
    }
}
