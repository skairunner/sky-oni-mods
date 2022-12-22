using TUNING;
using UnityEngine;

namespace ExpandedLights
{
    internal class LEDLightConfig : IBuildingConfig
    {
        public const string Id = "LEDLight";
        public const string DisplayName = "LED Light";

        public const string Description =
            "The only thing holding back this LED light is its high costs and lack of determination. It does, however, always give its one hundred percent.";

        public const int lux = 1500;
        public const float range = 6f;

        public static string Effect = "Emits light but no heat. Can be built anywhere.";

        public override BuildingDef CreateBuildingDef()
        {
            var width = 1;
            var height = 1;
            var anim = "ledLight_kanim";
            var hitpoints = 10;
            var construction_time = BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER2;
            var melting_point = 800f;
            var build_location_rule = BuildLocationRule.Anywhere;
            var none = NOISE_POLLUTION.NONE;

            var construction_materials = new[]
            {
                "RefinedMetal",
                "Plastic"
            };

            var construction_costs = new[]
            {
                50f,
                50f
            };

            var decor = new EffectorValues
            {
                amount = 5,
                radius = 3
            };

            var buildingDef = BuildingTemplates.CreateBuildingDef(Id, width, height, anim, hitpoints,
                construction_time, construction_costs, construction_materials, melting_point, build_location_rule,
                decor, none);
            buildingDef.RequiresPowerInput = true;
            buildingDef.PermittedRotations = PermittedRotations.Unrotatable;
            buildingDef.Floodable = false;
            buildingDef.EnergyConsumptionWhenActive = 2f;
            buildingDef.SelfHeatKilowattsWhenActive = 0f;
            buildingDef.ViewMode = OverlayModes.Light.ID;
            buildingDef.ObjectLayer = ObjectLayer.Building;
            buildingDef.SceneLayer = Grid.SceneLayer.BuildingFront;
            buildingDef.AudioCategory = "Metal";
            return buildingDef;
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            var lightShapePreview = go.AddComponent<LightShapePreview>();
            lightShapePreview.lux = lux;
            lightShapePreview.radius = range;
            lightShapePreview.shape = ExpandedLightsPatch.SmoothCircle.KleiLightShape;
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
            light2D.Offset = Vector2.zero;
            light2D.shape = ExpandedLightsPatch.SmoothCircle.KleiLightShape;
            light2D.drawOverlay = true;
            light2D.Lux = lux;
            go.AddOrGetDef<LightController.Def>();
        }
    }
}
