using System.Linq;

namespace LiveSplit.MarkOfTheNinja
{
    public class GameBuild
    {
        // Steam depot build id
        public int BuildId { get; }
        // Steam branch the build was published on
        public string Branch { get; }
        // TimeDateStamp from the game.exe PE header, unique per compilation
        public uint Timestamp { get; }
        public int ModuleSize { get; }

        // RVAs in game.exe: the cApp singleton, the current NinjaSimulation and the cLoadingScreen vtable
        public int BaseApp { get; }
        public int BaseSimulation { get; }
        public int LoadingScreenVtable { get; }

        public GameBuild(int buildId, string branch, uint timestamp, int moduleSize,
            int app, int simulation, int loadingScreenVtable)
        {
            BuildId = buildId;
            Branch = branch;
            Timestamp = timestamp;
            ModuleSize = moduleSize;
            BaseApp = app;
            BaseSimulation = simulation;
            LoadingScreenVtable = loadingScreenVtable;
        }

        public override string ToString() => $"build {BuildId} ({Branch})";
    }

    public static class MarkOfTheNinjaConstants
    {
        public const string FirstLevel = "0_1";
        public const string LastLevel = "4_2";

        // Level start animation of the ninja (0-1 checkpoint StartAnim). Its "branch" time tag,
        // from which the player's inputs are taken, starts at 1.0667 s (64 ticks).
        public const string GetUpState = "get_up_tattoo";
        public const float GetUpBranchTime = 1.066f;

        // Values of NinjaSimulation+0xC, set by the level script's setgameover action
        public const int ResultPlaying = 99;
        public const int ResultLevelComplete = 1;
        public const int ResultGameComplete = 2;

        // Values of cApp+0x88
        public const int AppStateInLevel = 2;
        public const int AppStateRestartingLevel = 4;

        // Newest first. Columns:
        // build id, branch, game.exe timestamp, module size, cApp, NinjaSimulation, cLoadingScreen vtable
        public static readonly GameBuild[] Builds =
        {
            new GameBuild(225199, "public", 0x52428B6E, 4468736, 0x3AB5DC, 0x3A5D48, 0x325B0C),
        };

        public static GameBuild DetectBuild(uint timestamp)
        {
            return Builds.FirstOrDefault(b => b.Timestamp == timestamp);
        }
    }
}
