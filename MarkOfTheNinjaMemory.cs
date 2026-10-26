using LiveSplit.ComponentUtil;
using System;
using System.Diagnostics;
using System.Linq;

namespace LiveSplit.MarkOfTheNinja
{
    public class MarkOfTheNinjaMemory
    {
        // The executable has a generic name, so candidates are filtered by their PE timestamp
        private const string PROCESS_NAME = "game";
        private const string MODULE_NAME = "game.exe";
        private const int HOOK_RETRY_INTERVAL_MS = 2000;
        private const int MAX_ACTIVE_SCREENS = 32;

        public Process Game { get; private set; }
        public GameBuild Build { get; private set; }
        private IntPtr _moduleBase;
        private DateTime _lastHookAttempt = DateTime.MinValue;

        public MemoryWatcherList Watchers { get; private set; }

        // cApp+0x88: 1 menus, 3 loading, 2 in level, 4 restarting the level, 5 leaving the level
        public MemoryWatcher<int> AppState { get; private set; }
        // cApp+0x150: level file name such as "0_1" (inline std::string, names are under 16 chars)
        public StringWatcher LevelName { get; private set; }
        // NinjaSimulation pointer, changes (or goes through 0) on every level load
        public MemoryWatcher<uint> Simulation { get; private set; }
        // NinjaSimulation+0xC: 99 playing, 1 level complete, 2 game complete
        public MemoryWatcher<int> GameResult { get; private set; }
        // Player entity (first sim entity) +0x9: set while a disablecontrol action is active
        public MemoryWatcher<byte> PlayerControlDisabled { get; private set; }
        // Player stategraph (+0x394): current state name (def at +0x40, inline std::string at +0x4)
        // and time spent in that state in seconds (+0xC)
        public StringWatcher PlayerStateName { get; private set; }
        public MemoryWatcher<float> PlayerStateTime { get; private set; }

        public bool IsLoading { get; private set; }

        private DeepPointer _screenManagerPointer;

        public bool TryHook()
        {
            if (Game != null && !Game.HasExited)
                return true;

            if (Game != null)
            {
                Game.Dispose();
                Game = null;
            }

            if ((DateTime.UtcNow - _lastHookAttempt).TotalMilliseconds < HOOK_RETRY_INTERVAL_MS)
                return false;
            _lastHookAttempt = DateTime.UtcNow;

            foreach (var process in Process.GetProcessesByName(PROCESS_NAME))
            {
                if (Game == null && TryAttach(process))
                {
                    Game = process;
                    continue;
                }
                process.Dispose();
            }

            if (Game == null)
                return false;

            InitializeWatchers();
            return true;
        }

        private bool TryAttach(Process process)
        {
            try
            {
                var module = process.ModulesWow64Safe()
                    .FirstOrDefault(m => m.ModuleName.Equals(MODULE_NAME, StringComparison.OrdinalIgnoreCase));
                if (module == null)
                    return false;

                uint timestamp = ReadModuleTimestamp(process, module);
                var build = MarkOfTheNinjaConstants.DetectBuild(timestamp);
                if (build == null)
                {
                    Log.Error($"Unknown game.exe build (timestamp 0x{timestamp:X8}, module size {module.ModuleMemorySize}) in {module.FileName}");
                    return false;
                }

                Build = build;
                _moduleBase = module.BaseAddress;
                return true;
            }
            catch (Exception ex)
            {
                // Other "game" processes may be 64-bit or protected
                Log.Error($"Could not inspect process {process.Id}: {ex.Message}");
                return false;
            }
        }

        // TimeDateStamp of the PE header, changes with every compilation of the game
        private static uint ReadModuleTimestamp(Process process, ProcessModuleWow64Safe module)
        {
            int peHeaderOffset = process.ReadValue<int>(module.BaseAddress + 0x3C);
            return process.ReadValue<uint>(module.BaseAddress + peHeaderOffset + 0x8);
        }

