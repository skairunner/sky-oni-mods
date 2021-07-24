using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace StoragePod
{
    internal class CoolPodConfig : IBuildingConfig
    {
        public const string ID = "CoolPodConfig";
        public const string DisplayName = "Cool Pod";
        public const string Description = "Snack pod snack pod snack pod!";

        public static string Effect =
            "Stores the food of your choosing. Compact and can be built anywhere.";

        public override BuildingDef CreateBuildingDef()
        {
            var id = ID;
            var width = 1;
            var height = 1;
            var anim = "coolPod_kanim";
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
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 60f;
            buildingDef.ExhaustKilowattsWhenActive = 0.25f;
            buildingDef.LogicOutputPorts = new List<LogicPorts.Port>()
            {
                LogicPorts.Port.OutputPort(FilteredStorage.FULL_PORT_ID, new CellOffset(0, 0), (string) STRINGS.BUILDINGS.PREFABS.REFRIGERATOR.LOGIC_PORT, (string) STRINGS.BUILDINGS.PREFABS.REFRIGERATOR.LOGIC_PORT_ACTIVE, (string) STRINGS.BUILDINGS.PREFABS.REFRIGERATOR.LOGIC_PORT_INACTIVE, false, false)
            };
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
            List<Tag> storedItems = new List<Tag>();
            storedItems.AddRange(STORAGEFILTERS.FOOD);
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
            storage.storageFilters = storedItems;
            storage.storageFullMargin = STORAGE.STORAGE_LOCKER_FILLED_MARGIN;
            storage.fetchCategory = Storage.FetchCategory.GeneralStorage;
            //storage.allowSublimation = false;
            go.AddOrGet<CopyBuildingSettings>().copyGroupTag = GameTags.StorageLocker;
            go.AddOrGet<Refrigerator>();
            go.GetComponent<Storage>().capacityKg = StoragePodOptions.Instance.coolPodCapacity;
            Prioritizable.AddRef(go);
            go.AddOrGet<UserNameable>();
            go.AddOrGet<DropAllWorkable>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<StorageController.Def>();
        }
    }
}
