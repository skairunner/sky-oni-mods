using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using UnityEngine;
using static SkyLib.Logger;

namespace RockCrusherConfiguration
{
    public static class FossilLimePatch
    {
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
                PUtil.InitLibrary(false);
                POptions.RegisterOptions(typeof(RockCrusherOption));
            }
        }

        [HarmonyPatch(typeof(RockCrusherConfig), "ConfigureBuildingTemplate")]
        public static class RockCrusherConfig_ConfigureBuildingTemplate_Patch
        {
            private static Tag Lime = SimHashes.Lime.CreateTag();
            private static Tag Salt = SimHashes.Salt.CreateTag();
            private static Tag TableSalt = TableSaltConfig.ID.ToTag();
            private static Tag Sand = SimHashes.Sand.CreateTag();
            private static Tag Fossil = SimHashes.Fossil.CreateTag();
            
            private static ComplexRecipe FindRecipe(Predicate<ComplexRecipe> predicate)
            {
                return ComplexRecipeManager.Get().recipes.Find(predicate);
            }
            
            private static List<ComplexRecipe> FindRecipes(Predicate<ComplexRecipe> predicate)
            {
                return ComplexRecipeManager.Get().recipes.FindAll(predicate);
            }
            
            public static void Postfix(GameObject go)
            {
                // First we need to find the existing lime recipe.
                var limeRecipe = FindRecipe(recipe => recipe.ingredients[0].material == Fossil);
                var limeAmount = RockCrusherOption.Instance.LimeFraction;
                var rockAmount = 100 - limeAmount;
                limeRecipe.results = new[]
                {
                    new ComplexRecipe.RecipeElement(Lime, limeAmount),
                    new ComplexRecipe.RecipeElement(SimHashes.SedimentaryRock.CreateTag(), rockAmount)
                };
    
                // although the code subtracts the amount of salt produced from the sand, in practice that doesn't
                // seem to be happening, so let's just ignore that.
                var saltRecipe = FindRecipe(recipe => recipe.ingredients[0].material == Salt);
                var saltProduced = RockCrusherOption.Instance.SaltAmount / 1000f;
                saltRecipe.results = new[]
                {
                    new ComplexRecipe.RecipeElement(TableSalt, saltProduced),
                    new ComplexRecipe.RecipeElement(Sand, 100f),
                };
                
                // set all refined metal fractions
                var metalProduced = (float) RockCrusherOption.Instance.RefineFraction;
                var sandProduced = 100f - metalProduced;
                
                // Logic borrowed from line ~80 of RockCrusherConfig
                var metals = ElementLoader.elements.FindAll(e => e.IsSolid && e.HasTag(GameTags.Metal) && e != e.highTempTransition.lowTempTransition);
                var metalSet = new HashSet<Tag>(metals.Select(e => e.tag));
                var metalRecipes = FindRecipes(recipe =>
                {
                    if (recipe.fabricators[0] != "RockCrusher")
                        return false;
                    if (recipe.ingredients.Length != 1)
                        return false;
                    return metalSet.Contains(recipe.ingredients[0].material);
                });

                foreach (var recipe in metalRecipes)
                {
                    var sandIndex = Array.FindIndex(recipe.results, element => element.material == Sand);
                    var metal = recipe.results[1 - sandIndex].material; // because arr length is 2, can do this
                    recipe.results = new[]
                    {
                        new ComplexRecipe.RecipeElement(metal, metalProduced),
                        new ComplexRecipe.RecipeElement(Sand, sandProduced)
                    };
                }
            }
        }
    }
}