        private void InitializeWatchers()
        {
            int app = Build.BaseApp;
            int sim = Build.BaseSimulation;

            AppState = new MemoryWatcher<int>(new DeepPointer(MODULE_NAME, app, 0x88)) { Name = "app_state" };
            LevelName = new StringWatcher(new DeepPointer(MODULE_NAME, app, 0x150), ReadStringType.ASCII, 16) { Name = "level_name" };
            Simulation = new MemoryWatcher<uint>(new DeepPointer(MODULE_NAME, sim)) { Name = "simulation" };
            GameResult = new MemoryWatcher<int>(new DeepPointer(MODULE_NAME, sim, 0xC)) { Name = "game_result" };
            PlayerControlDisabled = new MemoryWatcher<byte>(new DeepPointer(MODULE_NAME, sim, 0x4, 0x24, 0x0, 0x9)) { Name = "player_control_disabled" };
            PlayerStateName = new StringWatcher(new DeepPointer(MODULE_NAME, sim, 0x4, 0x24, 0x0, 0x394, 0x40, 0x4), ReadStringType.ASCII, 16) { Name = "player_state_name" };
            PlayerStateTime = new MemoryWatcher<float>(new DeepPointer(MODULE_NAME, sim, 0x4, 0x24, 0x0, 0x394, 0xC)) { Name = "player_state_time" };

            // Values must not linger once the simulation is destroyed
            GameResult.FailAction = MemoryWatcher.ReadFailAction.SetZeroOrNull;
            PlayerControlDisabled.FailAction = MemoryWatcher.ReadFailAction.SetZeroOrNull;
            PlayerStateName.FailAction = MemoryWatcher.ReadFailAction.SetZeroOrNull;
            PlayerStateTime.FailAction = MemoryWatcher.ReadFailAction.SetZeroOrNull;

            _screenManagerPointer = new DeepPointer(MODULE_NAME, app, 0x78, 0x10, 0x0);

            Watchers = new MemoryWatcherList
            {
                AppState, LevelName, Simulation, GameResult, PlayerControlDisabled,
                PlayerStateName, PlayerStateTime
            };
        }

        public void Update()
        {
            try
            {
                Watchers.UpdateAll(Game);
            }
            catch (Exception ex)
            {
                Log.Error($"Watchers.UpdateAll failed: {ex.Message}");
                return;
            }

            IsLoading = IsLoadingScreenOnTop();
        }

        // Walks cScreenManager's active screen list (std::list at +0x48, oldest first) and checks
        // whether the newest screen is the loading screen. Loadout, shop and intro video screens
        // are pushed above it, so they are not counted as loads.
        private bool IsLoadingScreenOnTop()
        {
            if (!_screenManagerPointer.DerefOffsets(Game, out IntPtr screenManager))
                return false;

            uint head = Game.ReadValue<uint>(screenManager + 0x48);
            if (head == 0)
                return false;

            uint top = 0;
            uint node = Game.ReadValue<uint>((IntPtr)head);
            for (int i = 0; node != 0 && node != head && i < MAX_ACTIVE_SCREENS; i++)
            {
                top = node;
                node = Game.ReadValue<uint>((IntPtr)node);
            }
            if (top == 0)
                return false;

            // ChuiScreen+0x10/+0x14: vector<Form*>, the first form's vtable gives the screen class
            uint screen = Game.ReadValue<uint>((IntPtr)(top + 0x8));
            uint formsBegin = Game.ReadValue<uint>((IntPtr)(screen + 0x10));
            uint formsEnd = Game.ReadValue<uint>((IntPtr)(screen + 0x14));
            if (formsBegin == 0 || formsEnd <= formsBegin)
                return false;

            uint form = Game.ReadValue<uint>((IntPtr)formsBegin);
            uint vtable = Game.ReadValue<uint>((IntPtr)form);
            return vtable == (uint)(_moduleBase.ToInt64() + Build.LoadingScreenVtable);
        }

        public void Dispose()
        {
            Game?.Dispose();
            Game = null;
            Build = null;
            IsLoading = false;
        }
    }
}
