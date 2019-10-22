using System.Collections.Generic;
using System.Linq;
using Harmony;
using PeterHan.PLib;
using PeterHan.PLib.Options;
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
                POptions.RegisterOptions(typeof(CarbonOption));

                Traverse.Create<OilFloaterConfig>().Field<float>("KG_ORE_EATEN_PER_CYCLE").Value = 40f;
                Traverse.Create<OilFloaterConfig>().Field<float>("CALORIES_PER_KG_OF_ORE").Value = OilFloaterTuning.STANDARD_CALORIES_PER_CYCLE / 40f;
                Traverse.Create<OilFloaterHighTempConfig>().Field<float>("KG_ORE_EATEN_PER_CYCLE").Value = 40f;
                Traverse.Create<OilFloaterHighTempConfig>().Field<float>("CALORIES_PER_KG_OF_ORE").Value = OilFloaterTuning.STANDARD_CALORIES_PER_CYCLE / 40f;
                
                // Add Coalplant crop type
                TUNING.CROPS.CROP_TYPES.Add(
                    new Crop.CropVal("Carbon", CoalPlantConfig.LIFECYCLE, (int)CoalPlantConfig.COAL_PRODUCED));
                var RESONANT_NUM_SEEDS = ResonantPlantConfig.COAL_PRODUCED_TOTAL / ResonantPlantConfig.COAL_PER_SEED;
                TUNING.CROPS.CROP_TYPES.Add(
                    new Crop.CropVal(ResonantPlantConfig.SEED_ID, ResonantPlantConfig.LIFECYCLE, (int)RESONANT_NUM_SEEDS));
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
                    CarbonOption.Instance.CO2_coalgen, 
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
                        new EnergyGenerator.OutputItem(SimHashes.DirtyWater, 0.0675f, 
                          CompatabilityPatches.GEN_STORE_OUTPUTS, new CellOffset(1, 1), 313.15f),
                        new EnergyGenerator.OutputItem(
                            SimHashes.CarbonDioxide, 
                            CarbonOption.Instance.CO2_natgasgen, 
                             true,
                            new CellOffset(0, 2), 
                            383.15f)
                    }
                };
            }
        }

        [HarmonyPatch(typeof(PetroleumGeneratorConfig), "DoPostConfigureComplete")]
        public static class PetroleumGeneratorConfig_DoPostConfigureComplete_Patch {
            public static void Postfix(GameObject go)
            {
                go.GetComponent<EnergyGenerator>()
                  .formula = new EnergyGenerator.Formula()
                {
                    inputs = new []
                    {
                        new EnergyGenerator.InputItem(GameTags.CombustibleLiquid, 2f, 20f)
                    },
                    outputs = new []
                    {
                        new EnergyGenerator.OutputItem(
                            SimHashes.CarbonDioxide, 
                            CarbonOption.Instance.CO2_petrolgen, 
                            CompatabilityPatches.GEN_STORE_OUTPUTS, 
                            new CellOffset(0, 3), 
                            383.15f),
                        new EnergyGenerator.OutputItem(SimHashes.DirtyWater, 0.75f, CompatabilityPatches.GEN_STORE_OUTPUTS, new CellOffset(1, 1), 313.15f)
                    }
                };
            }
        }
        
        [HarmonyPatch(typeof(WoodGasGeneratorConfig), "DoPostConfigureComplete")]
        public static class WoodGasGeneratorConfig_DoPostConfigureComplete_Patch {
            public static void Postfix(GameObject go)
            {
                go.GetComponent<EnergyGenerator>()
                  .formula = EnergyGenerator.CreateSimpleFormula(
                    WoodLogConfig.TAG, 
                    1.2f, 
                    720f, 
                    SimHashes.CarbonDioxide, 
                    CarbonOption.Instance.CO2_lumbergen, 
                    CompatabilityPatches.GEN_STORE_OUTPUTS, new CellOffset(0, 1), 383.15f);
            }
        }

        [HarmonyPatch(typeof(GourmetCookingStationConfig), "ConfigureBuildingTemplate")]
        public static class GourmetCookingStationConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                go.GetComponent<ElementConverter>()
                  .outputElements = new []
                {
                    new ElementConverter.OutputElement(CarbonOption.Instance.CO2_gasrange, SimHashes.CarbonDioxide, 348.15f, false, false, 0.0f, 3f, 1f, byte.MaxValue, 0)
                };
            }
        }

        [HarmonyPatch(typeof(EthanolDistilleryConfig), "ConfigureBuildingTemplate")]
        public static class EthanolDistilleryConfig_ConfigureBuildingTemplate_Patch
        {
            public static void Postfix(GameObject go)
            {
                var outputs = go.GetComponent<ElementConverter>()
                  .outputElements;
                outputs
                   .Where(element => element.elementHash != SimHashes.CarbonDioxide)
                   .Add(new ElementConverter.OutputElement(
                        CarbonOption.Instance.CO2_ethanoldistiller, 
                        SimHashes.CarbonDioxide, 
                        348.15f, 
                        false, 
                        CompatabilityPatches.GEN_STORE_OUTPUTS, 
                        0.0f, 
                        3f)
                    );
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
            public const string COALPLANT_RECIPE_DESC = "Where a Coal Nodule is planted, a Bituminous Blossom springs up.";

            public const string RESONANTPLANT_RECIPE_DESC =
                "Smokestalk seeds are surprisingly heavy, and you can hear a sharp crack when the bud cracks its shell.";
                
            public static void Postfix()
            {
                // add bituminous blossom
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
                    description = string.Format(COALPLANT_RECIPE_DESC),
                    fabricators = new List<Tag>()
                    {
                        TagManager.Create("Kiln")
                    },
                    nameDisplay = ComplexRecipe.RecipeNameDisplay.Result
                };
                // add smokestalk
                var ingredients2 = new []
                {
                    new ComplexRecipe.RecipeElement(SimHashes.RefinedCarbon.CreateTag(), 400f),
                    new ComplexRecipe.RecipeElement(SimHashes.Diamond.CreateTag(), 100f),
                    new ComplexRecipe.RecipeElement(CoalPlantConfig.SEED_ID, 1f),
                };
                var result2 = new []
                {
                    new ComplexRecipe.RecipeElement(ResonantPlantConfig.SEED_ID, 1f)
                };
                string recipeID2 = ComplexRecipeManager.MakeRecipeID("Kiln", ingredients2, result2);
                
                new ComplexRecipe(recipeID2, ingredients2, result2)
                {
                    time = 40f,
                    description = string.Format(RESONANTPLANT_RECIPE_DESC),
                    fabricators = new List<Tag>()
                    {
                        TagManager.Create("Kiln")
                    },
                    nameDisplay = ComplexRecipe.RecipeNameDisplay.Result
                };
            }
        }

        [HarmonyPatch(typeof(RockCrusherConfig), "ConfigureBuildingTemplate")]
        public static class RockCrusherConfig_COnfigureBuildingTemplate_Patch
        {
            private const string RECIPE_DESC = ""; 
            public static void Postfix()
            {   
                ComplexRecipe.RecipeElement[] ingredients2 = new ComplexRecipe.RecipeElement[1]
                {
                    new ComplexRecipe.RecipeElement(ResonantPlantConfig.SEED_ID, 1f)
                };
                ComplexRecipe.RecipeElement[] results2 = new ComplexRecipe.RecipeElement[1]
                {
                    new ComplexRecipe.RecipeElement(SimHashes.Carbon.CreateTag(), ResonantPlantConfig.COAL_PER_SEED)
                };
                new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("RockCrusher", (IList<ComplexRecipe.RecipeElement>) ingredients2, (IList<ComplexRecipe.RecipeElement>) results2), ingredients2, results2)
                {
                    time = 40f,
                    description = RECIPE_DESC,
                    nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult
                }.fabricators = new List<Tag>()
                {
                    TagManager.Create("RockCrusher")
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