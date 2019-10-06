﻿using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace DiseasesReimagined
{
    public class UVCleanerConfig : IBuildingConfig
    {
        public const string ID = "UVCleaner";
        public const string DisplayName = "UV Cleaner";
        public const string Description = "";
        public static string Effect = "Sterilizes liquids.";

        private static readonly List<Storage.StoredItemModifier> StoredItemModifiers =
            new List<Storage.StoredItemModifier>
            {
                Storage.StoredItemModifier.Hide,
                Storage.StoredItemModifier.Insulate,
                Storage.StoredItemModifier.Seal
            };

        public override BuildingDef CreateBuildingDef()
        {
            var width = 2;
            var height = 2;
            var anim = "liquidconditioner_kanim";
            var hitpoints = 100;
            var construction_time = 120f;
            float[] mass = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
            string[] mats = MATERIALS.REFINED_METALS;
            var melting_point = 1600f;
            var build_location_rule = BuildLocationRule.OnFloor;
            var tieR2 = NOISE_POLLUTION.NOISY.TIER2;
            var buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoints, construction_time,
                mass, mats, melting_point, build_location_rule, BUILDINGS.DECOR.NONE, tieR2);
            BuildingTemplates.CreateElectricalBuildingDef(buildingDef);
            buildingDef.EnergyConsumptionWhenActive = 480f;
            buildingDef.SelfHeatKilowattsWhenActive = 8.0f;
            buildingDef.InputConduitType = ConduitType.Liquid;
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.Floodable = false;
            buildingDef.PowerInputOffset = new CellOffset(1, 0);
            buildingDef.UtilityInputOffset = new CellOffset(0, 0);
            buildingDef.PermittedRotations = PermittedRotations.FlipH;
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
            buildingDef.OverheatTemperature = TUNING.BUILDINGS.OVERHEAT_TEMPERATURES.NORMAL;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LoopingSounds>();
            var conduitConsumer = go.AddOrGet<ConduitConsumer>();
            conduitConsumer.conduitType = ConduitType.Liquid;
            conduitConsumer.consumptionRate = 5f;
            var defaultStorage = BuildingTemplates.CreateDefaultStorage(go);
            defaultStorage.showInUI = true;
            defaultStorage.capacityKg = 2f * conduitConsumer.consumptionRate;
            defaultStorage.SetDefaultStoredItemModifiers(StoredItemModifiers);
            go.AddOrGet<UVCleaner>();
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, LogicOperationalController.INPUT_PORTS_1_1);
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}