using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using Klei.AI;
using Klei.AI.DiseaseGrowthRules;
using PeterHan.PLib;
using UnityEngine;

namespace DiseasesReimagined
{
    public class PlantsPatch
    {
        
        // Transfer germs from germy irrigation to the plant
        [HarmonyPatch(typeof(PlantElementAbsorbers), "Sim200ms")]
        public static class PlantElementAbsorbers_Sim200ms_Patch
        {
            public static void Prefix(List<PlantElementAbsorber> ___data, float dt,
                ref bool ___updating)
            {
                // This variable is remapped to an instance variable so the store is not dead
#pragma warning disable IDE0059 // Unnecessary assignment of a value
                ___updating = true;
#pragma warning restore IDE0059 // Unnecessary assignment of a value
                foreach (var absorber in ___data)
                {
                    var storage = absorber.storage;
                    GameObject farmTile;
                    if (storage != null && (farmTile = storage.gameObject) != null)
                    {
                        if (absorber.consumedElements == null)
                        {
                            var info = absorber.localInfo;
                            InfectPlant(farmTile, info.massConsumptionRate * dt, absorber,
                                info.tag);
                        }
                        else
                            // Grrr LocalInfo is not convertible to ConsumeInfo
                            foreach (var info in absorber.consumedElements)
                                InfectPlant(farmTile, info.massConsumptionRate * dt, absorber,
                                    info.tag);
                    }
                }
                ___updating = false;
            }

            // Infect the plant with germs from the irrigation material
            private static void InfectPlant(GameObject farmTile, float required,
                PlantElementAbsorber absorber, Tag material)
            {
                var storage = absorber.storage;
                GameObject plant;
                PrimaryElement irrigant;
                // Check all available items
                while (required > 0.0f && (irrigant = storage.FindFirstWithMass(material)) !=
                    null)
                {
                    float mass = irrigant.Mass, consumed = Mathf.Min(required, mass);
                    int disease = irrigant.DiseaseCount;
                    if (disease > 0 && (plant = farmTile.GetComponent<PlantablePlot>()?.
                        Occupant) != null)
                    {
                        plant.GetComponent<PrimaryElement>()?.AddDisease(irrigant.DiseaseIdx,
                            Mathf.RoundToInt(required * disease / mass), "Irrigation");
                    }
                    required -= consumed;
                }
            }
        }

        // Sporechids spread spores onto their current tile
        [HarmonyPatch(typeof(EvilFlower), "OnSpawn")]
        public static class EvilFlower_OnSpawn_Patch
        {
            public static void Postfix(EvilFlower __instance)
            {
                __instance.gameObject?.AddOrGet<MoreEvilFlower>();
            }
        }
        
        // Transfers germs from one object to another using their mass ratios
        public static void TransferByMassRatio(GameObject parent, GameObject child)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            if (child == null)
                throw new ArgumentNullException("child");
            PrimaryElement parentElement = parent.GetComponent<PrimaryElement>(),
                childElement = child.GetComponent<PrimaryElement>();
            float seedMass;
            int germs, subGerms;
            // Distribute the germs by mass ratio if there are any
            if (parentElement != null && childElement != null && (seedMass = childElement.
                Mass) > 0.0f && (germs = parentElement.DiseaseCount) > 0)
            {
                byte disease = parentElement.DiseaseIdx;
                subGerms = Mathf.RoundToInt(germs * seedMass / (seedMass +
                    parentElement.Mass));
                // Seed germs
                childElement.AddDisease(disease, subGerms, "TransferToChild");
                // Plant germs
                parentElement.AddDisease(disease, -subGerms, "TransferFromParent");
            }
        }

        // Transfer germs from plant to fruit
        [HarmonyPatch(typeof(Crop), "SpawnFruit")]
        public static class Crop_SpawnFruit_Patch
        {
            // Transfers germs from crop to child (fruit)
            internal static void AddGerms(Crop parent, GameObject child)
            {
                TransferByMassRatio(parent.gameObject, child);
            }

            public static IEnumerable<CodeInstruction> Transpiler(
                IEnumerable<CodeInstruction> method)
            {
                MethodBase setTemp = null, germify = null;
                // No easy way to get the game object without a replacement or transpiler
                try
                {
                    var tempProp = typeof(PrimaryElement).GetProperty("Temperature",
                        BindingFlags.Public | BindingFlags.Instance);
                    // set_Temperature
                    if (tempProp != null)
                        setTemp = tempProp.GetSetMethod(true);
                    germify = typeof(Crop_SpawnFruit_Patch).GetMethod(nameof(AddGerms),
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                }
                catch (AmbiguousMatchException e)
                {
                    // This is not good
                    PUtil.LogException(e);
                }
                foreach (var instr in method)
                    if (instr.opcode == OpCodes.Callvirt && instr.operand == setTemp &&
                        germify != null)
                    {
                        yield return instr;
                        // ldarg.0 loads "this"
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        // ldloc.1 loads the returned GameObject
                        yield return new CodeInstruction(OpCodes.Ldloc_1);
                        // Callvirt AddGerms
                        yield return new CodeInstruction(OpCodes.Call, germify);
                    }
                    else
                        // Rest of method
                        yield return instr;
            }
        }

        // Transfer germs from plant to seed
        [HarmonyPatch(typeof(SeedProducer), "ProduceSeed")]
        public static class SeedProducer_ProduceSeed_Patch
        {
            public static void Postfix(SeedProducer __instance, GameObject __result)
            {
                var seed = __result;
                var obj = __instance.gameObject;
                if (seed != null && obj != null)
                    TransferByMassRatio(obj, seed);
            }
        }
    }
}