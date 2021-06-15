using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

namespace Nightinggale.PipedOutput
{
    [HarmonyPatch(typeof(GeneratedBuildings))]
    [HarmonyPatch("LoadGeneratedBuildings")]
    public class MegaPatch
    {
        private delegate void BuildingConfig(BuildingDef def);

        private delegate void ApplyToBuilding(GameObject go);
        
        static Dictionary<String, BuildingConfig> CONFIGS;
        /// <summary>
        ///  All building defs are loaded by this point. Now we check them for buildings of interest
        /// and patch those.
        /// </summary>
        public static void Postfix()
        {
            Patches();

            foreach (var def in Assets.BuildingDefs)
            {
                if (CONFIGS.ContainsKey(def.PrefabID))
                {
                    Debug.Log($"[Piped Output Continued] Patching {def.PrefabID}");
                    CONFIGS[def.PrefabID](def);
                }
            }
        }

        // Curry function to apply the provided method to all three defs
        private static BuildingConfig ApplyThree(ApplyToBuilding del)
        {
            return def =>
            {
                del(def.BuildingPreview);
                del(def.BuildingUnderConstruction);
                del(def.BuildingComplete);
            };
        }

        // set all patches
        private static void Patches()
        {
            CONFIGS = new Dictionary<string, BuildingConfig>();
            // COOKING
            CONFIGS.Add("GourmetCookingStation", ApplyThree(CookingBuildingGenerationPatches.AddGourmetCooking));
            // OXYGEN
            CONFIGS.Add("AlgaeHabitat", OxygenBuildingGenerationPatches.ConfigureAlgaeHabitat);
            CONFIGS.Add("Electrolyzer", ApplyThree(OxygenBuildingGenerationPatches.AddElectrolyzer));
            CONFIGS.Add("MineralDeoxidizer", ApplyThree(OxygenBuildingGenerationPatches.AddMineralDeoxidizer));
            CONFIGS.Add("RustDeoxidizer", ApplyThree(OxygenBuildingGenerationPatches.AddRust));
            // POWER
            CONFIGS.Add("CoalBurner", ApplyThree(PowerBuildingGenerationPatches.AddCoalGenerator));
            CONFIGS.Add("WoodGasGenerator", ApplyThree(PowerBuildingGenerationPatches.AddWoodGenerator));
            CONFIGS.Add("PetroleumGenerator", ApplyThree(PowerBuildingGenerationPatches.AddOilGenerator));
            CONFIGS.Add("MethaneGenerator", PowerBuildingGenerationPatches.AddGasGenerator);
            // REFINEMENT
            CONFIGS.Add("OilRefinery", ApplyThree(RefinementBuildingGenerationPatches.AddOilRefinery));
            CONFIGS.Add("FertilizerMaker", RefinementBuildingGenerationPatches.AddFertilizerMaker);
            CONFIGS.Add("EthanolDistillery", ApplyThree(RefinementBuildingGenerationPatches.AddEthanolDistillery));
            CONFIGS.Add("Polymerizer", ApplyThree(RefinementBuildingGenerationPatches.AddPolymer));
            // UTILITY
            CONFIGS.Add("OilWellCap", UtilityBuildingGenerationPatches.AddOilWell);
        }
    }
}
