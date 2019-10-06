using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using Klei.AI;
using Klei.AI.DiseaseGrowthRules;
using PeterHan.PLib;
using UnityEngine;
using static SkyLib.Logger;
using static SkyLib.OniUtils;
using Sicknesses = Database.Sicknesses;

namespace DiseasesReimagined
{
    // Patches for disease changes
    class DiseasesPatch
    {
        // misc bookkeeping
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();

                AddDiseaseName(SlimeLethalSickness.ID, "Slimelung - lethal");
                AddDiseaseName(SlimeCoughSickness.ID, "Slimelung - cough");
                AddDiseaseName(FoodPoisonVomiting.ID, "Food poisoning - vomiting");
                
                SkipNotifications.Skip(SlimeLethalSickness.ID);
                SkipNotifications.Skip(SlimeCoughSickness.ID);
                SkipNotifications.Skip(FoodPoisonVomiting.ID);
            }
        }

        // Modifies the Curative Tablet's valid cures
        [HarmonyPatch(typeof(BasicCureConfig), "CreatePrefab")]
        public static class BasicCureConfig_CreatePrefab_Patch
        {
            public static void Postfix(GameObject __result)
            {
                var medinfo = __result.AddOrGet<MedicinalPill>().info;
                // The basic cure now doesn't cure the base disease, only certain symptoms
                medinfo.curedSicknesses = new List<string>(new[] {FoodPoisonVomiting.ID, SlimeCoughSickness.ID});
            }
        }

        // Adds custom disease cures to the doctor stations
        [HarmonyPatch(typeof(DoctorStation), "OnStorageChange")]
        public static class DoctorStation_OnStorageChange_Patch
        {
            public static bool Prefix(DoctorStation __instance, Dictionary<HashedString, Tag> ___treatments_available,
                                      Storage ___storage, DoctorStation.StatesInstance ___smi)
            {
                var docStation = Traverse.Create(__instance);
                ___treatments_available.Clear();
                
                foreach (GameObject go in ___storage.items)
                {
                    if (go.HasTag(GameTags.MedicalSupplies))
                    {
                        Tag tag = go.PrefabID();
                        if (tag == "IntermediateCure")
                        {
                            docStation.CallMethod("AddTreatment", SlimeLethalSickness.ID, tag);
                        }
                        if (tag == "AdvancedCure")
                            docStation.CallMethod("AddTreatment", ZombieSickness.ID, tag);
                    }
                }

                ___smi.sm.hasSupplies.Set(___treatments_available.Count > 0, ___smi);

                return false;
            }
        }

        // Registers our new sicknesses to the DB
        [HarmonyPatch(typeof(Sicknesses), MethodType.Constructor, typeof(ResourceSet))]
        public static class Sicknesses_Constructor_Patch
        {
            public static void Postfix(Sicknesses __instance)
            {
                __instance.Add(new FoodPoisonVomiting());
                __instance.Add(new SlimeCoughSickness());
                __instance.Add(new SlimeLethalSickness());
            }
        }

        // Enables food poisoning to give different symptoms when infected with it
        [HarmonyPatch(typeof(FoodSickness), MethodType.Constructor)]
        public static class FoodSickness_Constructor_Patch
        {
            public static void Postfix(FoodSickness __instance)
            {
                Traverse.Create(__instance)
                        .CallMethod("AddSicknessComponent",
                             new AddSicknessComponent(FoodPoisonVomiting.ID, "Food poisoning"));
            }
        }

        // Enables Slimelung to give different symptoms when infected with it.
        [HarmonyPatch(typeof(SlimeSickness), MethodType.Constructor)]
        public static class SlimeSickness_Constructor_Patch
        {
            public static void Postfix(SlimeSickness __instance, ref List<Sickness.SicknessComponent> ___components)
            {
                var sickness = Traverse.Create(__instance);

                // Remove the vanilla SlimelungComponent
                ___components = ___components.Where(comp => !(comp is SlimeSickness.SlimeLungComponent)).ToList();

                // Then replace it with our own
                sickness.CallMethod("AddSicknessComponent",
                    new AddSicknessComponent(SlimeCoughSickness.ID, "Slimelung"));
                sickness.CallMethod("AddSicknessComponent",
                    new AddSicknessComponent(SlimeLethalSickness.ID, "Slimelung"));
            }
        }

        // Enables skipping notifications when infected
        [HarmonyPatch(typeof(SicknessInstance.States), "InitializeStates")]
        public static class SicknessInstance_States_InitializeStates_Patch
        {
            public static void Postfix(SicknessInstance.States __instance)
            {
                var old_enterActions = __instance.infected.enterActions;
                if (old_enterActions == null)
                {
                    return;
                }
                var new_enterActions = __instance.infected.enterActions = new List<StateMachine.Action>();
                for (var i = 0; i < old_enterActions.Count; i++)
                {
                    if (old_enterActions[i].name != "DoNotification()")
                    {
                        new_enterActions.Add(old_enterActions[i]);
                    }
                    else
                    {
                        DoNotification(__instance);
                    }
                }
            }

            // DoNotification but with a custom version that checks the whitelist.
            public static void DoNotification(SicknessInstance.States __instance)
            {
                var state_target = Traverse
                  .Create(__instance.infected)
                  .CallMethod<GameStateMachine<SicknessInstance.States, SicknessInstance.StatesInstance, SicknessInstance, object>.TargetParameter>("GetStateTarget");
                __instance.infected.Enter("DoNotification()", smi =>
                {
                    // if it's not to be skipped, (reluctantly) do the notification.
                    if (!SkipNotifications.SicknessIDs.Contains(smi.master.Sickness.Id))
                    {
                        Notification notification = Traverse.Create(smi.master).GetField<Notification>("notification");
                        state_target.Get<Notifier>(smi).Add(notification, string.Empty);
                    }
                });
            }
        }

        // Make food poisoning rapidly die on gas
        [HarmonyPatch(typeof(FoodGerms), "PopulateElemGrowthInfo")]
        public static class FoodGerms_PopulateElemGrowthInfo
        {
            public static void Postfix(FoodGerms __instance)
            {
                // Simplest method is to have food poisoning max population on air be 0
                __instance.growthRules.ForEach(rule =>
                {
                    if ((rule as StateGrowthRule)?.state == Element.State.Gas)
                    {
                        rule.maxCountPerKG = 0;
                        rule.minCountPerKG = 0;
                        rule.overPopulationHalfLife = 0.001f;
                    }
                });
                var plasticRule = new ElementExposureRule(SimHashes.Polypropylene);
                plasticRule.populationHalfLife = 300f;
                __instance.exposureRules.Add(plasticRule);
            }
        }

        // Transfer germs from germy irrigation to the plant
        [HarmonyPatch(typeof(PlantElementAbsorbers), "Sim200ms")]
        public static class PlantElementAbsorbers_Sim200ms_Patch
        {
            public static void Prefix(ref List<PlantElementAbsorber> ___data, float dt,
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

        // Sink germ transfer
        [HarmonyPatch(typeof(HandSanitizer.Work), "OnWorkTick")]
        public static class HandSanitizer_Work_OnWorkTick_Patch
        {
            public static void Prefix(HandSanitizer.Work __instance, float dt)
            {
                GermySinkManager.Instance?.SinkWorkTick(__instance, dt);
            }
        }

        [HarmonyPatch(typeof(HandSanitizer.Work), "OnStartWork")]
        public static class HandSanitizer_Work_OnStartWork_Patch
        {
            public static void Prefix(HandSanitizer.Work __instance)
            {
                GermySinkManager.Instance?.StartGermyWork(__instance);
            }
        }

        [HarmonyPatch(typeof(HandSanitizer.Work), "OnCompleteWork")]
        public static class HandSanitizer_Work_OnCompleteWork_Patch
        {
            public static void Postfix(HandSanitizer.Work __instance, Worker worker)
            {
                GermySinkManager.Instance?.FinishGermyWork(__instance, worker);
            }
        }

        // Shower germ transfer
        [HarmonyPatch(typeof(Shower), "OnWorkTick")]
        public static class Shower_OnWorkTick_Patch
        {
            public static void Prefix(Shower __instance, float dt)
            {
                GermySinkManager.Instance?.ShowerWorkTick(__instance, dt);
            }
        }

        [HarmonyPatch(typeof(Shower), "OnStartWork")]
        public static class Shower_OnStartWork_Patch
        {
            public static void Prefix(Shower __instance)
            {
                GermySinkManager.Instance?.StartGermyWork(__instance);
            }
        }

        [HarmonyPatch(typeof(Shower), "OnAbortWork")]
        public static class Shower_OnAbortWork_Patch
        {
            public static void Postfix(Shower __instance, Worker worker)
            {
                GermySinkManager.Instance?.FinishGermyWork(__instance, worker);
            }
        }

        [HarmonyPatch(typeof(Shower), "OnCompleteWork")]
        public static class Shower_OnCompleteWork_Patch
        {
            public static void Postfix(Shower __instance, Worker worker)
            {
                GermySinkManager.Instance?.FinishGermyWork(__instance, worker);
            }
        }

        // Prevent OCD hand washing by observing the hand wash cooldown
        [HarmonyPatch]
        public static class WashHandsReactable_InternalCanBegin_Patch
        {
            public static void Postfix(GameObject new_reactor, ref bool __result)
            {
                var cooldown = new_reactor.GetComponent<WashCooldownComponent>();
                if (cooldown != null && !cooldown.CanWash)
                    __result = false;
            }

            public static MethodBase TargetMethod()
            {
                // Find private class by name
                var parentType = typeof(HandSanitizer);
                var childType = parentType.GetNestedType("WashHandsReactable", BindingFlags.
                    NonPublic | BindingFlags.Instance);
                if (childType == null)
                    throw new InvalidOperationException("Could not patch hand wash class!");
                try
                {
                    var targetMethod = childType.GetMethod("InternalCanBegin", BindingFlags.
                        Public | BindingFlags.Instance | BindingFlags.NonPublic);
                    if (targetMethod == null)
                        throw new InvalidOperationException("Could not patch hand wash method!");
#if DEBUG
                    PUtil.LogDebug("Patched hand wash method: " + targetMethod);
#endif
                    return targetMethod;
                }
                catch (AmbiguousMatchException e)
                {
                    throw new InvalidOperationException("Could not patch hand wash method!", e);
                }
            }
        }

        // Manage lifecycle of GermySinkManager
        [HarmonyPatch(typeof(Game), "OnPrefabInit")]
        public static class Game_OnPrefabInit_Patch
        {
            public static void Postfix()
            {
                GermySinkManager.CreateInstance();
            }
        }

        [HarmonyPatch(typeof(Game), "DestroyInstances")]
        public static class Game_DestroyInstances_Patch
        {
            public static void Postfix()
            {
                GermySinkManager.DestroyInstance();
            }
        }
    }
}
