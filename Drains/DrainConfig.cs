using TUNING;
using UnityEngine;

namespace Drains
{
    internal class DrainConfig : IBuildingConfig
    {
        public const string Id = "Drain";
        public const string DisplayName = "Drain";
        public const string Description = "";
        public static string Effect = "Slowly drains liquids into a pipe.";

        public override BuildingDef CreateBuildingDef()
        {
            var def = BuildingTemplates.CreateBuildingDef(
                Id,
                1,
                1,
                "drain_kanim",
                BUILDINGS.HITPOINTS.TIER1,
                BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER2,
                BUILDINGS.CONSTRUCTION_MASS_KG.TIER2,
                MATERIALS.ALL_METALS,
                1600f,
                BuildLocationRule.Tile,
                BUILDINGS.DECOR.PENALTY.TIER0,
                NOISE_POLLUTION.NONE
            );

            BuildingTemplates.CreateFoundationTileDef(def);
            def.DefaultAnimState = "built";
            def.SceneLayer = Grid.SceneLayer.TileMain;
            def.ViewMode = OverlayModes.LiquidConduits.ID;
            def.UtilityOutputOffset = new CellOffset(0, 0);
            def.OutputConduitType = ConduitType.Liquid;
            def.Floodable = false;
            def.Entombable = false;
            def.Overheatable = false;
            def.UseStructureTemperature = false;

            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            var settings = DrainSettings.GetSettings() ?? new DrainSettings();

            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            go.AddComponent<TileTemperature>();
            go.AddOrGet<SimCellOccupier>().doReplaceElement = false;
            go.AddOrGet<BuildingHP>().destroyOnDamaged = true;

            BuildingTemplates.CreateDefaultStorage(go);

            var elementConsumer = go.AddOrGet<ElementConsumer>();
            elementConsumer.configuration = ElementConsumer.Configuration.AllLiquid;
            elementConsumer.consumptionRate = settings.flowRate;
            elementConsumer.storeOnConsume = true;
            elementConsumer.showInStatusPanel = false;

            var conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Liquid;
            conduitDispenser.alwaysDispense = true;

            go.AddOrGet<Drain>();
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            go.GetComponent<KPrefabID>().AddTag(GameTags.FloorTiles);
            go.AddComponent<ZoneTile>();
            go.AddComponent<SimTemperatureTransfer>();
        }
    }
}