using Harmony;
using System;
using System.Reflection;
using UnityEngine;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace ExpandedLights
{
    public class ExpandedLightsPatch
    {
        public static string ModName = "ExpandedLights";
        public static bool didStartUp_Building = false;
        public static bool didStartUp_Db = false;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging(ModName);
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
                    AddBuildingStrings(FloodlightConfig.Id, FloodlightConfig.DisplayName, FloodlightConfig.Description, FloodlightConfig.Effect);
                    AddBuildingToBuildMenu("Furniture", FloodlightConfig.Id);
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
                if (!didStartUp_Db)
                {
                    AddBuildingToTech("Artistry", FloodlightConfig.Id);
                    didStartUp_Db = true;
                }
            }
        }
    }
}
