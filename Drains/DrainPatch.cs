using Harmony;
using PeterHan.PLib.Options;
using static SkyLib.Logger;
using static SkyLib.OniUtils;
using PeterHan.PLib;

namespace Drains
{
    public class DrainPatch
    {
        public static bool didStartUp_Building;
        public static bool didStartUp_Db;

        public static void OnLoad()
        {
            StartLogging();
            PUtil.InitLibrary(false);
            POptions.RegisterOptions(typeof(DrainOptions));
            PUtil.RegisterPatchClass(typeof(DrainPatch));
        }

        [HarmonyPatch(typeof(GeneratedBuildings))]
        [HarmonyPatch(nameof(GeneratedBuildings.LoadGeneratedBuildings))]
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

        [PLibMethod(RunAt.BeforeDbInit)]
        internal static void DbInitPrefix()
        {
            if (!didStartUp_Db)
            {
                AddBuildingToTech("SanitationSciences", DrainConfig.Id);
                didStartUp_Db = true;
            }
        }
    }
}
