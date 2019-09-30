using Harmony;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace Drain
{
    public class DrainPatch
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
                    AddBuildingStrings(DrainConfig.Id, DrainConfig.DisplayName, DrainConfig.Description, DrainConfig.Effect);
                    AddBuildingToBuildMenu("Plumbing", DrainConfig.Id);
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
                    AddBuildingToTech("SanitationSciences", DrainConfig.Id);
                    didStartUp_Db = true;
                }
            }
        }
    }
}
