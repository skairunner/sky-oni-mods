using Harmony;
using Newtonsoft.Json;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace StoragePod
{
    public class StoragePodOptions
    {
        [Option("Pod Capacity", "How many kg of Solids a Storage Pod can store.")]
        [JsonProperty]
        public float podCapacity { get; set; }

        [Option("Cool Pod Capacity", "How many kg of Solids a Cool Pod can store.")]
        [JsonProperty]
        public float coolPodCapacity { get; set; }

        public StoragePodOptions()
        {
            podCapacity = 5000f;
            coolPodCapacity = 50f;
        }
    }

    public class StoragePodPatch
    {
        public static bool didStartupBuilding = false;
        public static bool didStartupDb = false;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
                PUtil.InitLibrary(false);
                POptions.RegisterOptions(typeof(StoragePodOptions));
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Prefix()
            {
                if (!didStartupBuilding)
                {
                    AddBuildingStrings(StoragePodConfig.ID, StoragePodConfig.DisplayName, StoragePodConfig.Description,
                        StoragePodConfig.Effect);
                    AddBuildingToBuildMenu("Base", StoragePodConfig.ID);
                    didStartupBuilding = true;
                }
            }
        }

        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                if (!didStartupDb)
                {
                    AddBuildingToTech("RefinedObjects", StoragePodConfig.ID);
                    didStartupDb = true;
                }
            }
        }
    }
}