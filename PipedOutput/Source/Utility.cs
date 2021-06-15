using Harmony;
using UnityEngine;
using NightLib;

namespace Nightinggale.PipedOutput
{
    public static class UtilityBuildingGenerationPatches
    {
        public static void AddOilWell(BuildingDef def)
        {
            AddOilWell(def.BuildingPreview);
            AddOilWell(def.BuildingUnderConstruction);

            var go = def.BuildingComplete;
            PortDisplayOutput outputPort = AddOilWell(go);
            PipedDispenser dispenser = go.AddComponent<PipedDispenser>();
            dispenser.elementFilter = new SimHashes[] { SimHashes.Methane };
            dispenser.AssignPort(outputPort);
            dispenser.alwaysDispense = true;
            dispenser.SkipSetOperational = true;
        }
        
        internal static PortDisplayOutput AddOilWell(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(2, 1), SimHashes.CrudeOil);

            Element element = ElementLoader.GetElement(SimHashes.Methane.CreateTag());
            Color32 color = element.substance.conduitColour;
            color.a = 255;
            PortDisplayOutput outputPort = new PortDisplayOutput(ConduitType.Gas, new CellOffset(1, 1), null, color);
            PortDisplayController controller = go.AddOrGet<PortDisplayController>();
            controller.AssignPort(go, outputPort);

            return outputPort;
        }
    }
}
