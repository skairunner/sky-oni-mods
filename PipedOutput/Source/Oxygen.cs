using System;
using Harmony;
using UnityEngine;
using NightLib;

namespace Nightinggale.PipedOutput
{
    public static class OxygenBuildingGenerationPatches
    {

        public static void AddElectrolyzer(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(1, 1), SimHashes.Oxygen);
            ApplyExhaust.AddOutput(go, new CellOffset(0, 1), SimHashes.Hydrogen);
        }

        public static void AddRust(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(1, 1), SimHashes.Oxygen);
            ApplyExhaust.AddOutput(go, new CellOffset(0, 0), SimHashes.ChlorineGas);
        }

        internal static PortDisplayOutput AddAlgaeHabitat(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(0, 1), SimHashes.Oxygen);

            Element element = ElementLoader.GetElement(SimHashes.DirtyWater.CreateTag());
            Color32 color = element.substance.conduitColour;
            color.a = 255;
            PortDisplayOutput outputPort = new PortDisplayOutput(ConduitType.Liquid, new CellOffset(0, 0), null, color);
            PortDisplayController controller = go.AddOrGet<PortDisplayController>();
            controller.AssignPort(go, outputPort);

            return outputPort;
        }

        private static void PrintPorts(GameObject go)
        {
            var controller = go.GetComponent<PortDisplayController>();
            if (controller != null)
            {
                var ports = controller.GetAllPorts();
                foreach (var port in ports)
                {
                    Console.Write($"{port.color}{port.offset}");
                }

                Console.WriteLine();
            }
        }

        public static void ConfigureAlgaeHabitat(BuildingDef def)
        {
            AddAlgaeHabitat(def.BuildingPreview);
            AddAlgaeHabitat(def.BuildingUnderConstruction);

            var go = def.BuildingComplete;
            PortDisplayOutput outputPort = AddAlgaeHabitat(go);

            PipedDispenser dispenser = go.AddComponent<PipedDispenser>();
            dispenser.AssignPort(outputPort);
            dispenser.SkipSetOperational = true;
            dispenser.alwaysDispense = true;

            Storage[] storageComponents = go.GetComponents<Storage>();

            foreach (Storage storage in storageComponents)
            {
                if (storage.storageFilters != null && storage.storageFilters.Contains(SimHashes.DirtyWater.CreateTag()))
                {
                    dispenser.storage = storage;
                    break;
                }
            }
        }

        public static void AddMineralDeoxidizer(GameObject go)
        {
            ApplyExhaust.AddOutput(go, new CellOffset(0, 1), SimHashes.Oxygen);
        }
    }
}
