using Harmony;
using Newtonsoft.Json;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace StoragePod
{
    [RestartRequired]
    public class StoragePodOptions : POptions.SingletonOptions<StoragePodOptions>
    {
        public StoragePodOptions()
        {
            podCapacity = 5000f;
            coolPodCapacity = 50f;
            podStoresFood = false;
        }

        [Option("Pod Capacity", "How many kg of Solids a Storage Pod can store.")]
        [JsonProperty]
        public float podCapacity { get; set; }

        [Option("Cool Pod Capacity", "How many kg of Solids a Cool Pod can store.")]
        [JsonProperty]
        public float coolPodCapacity { get; set; }

        [Option("Pod Stores Food", "Can you store food in a Storage Pod?")]
        [JsonProperty]
        public bool podStoresFood { get; set; }
    }

    public class StoragePodPatch
    {
        public static bool didStartupBuilding;
        public static bool didStartupDb;

        public static void OnLoad()
        {
            StartLogging();
            PUtil.InitLibrary(false);
            POptions.RegisterOptions(typeof(StoragePodOptions));
            PUtil.RegisterPatchClass(typeof(StoragePodPatch));
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

        [PLibMethod(RunAt.BeforeDbInit)]
        internal static void DbInitPrefix()
        {
            if (!didStartupDb)
            {
                AddBuildingToTech("RefinedObjects", StoragePodConfig.ID);
                didStartupDb = true;
            }
        }
    }
}
