using HarmonyLib;

using Newtonsoft.Json;
using PeterHan.PLib.Options;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace StoragePod
{
    [RestartRequired]
    public class StoragePodOptions : SingletonOptions<StoragePodOptions>
    {
        public StoragePodOptions()
        {
            podCapacity = 5000f;
            coolPodCapacity = 50f;
            podStoresFood = false;
        }

        [Option("Pod Capacity", "How many kg of Solids a Storage Pod can store.", Format = "F0")]
        [JsonProperty]
        public float podCapacity { get; set; }

        [Option("Cool Pod Capacity", "How many kg of Solids a Cool Pod can store.", Format = "F0")]
        [JsonProperty]
        public float coolPodCapacity { get; set; }

        [Option("Pod Stores Food", "Can you store food in a Storage Pod?")]
        [JsonProperty]
        public bool podStoresFood { get; set; }
    }

    public class StoragePodPatch
    {
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                AddBuildingStrings(
                    StoragePodConfig.ID,
                    StoragePodConfig.DisplayName,
                    StoragePodConfig.Description,
                    StoragePodConfig.Effect);
                AddBuildingStrings(
                    CoolPodConfig.ID,
                    CoolPodConfig.DisplayName,
                    CoolPodConfig.Description,
                    CoolPodConfig.Effect);
            }

            public static void Postfix()
            {
                AddBuildingToBuildMenu("Base", StoragePodConfig.ID);
                AddBuildingToBuildMenu("Food", CoolPodConfig.ID);
                AddBuildingToTech("RefinedObjects", StoragePodConfig.ID);
                AddBuildingToTech("Agriculture", CoolPodConfig.ID);
            }
        }
    }
}
