using Harmony;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace WaterproofTransformer
{
    public class DrywallPatch
    {
        public static string ModName = "WaterproofTransformer";
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
                    AddBuildingStrings(WaterproofTransformerConfig.ID, WaterproofTransformerConfig.DisplayName, WaterproofTransformerConfig.Description, WaterproofTransformerConfig.Effect);
                    AddBuildingToBuildMenu("Power", WaterproofTransformerConfig.ID);
                    AddBuildingStrings(WaterproofBatteryConfig.ID, WaterproofBatteryConfig.DisplayName, WaterproofBatteryConfig.Description, WaterproofBatteryConfig.Effect);
                    AddBuildingToBuildMenu("Power", WaterproofBatteryConfig.ID);
                    didStartupBuilding = true;
                }
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                if (!didStartupDb)
                {
                    AddBuildingToTech("RenewableEnergy", WaterproofTransformerConfig.ID);
                    AddBuildingToTech("GenericSensors", WaterproofBatteryConfig.ID);

                    didStartupDb = true;
                }
            }
        }
    }


}
