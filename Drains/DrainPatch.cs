using Harmony;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace Drains
{
    public class DrainPatch
    {
        public static bool didStartUp_Building;
        public static bool didStartUp_Db;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
                PUtil.InitLibrary(false);
                POptions.RegisterOptions(typeof(DrainSettings));
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Prefix()
            {
                if (!didStartUp_Building)
                {
                    AddBuildingStrings(DrainConfig.Id, DrainConfig.DisplayName, DrainConfig.Description,
                        DrainConfig.Effect);
                    AddBuildingToBuildMenu("Plumbing", DrainConfig.Id);
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
                    AddBuildingToTech("SanitationSciences", DrainConfig.Id);
                    didStartUp_Db = true;
                }
            }
        }

        [HarmonyPatch(typeof(SelectToolHoverTextCard), "ShouldShowLiquidConduitOverlay")]
        public static class SelectToolHoverTextCard_ShouldShowLiquidConduitOverlay_Patch
        {
            public static void Postfix(KSelectable selectable, ref bool __result)
            {
                if (selectable.GetComponent<Drain>() != null)
                    __result = true;
            }
        }
    }
}