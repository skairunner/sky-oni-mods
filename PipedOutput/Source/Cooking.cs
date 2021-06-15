using Harmony;
using UnityEngine;
using NightLib;

namespace Nightinggale.PipedOutput
{
    public static class CookingBuildingGenerationPatches
    {
        internal static void AddGourmetCooking(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(1, 2), SimHashes.CarbonDioxide);
        }

        public static void AddGourmetCooking(BuildingDef def)
        {
            AddGourmetCooking(def.BuildingPreview);
            AddGourmetCooking(def.BuildingUnderConstruction);
            AddGourmetCooking(def.BuildingComplete);
        }
    }
}
