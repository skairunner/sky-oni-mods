using Harmony;
using UnityEngine;
using System;

namespace Nightinggale.PipedOutput
{
    public static class RefinementBuildingGenerationPatches
    {

        public static void AddOilRefinery(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(-1, 3), SimHashes.Methane);
        }

        public static void AddFertilizerMaker(BuildingDef def)
        {
            AddFertilizerMaker(def.BuildingPreview);
            AddFertilizerMaker(def.BuildingUnderConstruction);

            var go = def.BuildingComplete;
            BuildingElementEmitter emitter = go.GetComponent<BuildingElementEmitter>();
            if (emitter != null)
            {
                ElementConverter converter = go.GetComponent<ElementConverter>();
                if (converter != null)
                {
                    // Reserve memory for one more element in the array
                    Array.Resize(ref converter.outputElements, converter.outputElements.Length + 1);
                    // assign methane to what is now the last element in the array
                    converter.outputElements[converter.outputElements.Length - 1] =
                        new ElementConverter.OutputElement(emitter.emitRate, SimHashes.Methane, emitter.temperature);

                    UnityEngine.Object.DestroyImmediate(emitter);
                }
            }

            AddFertilizerMaker(go);
        }

        private static void AddFertilizerMaker(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(2, 2), SimHashes.Methane);
        }

        public static void AddEthanolDistillery(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(2, 2), SimHashes.CarbonDioxide);
        }

        public static void AddPolymer(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(0, 1), SimHashes.CarbonDioxide);
            ApplyExhaust.AddOutput(go, new CellOffset(1, 0), SimHashes.Steam);
        }

        [HarmonyPatch(typeof(PolymerizerConfig))]
        [HarmonyPatch("CreateBuildingDef")]
        public static class PolymerDefPatch
        {
            public static void Postfix(ref BuildingDef __result)
            {
                __result.OutputConduitType = ConduitType.None;
            }
        }
    }
}
