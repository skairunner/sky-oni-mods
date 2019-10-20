using TUNING;
using UnityEngine;

namespace CarbonRevolution
{
    public class RefinedCoalGeneratorConfig : IBuildingConfig
    {
        public const string ID = "RefinedCoalGenerator";
        public const string NAME = "CFB Generator";
        public const string DESC = "Any allegations that a CFB Generator is just the boiler of a Oil Refinery are pure libel, slander and hearsay.";
        public const string EFFECT = "Burns Refined Carbon at a very high temperature, producing large amounts of power.";
        
        private const float REFINED_BURN_RATE = 1f;
        private const float REFINED_CAPACITY = 600f;
        public const float CO2_OUTPUT_TEMPERATURE = 383.15f;

        public override BuildingDef CreateBuildingDef()
        {
            var width = 3;
            var height = 4;
            var anim = "cfbgen_kanim";
            var hitpoints = 100;
            var construction_time = 120f;
            float[] tieR5_1 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER5;
            string[] mats = new []
            {
                SimHashes.Ceramic.ToString()
            };
            var melting_point = 2400f;
            var build_location_rule = BuildLocationRule.OnFloor;
            var tieR5_2 = NOISE_POLLUTION.NOISY.TIER5;
            var buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoints, construction_time,
                tieR5_1, mats, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER2, tieR5_2);

            buildingDef.UtilityOutputOffset = new CellOffset(-1, 3);
            buildingDef.OutputConduitType = ConduitType.Gas;

            buildingDef.Overheatable = true;
            buildingDef.OverheatTemperature = 73.1f + 75f;
            buildingDef.GeneratorWattageRating = 2000f;
            buildingDef.GeneratorBaseCapacity = 20000f;
            buildingDef.ExhaustKilowattsWhenActive = 40f;
            buildingDef.SelfHeatKilowattsWhenActive = 40f;
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.AudioCategory = "HollowMetal";
            buildingDef.AudioSize = "large";
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
            var energyGenerator = go.AddOrGet<EnergyGenerator>();
            energyGenerator.formula = EnergyGenerator.CreateSimpleFormula(SimHashes.RefinedCarbon.CreateTag(), 1f, 600f,
                SimHashes.CarbonDioxide, 0.25f, true, CellOffset.none, 700f);
            energyGenerator.meterOffset = Meter.Offset.Behind;
            energyGenerator.SetSliderValue(50f, 0);
            energyGenerator.powerDistributionOrder = 9;
            var storage = go.AddOrGet<Storage>();
            storage.capacityKg = 600f;
            go.AddOrGet<LoopingSounds>();
            Prioritizable.AddRef(go);
            var manualDeliveryKg = go.AddOrGet<ManualDeliveryKG>();
            manualDeliveryKg.SetStorage(storage);
            manualDeliveryKg.requestedItemTag = SimHashes.RefinedCarbon.CreateTag();
            manualDeliveryKg.capacity = storage.capacityKg;
            manualDeliveryKg.refillMass = 100f;
            manualDeliveryKg.choreTypeIDHash = Db.Get().ChoreTypes.PowerFetch.IdHash;
            Tinkerable.MakePowerTinkerable(go);

            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Gas;
            conduitDispenser.invertElementFilter = false;
            conduitDispenser.elementFilter = new []
            {
                SimHashes.CarbonDioxide
            };
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_0);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_0);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_0_0);
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}
