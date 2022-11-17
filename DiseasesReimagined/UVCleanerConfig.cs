using PeterHan.PLib.Core;
using TUNING;
using UnityEngine;

namespace DiseasesReimagined
{
    public sealed class UVCleanerConfig : IBuildingConfig
    {
        public const string ID = "UVCleaner";
        public static LocString DISPLAY_NAME = "UV Cleaner";
        public static LocString DESCRIPTION = "The sun is a deadly laser, blindingly bright and prone to inducing sunburn. Naturally, some duplicants decided to bottle it for water sanitization purposes.";
        public static LocString EFFECT = "Removes almost all Germs from liquids. Emits UV radiation while running, which might burn Duplicants that get too close.";

        public static int LUX = 500;
        public static int RADIUS = 3;

        public override BuildingDef CreateBuildingDef()
        {
            var buildingDef = BuildingTemplates.CreateBuildingDef(ID, 3, 3, "uvcleaner_kanim",
                100, 120.0f, BUILDINGS.CONSTRUCTION_MASS_KG.TIER3, MATERIALS.REFINED_METALS,
                1600f, BuildLocationRule.OnFloor, BUILDINGS.DECOR.NONE, NOISE_POLLUTION.NOISY.
                TIER2);
            PGameUtils.CopySoundsToAnim("uvcleaner_kanim", "waterpurifier_kanim");
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
            buildingDef.OverheatTemperature = BUILDINGS.OVERHEAT_TEMPERATURES.NORMAL;
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
            defaultStorage.SetDefaultStoredItemModifiers(Storage.StandardInsulatedStorage);
            go.AddOrGet<UVCleaner>();
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            var lightShapePreview = go.AddOrGet<LightShapePreview>();
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
