using Harmony;
using UnityEngine;

namespace ExpandedLights
{
    public class StockLightsPatch
    {
        public static string ModName = "ExpandedLights_Stock";

        [HarmonyPatch(typeof(CeilingLightConfig), "CreateBuildingDef")]
        public static class CeilingLightConfig_CreateBuildingDef_Patch
        {
            public static BuildingDef Postfix(BuildingDef __result)
            {
                __result.SelfHeatKilowattsWhenActive = 0.1f;
                return __result;
            }
        }



        [HarmonyPatch(typeof(FloorLampConfig), "CreateBuildingDef")]
        public static class FloorLampConfig_CreateBuildingDef_Patch
        {
            public static BuildingDef Postfix(BuildingDef __result)
            {
                __result.EnergyConsumptionWhenActive = 4f;
                __result.SelfHeatKilowattsWhenActive = 0.1f;
                return __result;
            }
        }
    }
}
