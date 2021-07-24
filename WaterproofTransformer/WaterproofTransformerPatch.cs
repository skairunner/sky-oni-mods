using HarmonyLib;
using static SkyLib.OniUtils;

namespace WaterproofTransformer
{
    public class DrywallPatch
    {
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                AddBuildingStrings(
                    WaterproofTransformerConfig.ID,
                    WaterproofTransformerConfig.DisplayName,
                    WaterproofTransformerConfig.Description,
                    WaterproofTransformerConfig.Effect);
                AddBuildingStrings(
                    WaterproofBatteryConfig.ID,
                    WaterproofBatteryConfig.DisplayName,
                    WaterproofBatteryConfig.Description,
                    WaterproofBatteryConfig.Effect);
            }

            public static void Postfix()
            {
                AddBuildingToBuildMenu("Power", WaterproofTransformerConfig.ID);
                AddBuildingToBuildMenu("Power", WaterproofBatteryConfig.ID);
                AddBuildingToTech("RenewableEnergy", WaterproofTransformerConfig.ID);
                AddBuildingToTech("GenericSensors", WaterproofBatteryConfig.ID);
            }
        }
    }
}
