using Harmony;
using UnityEngine;
using NightLib;

using System;

namespace Nightinggale.PipedOutput
{
    public static class PowerBuildingGenerationPatches
    {
        public static void AddCoalGenerator(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(1, 2), SimHashes.CarbonDioxide);
        }

        public static void AddWoodGenerator(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(0, 1), SimHashes.CarbonDioxide);
        }

        public static void AddOilGenerator(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(0, 3), SimHashes.CarbonDioxide);
            ApplyExhaust.AddOutput(go, new CellOffset(1, 1), SimHashes.DirtyWater);
        }

        public static void AddGasGenerator(BuildingDef def)
        {
            AddGasGenerator(def.BuildingPreview);
            AddGasGenerator(def.BuildingUnderConstruction);
            AddGasGenerator(def.BuildingComplete);
            // remove the existing dispenser because it is messing up the CO2 output and it's no longer needed
            ConduitDispenser conduitDispenser = def.BuildingComplete.GetComponent<ConduitDispenser>();
            if (conduitDispenser != null)
            {
                UnityEngine.Object.DestroyImmediate(conduitDispenser);
            }
        } 

        private static void AddGasGenerator(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(2, 2), SimHashes.CarbonDioxide);
            ApplyExhaust.AddOutput(go, new CellOffset(1, 1), SimHashes.DirtyWater);
        }
        
        [HarmonyPatch(typeof(MethaneGeneratorConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public static class GasBurnerDefPatch
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.OutputConduitType = ConduitType.None;
            }
        }
    }
}
