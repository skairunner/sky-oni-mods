using TUNING;
using UnityEngine;

namespace Radiator
{
    internal class RadiatorConfig : IBuildingConfig
    {
        public const string Id = "Radiator";
        public const string DisplayName = "Radiator";

        public const string Description =
            "A space-age space heater, a Radiator passively absorbs heat from liquid pipes through it and radiates it away into space. The hotter it is, the faster it cools itself. It can also be used to equalize the temperature of the piped liquid and a liquid or gas environment.";

        public static string Effect =
            "Warmed up by liquids piped through it, and radiates heat into space. More effective the hotter it is.";

        public static float[] MASS = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;

        public override BuildingDef CreateBuildingDef()
        {
            var def = BuildingTemplates.CreateBuildingDef(
                Id,
                1,
                4,
                "radiator_kanim",
                BUILDINGS.HITPOINTS.TIER1,
                BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER3,
                MASS,
                MATERIALS.ANY_BUILDABLE,
                BUILDINGS.MELTING_POINT_KELVIN.TIER4,
                BuildLocationRule.Tile,
                BUILDINGS.DECOR.NONE,
                NOISE_POLLUTION.NONE
            );
            def.Floodable = false;
            def.MaterialCategory = MATERIALS.REFINED_METALS;
            def.AudioCategory = "HollowMetal";
            def.Overheatable = false;
            def.Entombable = true;
            def.IsFoundation = true;
            def.UtilityInputOffset = new CellOffset(0, 0);
            def.UtilityOutputOffset = new CellOffset(0, 3);
            def.OutputConduitType = ConduitType.Liquid;
            def.InputConduitType = ConduitType.Liquid;
            def.ViewMode = OverlayModes.LiquidConduits.ID;
            def.PermittedRotations = PermittedRotations.FlipV;
            def.TileLayer = ObjectLayer.Building;
            def.AudioSize = "small";
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            // where you add the state machine, i think
            go.AddOrGet<Radiator>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<KBatchedAnimController>().initialAnim = "closed";
            Object.DestroyImmediate(go.GetComponent<RequireInputs>());
            Object.DestroyImmediate(go.GetComponent<ConduitConsumer>());
            Object.DestroyImmediate(go.GetComponent<ConduitDispenser>());
            BuildingTemplates.DoPostConfigure(go);
        }
    }
}