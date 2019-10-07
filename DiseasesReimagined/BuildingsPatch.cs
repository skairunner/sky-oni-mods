using System;
using Harmony;
using static SkyLib.OniUtils;

namespace DiseasesReimagined
{
    class BuildingsPatch
    {
        public static class Mod_OnLoad
        {
            public static void OnLoad() {}
        }

        [HarmonyPatch(typeof(Debug), "Assert", typeof(bool))]
        public static class Debug_Assert_Patch1
        {
            public static void Postfix(bool condition)
            {
                if (!condition)
                    SkyLib.Logger.LogLine(Environment.StackTrace);
            }
        }

        [HarmonyPatch(typeof(Debug), "Assert", typeof(bool), typeof(object))]
        public static class Debug_Assert_Patch2
        {
            public static void Postfix(bool condition)
            {
                if (!condition)
                    SkyLib.Logger.LogLine(Environment.StackTrace);
            }
        }
        
        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Postfix()
            {
                AddBuildingStrings(UVCleanerConfig.ID, UVCleanerConfig.DisplayName, UVCleanerConfig.Description,
                    UVCleanerConfig.Effect);
                AddBuildingToBuildMenu("Medical", UVCleanerConfig.ID);
            }
        }

        [HarmonyPatch(typeof(Db),"Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Postfix()
            {
                AddBuildingToTech("MedicineIII", UVCleanerConfig.ID);
            }
        }
    }
}
