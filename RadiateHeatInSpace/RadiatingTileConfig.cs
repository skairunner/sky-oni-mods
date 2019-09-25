using System;
using TUNING;
using UnityEngine;

namespace RadiateHeat
{
    class RadiatorTileConfig : IBuildingConfig
    {
        public const string Id = "RadiativeTile";
        public const string DisplayName = "Radiative Tile";
        public const string Description = "Perfect for building a sauna, if the sauna were in space. On second thought, not so perfect for building a sauna.";
        public static string Effect = $"A tile that passively radiates heat to space. More effective the hotter it is.";
        public static float[] MASS = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;

        public override BuildingDef CreateBuildingDef()
        {
            var def = BuildingTemplates.CreateBuildingDef(
                id: Id,
                width: 1,
                height: 1,
                anim: "floor_basic_kanim",
                hitpoints: BUILDINGS.HITPOINTS.TIER1,
                construction_time: BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER0,
                construction_mass: MASS,
                construction_materials: MATERIALS.ANY_BUILDABLE,
                melting_point: BUILDINGS.MELTING_POINT_KELVIN.TIER4,
                build_location_rule: BuildLocationRule.Tile,
                decor: BUILDINGS.DECOR.NONE,
                noise: NOISE_POLLUTION.NONE
            );
            BuildingTemplates.CreateFoundationTileDef(def);
            def.BlockTileAtlas = Assets.GetTextureAtlas("tiles_solid");
            def.BlockTilePlaceAtlas = Assets.GetTextureAtlas("tiles_solid_place");
            def.BlockTileMaterial = Assets.GetMaterial("tiles_solid");
            def.DecorBlockTileInfo = Assets.GetBlockTileDecorInfo("tiles_solid_tops_info");
            def.DecorPlaceBlockTileInfo = Assets.GetBlockTileDecorInfo("tiles_solid_tops_place_info");
            def.ConstructionOffsetFilter = BuildingDef.ConstructionOffsetFilter_OneDown;
            def.MaterialCategory = MATERIALS.ANY_BUILDABLE;
            def.AudioCategory = "Metal";
            def.AudioSize = "small";
            def.BaseTimeUntilRepair = -1f;
            def.Overheatable = false;
            def.Entombable = false;
            def.IsFoundation = true;
            def.Floodable = false;
            def.DragBuild = true;
            def.TileLayer = ObjectLayer.FoundationTile;
            def.SceneLayer = Grid.SceneLayer.TileMain;
            def.isKAnimTile = true;
            def.isSolidTile = true;
            return def;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            // where you add the state machine, i think
            var rh = go.AddOrGet<RadiatesHeat>();
            rh.emissivity = .77f;
            rh.surface_area = 1f;
            // tile stuff
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
            SimCellOccupier simCellOccupier = go.AddOrGet<SimCellOccupier>();
            simCellOccupier.doReplaceElement = true;
            simCellOccupier.strengthMultiplier = 1.5f;
            simCellOccupier.movementSpeedMultiplier = DUPLICANTSTATS.MOVEMENT.BONUS_2;
            simCellOccupier.notifyOnMelt = true;
            go.AddOrGet<TileTemperature>();
            go.AddOrGet<KAnimGridTileVisualizer>().blockTileConnectorID = TileConfig.BlockTileConnectorID;
            go.AddOrGet<BuildingHP>().destroyOnDamaged = true;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            BuildingTemplates.DoPostConfigure(go);
            go.GetComponent<KPrefabID>().AddTag(GameTags.FloorTiles, false);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            base.DoPostConfigureUnderConstruction(go);
            go.AddOrGet<KAnimGridTileVisualizer>();
        }
    }
}
