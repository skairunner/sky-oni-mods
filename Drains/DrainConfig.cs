using TUNING;
using UnityEngine;

namespace Drains
{
    internal class DrainConfig : IBuildingConfig
    {
        public const string Id = "Drain";
        public const string DisplayName = "Drain";
        public const string Description = "9 out of 10 plumbers recommend Drain® for its uncloggability. Our new Drain® companion product, Clog-Be-Gone™, will hit shelves soon. Now less likely to vaporize unexpectedly during normal operation!";
        public static string Effect = "Slowly drains liquids into a pipe.";
        public static float[] MASS = BUILDINGS.CONSTRUCTION_MASS_KG.TIER2;

        public override BuildingDef CreateBuildingDef()
        {
            var def = BuildingTemplates.CreateBuildingDef(
                Id,
                1,
                1,
                DrainOptions.Instance.UseSolidDrain ? "solidDrain_kanim" : "drain_kanim",
                BUILDINGS.HITPOINTS.TIER2,
                30f,
                MASS,
                MATERIALS.ALL_METALS,
                1600f,
                BuildLocationRule.Tile,
                BUILDINGS.DECOR.PENALTY.TIER0,
                NOISE_POLLUTION.NONE
            );
            BuildingTemplates.CreateFoundationTileDef(def);
            def.UseStructureTemperature = false;
            def.Floodable = false;
            def.Entombable = false;
            def.Overheatable = false;
            def.UseStructureTemperature = false;
            def.AudioCategory = "Metal";
            def.AudioSize = "small";
            def.SceneLayer = Grid.SceneLayer.TileMain;
            def.IsFoundation = true;
            def.EnergyConsumptionWhenActive = 0f;
            def.ExhaustKilowattsWhenActive = 0f;
            def.SelfHeatKilowattsWhenActive = 0f;
            def.UtilityOutputOffset = new CellOffset(0, 0);
            def.OutputConduitType = ConduitType.Liquid;
            def.ViewMode = OverlayModes.LiquidConduits.ID;
            def.PermittedRotations = PermittedRotations.Unrotatable;
            def.ObjectLayer = ObjectLayer.Building;
            def.AudioSize = "small";
            if (DrainOptions.Instance.UseSolidDrain)
            {
                def.isSolidTile = true;
            }
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            // varioius configs stolen from meshtile
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            var sco = go.AddOrGet<SimCellOccupier>();
            if (DrainOptions.Instance.UseSolidDrain)
            {
                sco.notifyOnMelt = true;
                sco.doReplaceElement = true;
            }
            else
            {
                sco.doReplaceElement = false;
            }
            go.AddOrGet<TileTemperature>();
            go.AddOrGet<BuildingHP>().destroyOnDamaged = true;
            // where you add the state machine, i think
            go.AddOrGet<Drain>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            BuildingTemplates.DoPostConfigure(go);
            
            GeneratedBuildings.RemoveLoopingSounds(go);
            // MeshTile stuff
            go.GetComponent<KPrefabID>().AddTag(GameTags.FloorTiles);
//            go.GetComponent<ZoneTile>();

            // Pump stuff
            go.AddOrGet<Storage>().capacityKg = 1f;
            var elementConsumer = go.AddOrGet<ElementConsumer>();
            elementConsumer.configuration = ElementConsumer.Configuration.AllLiquid;
            elementConsumer.consumptionRate = 0.1f;
            elementConsumer.storeOnConsume = true;
            elementConsumer.showInStatusPanel = false;
            elementConsumer.consumptionRadius = 1;
            var conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Liquid;
            conduitDispenser.alwaysDispense = true;
            conduitDispenser.elementFilter = null;

            // add anim
            go.GetComponent<KBatchedAnimController>().initialAnim = "built";
            
            if (DrainOptions.Instance.UseSolidDrain)
            {
                elementConsumer.sampleCellOffset = new Vector3(0, 1f);
            }
        }
    }
}
