using Harmony;
using UnityEngine;

namespace ExpandedLights2
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

        [HarmonyPatch(typeof(CeilingLightConfig), "DoPostConfigurePreview")]
        public static class CeilingLightConfig_DoPostConfigurePreview_Patch
        {
            public static void Postfix(BuildingDef def, GameObject go)
            {
                go.AddComponent<LightShapePreview>().shape = ExpandedLightsPatch.FixedSemi.GetKLightShape();
            }
        }

        [HarmonyPatch(typeof(CeilingLightConfig), "DoPostConfigureComplete")]
        public static class CeilingLightConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                go.AddComponent<Light2D>().shape = ExpandedLightsPatch.FixedSemi.GetKLightShape();
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

        [HarmonyPatch(typeof(FloorLampConfig), "DoPostConfigurePreview")]
        public static class FloorLampConfig_DoPostConfigurePreview_Patch
        {
            public static void Postfix(BuildingDef def, GameObject go)
            {
                go.AddComponent<LightShapePreview>().shape = ExpandedLightsPatch.SmoothCircle.GetKLightShape();
            }
        }

        [HarmonyPatch(typeof(FloorLampConfig), "DoPostConfigureComplete")]
        public static class FloorLampConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                go.AddComponent<Light2D>().shape = ExpandedLightsPatch.SmoothCircle.GetKLightShape();
            }
        }
    }
}
