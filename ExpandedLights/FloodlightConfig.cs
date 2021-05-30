using TUNING;
using UnityEngine;

namespace ExpandedLights
{
    internal class FloodlightConfig : IBuildingConfig
    {
        public const string Id = "Floodlight";
        public const string DisplayName = "Floodlight";

        public const string Description =
            "It's called a floodlight because 'deluge light' didn't quite have the same ring.";

        public const int lux = 2000;
        public const float range = 16f;

        public static string Effect = "Brightly illuminates a large area.";

        public override BuildingDef CreateBuildingDef()
        {
            var width = 1;
            var height = 1;
            var anim = "floodlight_kanim";
            var hitpoints = 10;
            var construction_time = BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER2;
            float[] tieR1 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;
            string[] allMetals = MATERIALS.REFINED_METALS;
            var melting_point = 800f;
            var build_location_rule = BuildLocationRule.OnFoundationRotatable;
            var none = NOISE_POLLUTION.NONE;
            var buildingDef = BuildingTemplates.CreateBuildingDef(Id, width, height, anim, hitpoints,
                construction_time, tieR1, allMetals, melting_point, build_location_rule, BUILDINGS.DECOR.PENALTY.TIER5,
                none);
            buildingDef.RequiresPowerInput = true;
            buildingDef.PermittedRotations = PermittedRotations.R360;
            buildingDef.EnergyConsumptionWhenActive = 30f;
            buildingDef.SelfHeatKilowattsWhenActive = 0.5f;
            buildingDef.ViewMode = OverlayModes.Light.ID;
            buildingDef.AudioCategory = "Metal";
            return buildingDef;
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            var lightShapePreview = go.AddComponent<LightShapePreview>();
            lightShapePreview.lux = lux;
            lightShapePreview.radius = range;
            lightShapePreview.shape = ExpandedLightsPatch.Beam5.GetKLightShape();
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.LightSource);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<EnergyConsumer>();
            go.AddOrGet<LoopingSounds>();
            var light2D = go.AddOrGet<Light2D>();
            light2D.overlayColour = LIGHT2D.CEILINGLIGHT_OVERLAYCOLOR;
            light2D.Color = LIGHT2D.CEILINGLIGHT_COLOR;
            light2D.Range = range;
            light2D.Angle = 2.6f;
            light2D.Direction = LIGHT2D.CEILINGLIGHT_DIRECTION;
            light2D.Offset = LIGHT2D.CEILINGLIGHT_OFFSET;
            light2D.shape = ExpandedLightsPatch.Beam5.GetKLightShape();
            light2D.drawOverlay = true;
            light2D.Lux = lux;
            go.AddOrGetDef<LightController.Def>();
        }
    }
}