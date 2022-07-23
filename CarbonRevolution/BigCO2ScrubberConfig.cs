using STRINGS;
using TUNING;
using UnityEngine;
using BUILDINGS = TUNING.BUILDINGS;

namespace CarbonRevolution
{
    public class BigCO2ScrubberConfig : IBuildingConfig
    {
        public const string ID = "BigCO2Scrubber";
        public const string NAME = "CO2 Crystallizer";

        public const string DESC =
            "Catalytics is reverse alchemy: a field of study focusing on converting precious, desirable resources into trash. The CO2 Crystallizer, for instance, serves as a philosopher's stone that catalyzes perfectly good Lime into Refined Carbon.";
        public static string EFFECT = $"Rapidly turns CO2 into {UI.PRE_KEYWORD}Refined Carbon{UI.PST_KEYWORD} at the cost of a small amount of {UI.PRE_KEYWORD}Lime{UI.PST_KEYWORD}.";
        
        private const float CO2_CONSUMPTION_RATE = 1f; // 1kg
        private const float LIME_CONSUMPTION_RATE = 0.005f; // 5g

        public override BuildingDef CreateBuildingDef()
        {
            var width = 2;
            var height = 2;
            var hitpoints = 30;
            var construction_time = 60f;
            float[] tieR2 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
            string[] rawMetals = MATERIALS.REFINED_METALS;
            var melting_point = 800f;
            var build_location_rule = BuildLocationRule.OnFloor;
            var tieR3 = NOISE_POLLUTION.NOISY.TIER3;
            var buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, "bigco2scrubber_kanim", hitpoints, construction_time,
                tieR2, rawMetals, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER1, tieR3);
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 360f;
            buildingDef.SelfHeatKilowattsWhenActive = 2f;
            buildingDef.ViewMode = OverlayModes.Oxygen.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.AudioSize = "large";
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(1, 1);
            buildingDef.PermittedRotations = PermittedRotations.FlipH;
            buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(1, 0));
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
            var defaultStorage = BuildingTemplates.CreateDefaultStorage(go);
            defaultStorage.showInUI = true;
            defaultStorage.capacityKg = 1000f;
            defaultStorage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
            go.AddOrGet<AirFilter>().filterTag = GameTagExtensions.Create(SimHashes.Lime);
            var elementConsumer = go.AddOrGet<PassiveElementConsumer>();
            elementConsumer.elementToConsume = SimHashes.CarbonDioxide;
            elementConsumer.consumptionRate = CO2_CONSUMPTION_RATE * 2;
            elementConsumer.capacityKG = CO2_CONSUMPTION_RATE * 2;
            elementConsumer.consumptionRadius = 6;
            elementConsumer.showInStatusPanel = true;
            elementConsumer.sampleCellOffset = new Vector3(0.0f, 0.0f, 0.0f);
            elementConsumer.isRequired = false;
            elementConsumer.storeOnConsume = true;
            elementConsumer.showDescriptor = false;
            
            // Add the bit where you deliver lime
            Prioritizable.AddRef(go);
            ManualDeliveryKG manualDeliveryKg = go.AddOrGet<ManualDeliveryKG>();
            manualDeliveryKg.SetStorage(defaultStorage);
            manualDeliveryKg.requestedItemTag = new Tag("Lime");
            manualDeliveryKg.capacity = 1f;
            manualDeliveryKg.refillMass = .5f;
            manualDeliveryKg.choreTypeIDHash = Db.Get().ChoreTypes.FetchCritical.IdHash;

            // Drop refined carbon
            ElementDropper elementDropper = go.AddComponent<ElementDropper>();
            elementDropper.emitMass = 10f;
            elementDropper.emitTag = SimHashes.RefinedCarbon.CreateTag();
            elementDropper.emitOffset = new Vector3(0.0f, 0.0f, 0.0f);
            
            // Define conversions
            var elementConverter = go.AddOrGet<ElementConverter>();
            elementConverter.consumedElements = new ElementConverter.ConsumedElement[2]
            {
                new ElementConverter.ConsumedElement(GameTagExtensions.Create(SimHashes.Lime), LIME_CONSUMPTION_RATE),
                new ElementConverter.ConsumedElement(GameTagExtensions.Create(SimHashes.CarbonDioxide), CO2_CONSUMPTION_RATE)
            };
            elementConverter.outputElements = new ElementConverter.OutputElement[1]
            {
                new ElementConverter.OutputElement(CO2_CONSUMPTION_RATE, SimHashes.RefinedCarbon, 0.0f, false, true)
            };
            go.AddOrGet<KBatchedAnimController>().randomiseLoopedOffset = true;
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}