using UnityEngine;
using TUNING;
using static ExpandedLights.LightSystemPatch;
using Harmony;

namespace ExpandedLights
{
    class FloodlightConfig: IBuildingConfig
    {
        public const string Id = "Floodlight";
        public const string DisplayName = "Floodlight";
        public const string Description = "It's called a floodlight because 'deluge light' didn't quite have the same ring.";
        public static string Effect = $"Lights a large area.";

        public override BuildingDef CreateBuildingDef()
        {
            int width = 1;
            int height = 1;
            string anim = "ceilinglight_kanim";
            int hitpoints = 10;
            float construction_time = BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER2;
            float[] tieR1 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER1;
            string[] allMetals = MATERIALS.REFINED_METALS;
            float melting_point = 800f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFoundationRotatable;
            EffectorValues none = NOISE_POLLUTION.NONE;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(Id, width, height, anim, hitpoints, construction_time, tieR1, allMetals, melting_point, build_location_rule, BUILDINGS.DECOR.NONE, none, 0.2f);
            buildingDef.RequiresPowerInput = true;
            buildingDef.PermittedRotations = PermittedRotations.R360;
            buildingDef.EnergyConsumptionWhenActive = 40f;
            buildingDef.SelfHeatKilowattsWhenActive = 0.6f;
            buildingDef.ViewMode = OverlayModes.Light.ID;
            buildingDef.AudioCategory = "Metal";
            return buildingDef;
          }

         public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            LightShapePreview lightShapePreview = go.AddComponent<LightShapePreview>();
            lightShapePreview.lux = 6000;
            lightShapePreview.radius = 16f;
            switch (go.AddOrGet<Rotatable>().GetOrientation())
            {
                case Orientation.R90:
                    lightShapePreview.shape = (LightShape)ExtendedLightShapes.ConeLeft;
                    break;
                case Orientation.R180:
                    lightShapePreview.shape = (LightShape)ExtendedLightShapes.ConeUp;
                    break;
                case Orientation.R270:
                    lightShapePreview.shape = (LightShape)ExtendedLightShapes.ConeRight;
                    break;
                case Orientation.Neutral:
                    lightShapePreview.shape = LightShape.Cone;
                    break;
                default:
                    lightShapePreview.shape = LightShape.Circle;
                    break;
            }
            //lightShapePreview.shape = (LightShape)ExtendedLightShapes.ConeLeft;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.LightSource, false);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGet<LoopingSounds>();
            Light2D light2D = go.AddOrGet<Light2D>();
            light2D.overlayColour = LIGHT2D.CEILINGLIGHT_OVERLAYCOLOR;
            light2D.Color = LIGHT2D.CEILINGLIGHT_COLOR;
            light2D.Range = 16f;
            light2D.Angle = 2.6f;
            light2D.Direction = LIGHT2D.CEILINGLIGHT_DIRECTION;
            light2D.Offset = LIGHT2D.CEILINGLIGHT_OFFSET;
            light2D.shape = LightShape.Cone;
            light2D.drawOverlay = true;
            light2D.Lux = 6000;
            go.AddOrGetDef<LightController.Def>();
            go.AddOrGet<RotatableLight>();
        }
    }
}
