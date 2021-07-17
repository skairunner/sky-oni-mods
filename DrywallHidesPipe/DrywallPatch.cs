using HarmonyLib;
using KMod;
using static SkyLib.Logger;

namespace DrywallHidesPipes
{
    public class DrywallPatch
    {
        public static bool didStartUp_Building = false;

        public class Mod_OnLoad : UserMod2
        {
            public override void OnLoad(Harmony harmony)
            {
                base.OnLoad(harmony);
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
