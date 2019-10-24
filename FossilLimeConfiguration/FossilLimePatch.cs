using Harmony;
using UnityEngine;
using static SkyLib.Logger;

namespace FossilLimeConfiguration
{
    public static class FossilLimePatch
    {
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
            }
        }

        [HarmonyPatch(typeof(RockCrusherConfig), "ConfigureBuildingTemplate")]
        public static class RockCrusherConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                // First we need to find the existing lime recipe.
                
            }
        }
    }
}