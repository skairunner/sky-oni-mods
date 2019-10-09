using System.Collections.Generic;
using Harmony;
using PeterHan.PLib;
using UnityEngine;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace CarbonRevolution
{
    public class CarbonRevolutionPatch
    {
        public static bool didStartUp_Building;
        public static bool didStartUp_Db;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
                PUtil.InitLibrary(false);
                PUtil.RegisterPostload(CompatabilityPatches.DoPatches);
                Traverse.Create<OilFloaterConfig>().Field<float>("KG_ORE_EATEN_PER_CYCLE").Value = 40f;
                Traverse.Create<OilFloaterConfig>().Field<float>("CALORIES_PER_KG_OF_ORE").Value = OilFloaterTuning.STANDARD_CALORIES_PER_CYCLE / 40f;
                Traverse.Create<OilFloaterHighTempConfig>().Field<float>("KG_ORE_EATEN_PER_CYCLE").Value = 40f;
                Traverse.Create<OilFloaterHighTempConfig>().Field<float>("CALORIES_PER_KG_OF_ORE").Value = OilFloaterTuning.STANDARD_CALORIES_PER_CYCLE / 40f;
                
                // Add Coalplant crop type
                TUNING.CROPS.CROP_TYPES.Add(
                    new Crop.CropVal("Carbon", 100f, 100));
            }
        }
        
        // Makes coal generators output more co2
        [HarmonyPatch(typeof(GeneratorConfig), "ConfigureBuildingTemplate")]
        public static class GeneratorConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go, Tag prefab_tag)
            {
                var energyGenerator = go.GetComponent<EnergyGenerator>();
                energyGenerator.formula = EnergyGenerator.CreateSimpleFormula(
                    SimHashes.Carbon.CreateTag(), 
                    1f, 
                    600f, 
                    SimHashes.CarbonDioxide, 
                    0.25f, 
                    CompatabilityPatches.GEN_STORE_OUTPUTS, 
                    new CellOffset(1, 2), 
                    383.15f);
            }
        }
        
        // Make natural gas generators output more co2 and have a minimum output temp
        [HarmonyPatch(typeof(MethaneGeneratorConfig), "DoPostConfigureComplete")]
        public static class MethaneGeneratorConfig_DoPostConfigureComplete_Patch
        {
            public static void Postfix(GameObject go)
            {
                var energyGenerator = go.GetComponent<EnergyGenerator>();
                energyGenerator.formula = new EnergyGenerator.Formula()
                {
                    inputs = new EnergyGenerator.InputItem[1]
                    {
                        new EnergyGenerator.InputItem(GameTags.CombustibleGas, 0.09f, 0.9f)
                    },
                    outputs = new EnergyGenerator.OutputItem[2]
                    {
                        new EnergyGenerator.OutputItem(SimHashes.DirtyWater, 0.0675f, false, new CellOffset(1, 1), 313.15f),
                        new EnergyGenerator.OutputItem(
                            SimHashes.CarbonDioxide, 
                            0.140f, 
                            true,
                            new CellOffset(0, 2), 
                            383.15f)
                    }
                };
            }
        }

        [HarmonyPatch(typeof(FertilizerMakerConfig), "ConfigureBuildingTemplate")]
        public static class FertilizerMakerConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                BuildingElementEmitter buildingElementEmitter = go.AddOrGet<BuildingElementEmitter>();
                buildingElementEmitter.emitRate = 0.03f;
            }
        }

        [HarmonyPatch(typeof(SupermaterialRefineryConfig), "ConfigureBuildingTemplate")]
        public static class SupermaterialRefineryConfig_ConfigureBuildingTemplate_Patch
        {
            public const string RECIPE_DESC = "Diamond is industrial-grade, high density carbon.";
            
            public static void Postfix()
            {
                var ingredients = new []
                {
                    new ComplexRecipe.RecipeElement(SimHashes.RefinedCarbon.CreateTag(), 100f),
                };
                var result = new []
                {
                    new ComplexRecipe.RecipeElement(SimHashes.Diamond.CreateTag(), 100f)
                };
                new ComplexRecipe(ComplexRecipeManager.MakeRecipeID(
                    "SupermaterialRefinery", 
                    ingredients, 
                    result), ingredients, result)
                {
                    time = 80f,
                    description = RECIPE_DESC,
                    nameDisplay = ComplexRecipe.RecipeNameDisplay.Result
                }.fabricators = new List<Tag>()
                {
                    TagManager.Create("SupermaterialRefinery")
                };
            }
        }

        [HarmonyPatch(typeof(KilnConfig), "ConfgiureRecipes")]
        public static class KilnConfig_ConfgiureRecipes_Patch
        {
            public const string RECIPE_DESC = "Where a Coal Nodule is planted, a Coalplant springs up.";
                
            public static void Postfix()
            {
                var ingredients = new []
                {
                    new ComplexRecipe.RecipeElement(SimHashes.RefinedCarbon.CreateTag(), 100f),
                    new ComplexRecipe.RecipeElement(PrickleFlowerConfig.SEED_ID, 1f),
                };
                var result = new []
                {
                    new ComplexRecipe.RecipeElement(CoalPlantConfig.SEED_ID, 1f)
                };
                string recipeID = ComplexRecipeManager.MakeRecipeID("Kiln", ingredients, result);
                
                new ComplexRecipe(recipeID, ingredients, result)
                {
                    time = 40f,
                    description = string.Format(RECIPE_DESC),
                    fabricators = new List<Tag>()
                    {
                        TagManager.Create("Kiln")
                    },
                    nameDisplay = ComplexRecipe.RecipeNameDisplay.Result
                };
            }
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Prefix()
            {
                if (!didStartUp_Building)
                {
                    AddBuildingStrings(BigCO2ScrubberConfig.ID, BigCO2ScrubberConfig.NAME,
                        BigCO2ScrubberConfig.DESC, BigCO2ScrubberConfig.EFFECT);
                    AddBuildingStrings(RefinedCoalGeneratorConfig.ID, RefinedCoalGeneratorConfig.NAME,
                        RefinedCoalGeneratorConfig.DESC, RefinedCoalGeneratorConfig.EFFECT);
                    AddBuildingToBuildMenu("Power", RefinedCoalGeneratorConfig.ID);
                    AddBuildingToBuildMenu("Oxygen", BigCO2ScrubberConfig.ID);
                    didStartUp_Building = true;
                }
            }
        }

        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                if (!didStartUp_Db)
                {
                    AddBuildingToTech("Catalytics", BigCO2ScrubberConfig.ID);
                    AddBuildingToTech("HighTempForging", RefinedCoalGeneratorConfig.ID);
                    didStartUp_Db = true;
                }
            }
        }
    }
}