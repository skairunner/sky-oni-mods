using Harmony;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace DrywallHidesPipes
{
    public class DrywallPatch
    {
        public static string ModName = "DrywallHidesPipes";
        public static bool didStartUp_Building = false;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging(ModName);
            }
        }

        [HarmonyPatch(typeof(ExteriorWallConfig), "CreateBuildingDef")]
        public static class ExteriorWallConfig_CreateBuildingDef_Path
        {
            public static void Postfix(BuildingDef __result)
            {
                if (!didStartUp_Building)
                {
                    __result.SceneLayer = Grid.SceneLayer.LogicGatesFront;
                    didStartUp_Building = true;
                }
            }
        }
    }
}
