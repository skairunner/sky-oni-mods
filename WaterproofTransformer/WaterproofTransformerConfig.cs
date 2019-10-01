using TUNING;
using UnityEngine;

namespace WaterproofTransformer
{
    public class WaterproofTransformerConfig : IBuildingConfig
    {
        public const string ID = "ScubaTransformer";
        public const string DisplayName = "Waterproof Transformer";
        public const string Description = "This transformer isn't afraid of deep water, or sharks.";

        public static string Effect =
            $"Connect Batteries on the large side to act as a valve and prevent Wires from drawing more than 2 kW.";


        public override BuildingDef CreateBuildingDef()
        {
            int width = 2;
            int height = 2;
            string anim = "waterformer_kanim";
            int hitpoints = 40;
            float construction_time = 30f;
            var construction_mass = new[] {150f, 50f};
            string[] construction_mats = new string[] {MATERIALS.REFINED_METAL, MATERIALS.TRANSPARENT};
            float melting_point = 800f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            EffectorValues tieR5 = NOISE_POLLUTION.NOISY.TIER5;
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoints,
                construction_time, construction_mass, construction_mats, melting_point, build_location_rule,
                BUILDINGS.DECOR.PENALTY.TIER1, tieR5, 0.2f);
            buildingDef.RequiresPowerInput = true;
            buildingDef.UseWhitePowerOutputConnectorColour = true;
            buildingDef.PowerInputOffset = new CellOffset(0, 1);
            buildingDef.PowerOutputOffset = new CellOffset(1, 0);
            buildingDef.ElectricalArrowOffset = new CellOffset(1, 0);
            buildingDef.ExhaustKilowattsWhenActive = 0.25f;
            buildingDef.SelfHeatKilowattsWhenActive = 2f;
            buildingDef.ViewMode = OverlayModes.Power.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.Entombable = true;
            buildingDef.Floodable = false;
            buildingDef.GeneratorWattageRating = 2000f;
            buildingDef.GeneratorBaseCapacity = 2000f;
            buildingDef.PermittedRotations = PermittedRotations.FlipH;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);
            go.AddComponent<RequireInputs>();
            BuildingDef def = go.GetComponent<Building>().Def;
            Battery battery = go.AddOrGet<Battery>();
            battery.powerSortOrder = 1000;
            battery.capacity = def.GeneratorWattageRating;
            battery.chargeWattage = def.GeneratorWattageRating;
            go.AddComponent<PowerTransformer>().powerDistributionOrder = 9;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            GameObject.DestroyImmediate(go.GetComponent<EnergyConsumer>());
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}