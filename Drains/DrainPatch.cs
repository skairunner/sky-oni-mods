using HarmonyLib;
using PeterHan.PLib.Options;
using static SkyLib.Logger;
using static SkyLib.OniUtils;
using PeterHan.PLib;
using PeterHan.PLib.Core;
using PeterHan.PLib.PatchManager;

namespace Drains
{
    public class DrainPatch
    {
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                AddBuildingStrings(
                    DrainConfig.Id,
                    DrainConfig.DisplayName,
                    DrainConfig.Description,
                    DrainConfig.Effect);
            }

            public static void Postfix()
            {
                AddBuildingToBuildMenu("Plumbing", DrainConfig.Id);
                AddBuildingToTech("SanitationSciences", DrainConfig.Id);
            }
        }
    }
}
