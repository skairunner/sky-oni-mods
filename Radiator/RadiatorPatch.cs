using Harmony;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace Radiator
{
    public class RadiatorPatch
    {
        public static bool didStartUp_Building = false;
        public static bool didStartUp_Db = false;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch(nameof(GeneratedBuildings.LoadGeneratedBuildings))]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Prefix()
            {
                if (!didStartUp_Building)
                {
                    AddBuildingStrings(RadiatorConfig.Id, RadiatorConfig.DisplayName, RadiatorConfig.Description, RadiatorConfig.Effect);
                    AddBuildingToBuildMenu("Utilities", RadiatorConfig.Id);
                    AddStatusItem($"{RadiatorConfig.Id}_RADIATING", "NAME", "Radiating {0}");
                    AddStatusItem($"{RadiatorConfig.Id}_RADIATING", "TOOLTIP", "This radiator is currently radiating heat at {0}.");
                    AddStatusItem($"{RadiatorConfig.Id}_NOTINSPACE", "NAME", "Not in space");
                    AddStatusItem($"{RadiatorConfig.Id}_NOTINSPACE", "TOOLTIP", "This radiator is not fully in space and can't operate.");
                    didStartUp_Building = true;
                }
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                if(!didStartUp_Db)
                {
                    AddBuildingToTech("TemperatureModulation", RadiatorConfig.Id);
                    didStartUp_Db = true;
                }
            }
        }
    }
}
