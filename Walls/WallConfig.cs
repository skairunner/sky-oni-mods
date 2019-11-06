using System;
using UnityEngine;
using TUNING;

namespace Walls
{
    class WallConfig : IBuildingConfig
    {
        public const string ID = "DecorativeWall";
        public const string DisplayName = "Wall";
        public const string Description = "";
        public const string Effect = "";


        public override BuildingDef CreateBuildingDef()
        {
            int width = 1;
            int height = 1;
            string anim = "decorative_walls_kanim";
            int hitpoints = 30;
            float construction_time = 30f;
            float[] tieR4 = BUILDINGS.CONSTRUCTION_MASS_KG.TIER4;
            string[] rawMinerals = MATERIALS.RAW_MINERALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.NotInTiles;
            EffectorValues none = NOISE_POLLUTION.NONE;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoints, construction_time, tieR4, rawMinerals, melting_point, build_location_rule, DECOR.NONE, none, 0.2f);
            buildingDef.Entombable = false;
            buildingDef.Floodable = false;
            buildingDef.Overheatable = false;
            buildingDef.AudioCategory = "Metal";
            buildingDef.AudioSize = "small";
            buildingDef.BaseTimeUntilRepair = -1f;
            buildingDef.DefaultAnimState = "off";
            buildingDef.ObjectLayer = ObjectLayer.Backwall;
            buildingDef.SceneLayer = Grid.SceneLayer.Backwall;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            GeneratedBuildings.MakeBuildingAlwaysOperational(go);
            go.AddComponent<ZoneTile>();
            BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GeneratedBuildings.RemoveLoopingSounds(go);
        }
    }
}
