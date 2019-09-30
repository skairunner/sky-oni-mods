using Harmony;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace DrywallHidesPipes
{
    public class DrywallPatch
    {
        public static bool didStartUp_Building = false;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
            }
        }

        [HarmonyPatch(typeof(ExteriorWallConfig), "CreateBuildingDef")]
        public static class ExteriorWallConfig_CreateBuildingDef_Path
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.SceneLayer = Grid.SceneLayer.LogicGatesFront;
            }
        }

        [HarmonyPatch(typeof(ThermalBlockConfig), "CreateBuildingDef")]
        public static class ThermalBlockConfig_CreateBuildingDef_Path
        {
            public static void Postfix(BuildingDef __result)
            {
                __result.SceneLayer = Grid.SceneLayer.LogicGatesFront;
            }
        }
    }
}
