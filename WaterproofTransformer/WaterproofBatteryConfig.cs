using TUNING;
using UnityEngine;

namespace WaterproofTransformer { 
    public class WaterproofBatteryConfig : BaseBatteryConfig
    {
        public const string ID = "ScubaBattery";
        public const string DisplayName = "Waterproof Battery";
        public const string Description = "This battery has been to the Marinara Trench. It was full of tomato-based sauce.";
        public static string Effect = $"It's a smart battery - but waterproof.";

        private static readonly LogicPorts.Port[] OUTPUT_PORTS = new LogicPorts.Port[1]
        {
            LogicPorts.Port.OutputPort(BatterySmart.PORT_ID, new CellOffset(0, 0), (string) STRINGS.BUILDINGS.PREFABS.BATTERYSMART.LOGIC_PORT, (string) STRINGS.BUILDINGS.PREFABS.BATTERYSMART.LOGIC_PORT_ACTIVE, (string) STRINGS.BUILDINGS.PREFABS.BATTERYSMART.LOGIC_PORT_INACTIVE, true, false)
        };

        public override BuildingDef CreateBuildingDef()
        {
            int width = 2;
            int height = 2;
            int hitpoints = 30;
            string anim = "waterbattery_kanim";
            float construction_time = 60f;
            var construction_mass = new[] { 150f, 50f };
            var construction_mats = new string[] { "RefinedMetal", "Transparent" };
            float melting_point = 800f;
            float exhaust_temperature_active = 0.0f;
            float self_heat_kilowatts_active = 0.5f;
            EffectorValues tieR1 = TUNING.NOISE_POLLUTION.NOISY.TIER1;
            BuildingDef buildingDef = this.CreateBuildingDef(ID, width, height, hitpoints, anim, construction_time, construction_mass, construction_mats, melting_point, exhaust_temperature_active, self_heat_kilowatts_active, TUNING.BUILDINGS.DECOR.PENALTY.TIER2, tieR1);
            SoundEventVolumeCache.instance.AddVolume("batterymed_kanim", "Battery_med_rattle", TUNING.NOISE_POLLUTION.NOISY.TIER2);
            buildingDef.Floodable = false;
            return buildingDef;
        }

        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, (LogicPorts.Port[])null, OUTPUT_PORTS);
        }

        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            GeneratedBuildings.RegisterLogicPorts(go, (LogicPorts.Port[])null, OUTPUT_PORTS);
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            BatterySmart batterySmart = go.AddOrGet<BatterySmart>();
            batterySmart.capacity = 20000f;
            batterySmart.joulesLostPerSecond = 0.6666667f;
            batterySmart.powerSortOrder = 1000;
            GeneratedBuildings.RegisterLogicPorts(go, (LogicPorts.Port[])null, OUTPUT_PORTS);
            base.DoPostConfigureComplete(go);
        }
    }

}
