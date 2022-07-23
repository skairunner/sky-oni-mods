using Harmony;
using PeterHan.PLib;
using UnityEngine;

namespace RadiateHeatInSpace
{
    public static class VeryLatePatches
    {
        [PLibPatch(RunAt.AfterModsLoad, "DoPostConfigureComplete", IgnoreOnFail = true, PatchType = HarmonyPatchType.Postfix, RequireType = "ExpandedLights.FloodlightConfig")]
        public static void Patch_Floodlights(GameObject go)
        {
            RadiatePatch.AttachHeatComponent(go, 0.9f, 1f);
        }

        [PLibPatch(RunAt.AfterModsLoad, "DoPostConfigureComplete", IgnoreOnFail = true, PatchType = HarmonyPatchType.Postfix, RequireType = "ExpandedLights.TileLightConfig")]
        public static void Patch_TileLight(GameObject go)
        {
            RadiatePatch.AttachHeatComponent(go, 0.9f, 0.2f);
        }

        [PLibPatch(RunAt.AfterModsLoad, "DoPostConfigureComplete", IgnoreOnFail = true, PatchType = HarmonyPatchType.Postfix, RequireType = "WaterproofTransformer.WaterproofTransformerConfig")]
        public static void Patch_WaterproofTransformer(GameObject go)
        {
            RadiatePatch.AttachHeatComponent(go, 0.6f, 0.4f);
        }

        [PLibPatch(RunAt.AfterModsLoad, "DoPostConfigureComplete", IgnoreOnFail = true, PatchType = HarmonyPatchType.Postfix, RequireType = "WaterproofTransformer.WaterproofBatteryConfig")]
        public static void Patch_WaterproofBattery(GameObject go)
        {
            RadiatePatch.AttachHeatComponent(go, 0.6f, 0.4f);
        }

        [PLibPatch(RunAt.AfterModsLoad, "DoPostConfigureComplete", IgnoreOnFail = true, PatchType = HarmonyPatchType.Postfix, RequireType = "rlane.MeteorDefenseLaserConfig")]
        public static void Patch_MeteorDefenseLaser(GameObject go)
        {
            RadiatePatch.AttachHeatComponent(go, .2f, 3f);
        }
    }
}
