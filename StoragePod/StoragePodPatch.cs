using Harmony;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace DrywallHidesPipes
{
    public class DrywallPatch
    {
        public static string ModName = "StoragePod";
        public static bool didStartupBuilding = false;
        public static bool didStartupDb = false;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging(ModName);
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Prefix()
            {
                if (!didStartupBuilding)
                {
                    AddBuildingStrings(StoragePodConfig.ID, StoragePodConfig.DisplayName, StoragePodConfig.Description, StoragePodConfig.Effect);
                    AddBuildingToBuildMenu("Base", StoragePodConfig.ID);
                    didStartupBuilding = true;
                }
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                if (!didStartupDb)
                {
                    AddBuildingToTech("RefinedObjects", StoragePodConfig.ID);
                    didStartupDb = true;
                }
            }
        }
    }
}
