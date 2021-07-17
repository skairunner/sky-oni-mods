using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace DiseasesReimagined
{
    public class UVCleanerConfig : IBuildingConfig
    {
        public const string ID = "UVCleaner";
        public static LocString DISPLAY_NAME = "UV Cleaner";
        public static LocString DESCRIPTION = "The sun is a deadly laser, blindingly bright and prone to inducing sunburn. Naturally, some duplicants decided to bottle it for water sanitization purposes.";
        public static LocString EFFECT = "Sterilizes liquids.";

        public static int LUX = 500;
        public static int RADIUS = 3;

        private static readonly List<Storage.StoredItemModifier> StoredItemModifiers =
            new List<Storage.StoredItemModifier>
            {
                Storage.StoredItemModifier.Hide,
                Storage.StoredItemModifier.Insulate,
                Storage.StoredItemModifier.Seal
            };

        public override BuildingDef CreateBuildingDef()
        {
            var width = 3;
            var height = 3;
            var anim = "uvcleaner_kanim";
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
            buildingDef.EnergyConsumptionWhenActive = 320f;
            buildingDef.SelfHeatKilowattsWhenActive = 6.0f;
            buildingDef.InputConduitType = ConduitType.Liquid;
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.Floodable = false;
            buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(
                new CellOffset(1, 1));
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
            var lightShapePreview = go.AddComponent<LightShapePreview>();
            lightShapePreview.lux = LUX;
            lightShapePreview.radius = RADIUS;
            lightShapePreview.shape = LightShape.Circle;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LogicOperationalController>();
            go.AddOrGetDef<PoweredActiveController.Def>();
            var light2D = go.AddOrGet<Light2D>();
            light2D.overlayColour = LIGHT2D.FLOORLAMP_OVERLAYCOLOR;
            light2D.Color = new Color(120/255f, 100/255f, 120/255f);
            light2D.Range = RADIUS;
            light2D.Angle = 2.6f;
            light2D.Direction = LIGHT2D.FLOORLAMP_DIRECTION;
            light2D.Offset = new Vector2(0.05f, 2.5f);
            light2D.shape = BuildingsPatch.uvlight.KleiLightShape;
            light2D.drawOverlay = false;
            light2D.Lux = LUX;
            go.AddOrGetDef<LightController.Def>();
        }
    }
}
