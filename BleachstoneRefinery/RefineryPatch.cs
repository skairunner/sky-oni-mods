using Harmony;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace BleachstoneRefinery
{
    public class BleachstoneRefineryPatch
    {
        public static bool didStartUp_Building = false;
        public static bool didStartUp_Db = false;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Prefix()
            {
                if (!didStartUp_Building)
                {
                    AddBuildingStrings(BleachstoneRefineryConfig.Id, BleachstoneRefineryConfig.DisplayName,
                        BleachstoneRefineryConfig.Description, BleachstoneRefineryConfig.Effect);
                    AddBuildingToBuildMenu("Refining", BleachstoneRefineryConfig.Id);
                    didStartUp_Building = true;
                }
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                if (!didStartUp_Db)
                {
                    AddBuildingToTech("Catalytics", BleachstoneRefineryConfig.Id);
                    didStartUp_Db = true;
                }
            }
        }
    }
}