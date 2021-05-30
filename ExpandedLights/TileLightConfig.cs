using TUNING;
using UnityEngine;

namespace ExpandedLights
{
    internal class TileLightConfig : IBuildingConfig
    {
        public const string Id = "TileLight";
        public const string DisplayName = "Tile Light";

        public const string Description =
            "These tiny lights are the real MVP, illuminating places that other lights don't dare to go.";

        public const int lux = 1800;
        public const float range = 8f;

        public static string Effect =
            "Emits light. Can be built behind most other buildings, except Drywall and Tempshift Plates.";

        public override BuildingDef CreateBuildingDef()
        {
            var width = 1;
            var height = 1;
            var anim = "tileLight_kanim";
            var hitpoints = 10;
            var construction_time = BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER2;
            float[] tieR1 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER1;
            string[] allMetals = MATERIALS.PLASTICS;
            var melting_point = 800f;
            var build_location_rule = BuildLocationRule.Anywhere;
            var none = NOISE_POLLUTION.NONE;
            var buildingDef = BuildingTemplates.CreateBuildingDef(Id, width, height, anim, hitpoints,
                construction_time, tieR1, allMetals, melting_point, build_location_rule, BUILDINGS.DECOR.NONE, none);
            buildingDef.RequiresPowerInput = true;
            buildingDef.PermittedRotations = PermittedRotations.R360;
            buildingDef.EnergyConsumptionWhenActive = 5f;
            buildingDef.SelfHeatKilowattsWhenActive = 0.1f;
            buildingDef.ViewMode = OverlayModes.Light.ID;
            buildingDef.ObjectLayer = ObjectLayer.Backwall;
            buildingDef.SceneLayer = Grid.SceneLayer.TileMain;
            buildingDef.AudioCategory = "Metal";
            buildingDef.OverheatTemperature = BUILDINGS.OVERHEAT_TEMPERATURES.HIGH_2;
            return buildingDef;
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            var lightShapePreview = go.AddComponent<LightShapePreview>();
            lightShapePreview.lux = lux;
            lightShapePreview.radius = range;
            lightShapePreview.shape = ExpandedLightsPatch.OffsetSemi.GetKLightShape();
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.LightSource);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LoopingSounds>();
            var light2D = go.AddOrGet<Light2D>();
            light2D.overlayColour = LIGHT2D.CEILINGLIGHT_OVERLAYCOLOR;
            light2D.Color = LIGHT2D.CEILINGLIGHT_COLOR;
            light2D.Range = range;
            light2D.Angle = 2.6f;
            light2D.Direction = LIGHT2D.CEILINGLIGHT_DIRECTION;
            light2D.Offset = LIGHT2D.CEILINGLIGHT_OFFSET;
            light2D.shape = ExpandedLightsPatch.OffsetSemi.GetKLightShape();
            light2D.drawOverlay = true;
            light2D.Lux = lux;
            go.AddOrGetDef<LightController.Def>();
        }
    }
}