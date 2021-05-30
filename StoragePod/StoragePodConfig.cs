using TUNING;
using UnityEngine;

namespace StoragePod
{
    internal class StoragePodConfig : IBuildingConfig
    {
        public const string ID = "StoragePodConfig";
        public const string DisplayName = "Storage Pod";
        public const string Description = "Now you, too, can store things in pods.";

        public static string Effect =
            "Stores the Solid resources of your choosing. Compact and can be built anywhere.";

        public override BuildingDef CreateBuildingDef()
        {
            var id = ID;
            var width = 1;
            var height = 1;
            var anim = "storagePod_kanim";
            var hitpoints = 30;
            var construction_time = 10f;
            float[] tieR4 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
            string[] construction_mats = MATERIALS.REFINED_METALS;
            var melting_point = 1600f;
            var build_location_rule = BuildLocationRule.Anywhere;
            var none = NOISE_POLLUTION.NONE;
            var buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints,
                construction_time, tieR4, construction_mats, melting_point, build_location_rule,
                BUILDINGS.DECOR.PENALTY.TIER1, none);
            buildingDef.Floodable = false;
            buildingDef.AudioCategory = "Metal";
            buildingDef.Overheatable = false;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            SoundEventVolumeCache.instance.AddVolume("storagelocker_kanim", "StorageLocker_Hit_metallic_low",
                NOISE_POLLUTION.NOISY.TIER1);
            Prioritizable.AddRef(go);
            var storage = go.AddOrGet<Storage>();
            storage.showInUI = true;
            storage.allowItemRemoval = true;
            storage.showDescriptor = true;
            System.Collections.Generic.List<Tag> storedItems = new System.Collections.Generic.List<Tag>();
            storedItems.AddRange(STORAGEFILTERS.NOT_EDIBLE_SOLIDS);
            if (StoragePodOptions.Instance.podStoresFood)
            {
                storedItems.AddRange(STORAGEFILTERS.FOOD);
            }
            storage.storageFilters = storedItems;
            storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
            storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
            storage.allowSublimation = false;
            go.AddOrGet<CopyBuildingSettings>().copyGroupTag = GameTags.StorageLocker;
            go.AddOrGet<StorageLocker>();
            go.GetComponent<Storage>().capacityKg = StoragePodOptions.Instance.podCapacity;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
        }
    }
}
