using UnityEngine;
using TUNING;
using Harmony;
using PeterHan.PLib.Options;

namespace StoragePod
{
    class StoragePodConfig : IBuildingConfig
    {
        public const string ID = "StoragePodConfig";
        public const string DisplayName = "Storage Pod";
        public const string Description = "Now you, too, can store things in pods.";
        public static string Effect = $"Stores the Solid resources of your choosing. Compact and can be build anywhere.";

        public override BuildingDef CreateBuildingDef()
        {
            string id = ID;
            int width = 1;
            int height = 1;
            string anim = "storagePod_kanim";
            int hitpoints = 30;
            float construction_time = 10f;
            float[] tieR4 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
            string[] construction_mats = MATERIALS.REFINED_METALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.Anywhere;
            EffectorValues none = NOISE_POLLUTION.NONE;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tieR4, construction_mats, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER1, none, 0.2f);
            buildingDef.Floodable = false;
            buildingDef.AudioCategory = "Metal";
            buildingDef.Overheatable = false;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            SoundEventVolumeCache.instance.AddVolume("storagelocker_kanim", "StorageLocker_Hit_metallic_low", NOISE_POLLUTION.NOISY.TIER1);
            Prioritizable.AddRef(go);
            Storage storage = go.AddOrGet<Storage>();
            storage.showInUI = true;
            storage.allowItemRemoval = true;
            storage.showDescriptor = true;
            storage.storageFilters = STORAGEFILTERS.NOT_EDIBLE_SOLIDS;
            storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
            storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
            go.AddOrGet<CopyBuildingSettings>().copyGroupTag = GameTags.StorageLocker;
            go.AddOrGet<StorageLocker>();
            var config = POptions.ReadSettings<StoragePodOptions>();
            if (config == null)
            {
                POptions.WriteSettings(new StoragePodOptions());
                config = new StoragePodOptions();
            }
            go.GetComponent<Storage>().capacityKg = config.podCapacity;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
        }
    }
}
