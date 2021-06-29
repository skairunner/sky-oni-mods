using Harmony;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace Radiator
{
    public class RadiatorPatch
    {
        public static bool didStartUp_Building;

        public static void OnLoad()
        {
            StartLogging();
        }

        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch(nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Prefix()
            {
                if (!didStartUp_Building)
                {
                    AddBuildingStrings(RadiatorConfig.Id, RadiatorConfig.DisplayName, RadiatorConfig.Description,
                        RadiatorConfig.Effect);
                    AddBuildingToBuildMenu("Utilities", RadiatorConfig.Id);
                    AddStatusItem($"{RadiatorConfig.Id}_RADIATING", "NAME", "Radiating {0}");
                    AddStatusItem($"{RadiatorConfig.Id}_RADIATING", "TOOLTIP",
                        "This radiator is currently radiating heat at {0}.");
                    AddStatusItem($"{RadiatorConfig.Id}_NOTINSPACE", "NAME", "Not in space");
                    AddStatusItem($"{RadiatorConfig.Id}_NOTINSPACE", "TOOLTIP",
                        "This radiator is not fully in space and can't operate.");
                    didStartUp_Building = true;
                }
            }
        }

        [HarmonyPatch(typeof(Database.Techs))]
        [HarmonyPatch("Init")]
        public static class Techs_Init_Patch
        {
            public static void Postfix(Database.Techs __instance)
            {
                AddBuildingToTech(ref __instance, "TemperatureModulation",RadiatorConfig.Id);
            }
        }
    }
}
