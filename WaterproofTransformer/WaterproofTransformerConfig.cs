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
            "Connect Batteries on the large side to act as a valve and prevent Wires from drawing more than 2 kW.";


        public override BuildingDef CreateBuildingDef()
        {
            var width = 2;
            var height = 2;
            var anim = "waterformer_kanim";
            var hitpoints = 40;
            var construction_time = 30f;
            var construction_mass = new[] {150f, 50f};
            string[] construction_mats = {MATERIALS.REFINED_METAL, MATERIALS.TRANSPARENT};
            var melting_point = 800f;
            var build_location_rule = BuildLocationRule.OnFloor;
            var tieR5 = NOISE_POLLUTION.NOISY.TIER5;
            var buildingDef = BuildingTemplates.CreateBuildingDef(ID, width, height, anim, hitpoints,
                construction_time, construction_mass, construction_mats, melting_point, build_location_rule,
                BUILDINGS.DECOR.PENALTY.TIER1, tieR5);
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
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery);
            go.AddComponent<RequireInputs>();
            var def = go.GetComponent<Building>().Def;
            var battery = go.AddOrGet<Battery>();
            battery.powerSortOrder = 1000;
            battery.capacity = def.GeneratorWattageRating;
            battery.chargeWattage = def.GeneratorWattageRating;
            go.AddComponent<PowerTransformer>().powerDistributionOrder = 9;
        }

        public override void DoPostConfigureComplete(GameObject go)
        {
            Object.DestroyImmediate(go.GetComponent<EnergyConsumer>());
            go.AddOrGetDef<PoweredActiveController.Def>();
        }
    }
}