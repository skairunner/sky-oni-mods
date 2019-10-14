using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using Klei.AI;
using Klei.AI.DiseaseGrowthRules;
using PeterHan.PLib;
using STRINGS;
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

                AddDiseaseName(SlimeLethalSickness.ID, "Slimelung (lethal)");
                AddDiseaseName(SlimeCoughSickness.ID, "Slimelung (cough)");
                AddDiseaseName(FoodPoisonVomiting.ID, "Food Poisoning (vomiting)");
                
                SkipNotifications.Skip(SlimeLethalSickness.ID);
                SkipNotifications.Skip(SlimeCoughSickness.ID);
                SkipNotifications.Skip(FoodPoisonVomiting.ID);

                PUtil.InitLibrary(false);
                PUtil.RegisterPostload(CompatPatch.CompatPatches);
            }
            
            // Helper method to find a specific attribute modifier
            public static AttributeModifier FindAttributeModifier(List<Sickness.SicknessComponent> components, string id)
            {
                var attr_mod = (AttributeModifierSickness)components.Find(comp => comp is AttributeModifierSickness);
                return Array.Find(attr_mod.Modifers, mod => mod.AttributeId == id);
            }
        }

        // Modifies the Curative Tablet's valid cures
        [HarmonyPatch(typeof(BasicCureConfig), "CreatePrefab")]
        public static class BasicCureConfig_CreatePrefab_Patch
        {
            public static void Postfix(GameObject __result)
            {
                var medInfo = __result.AddOrGet<MedicinalPill>().info;
                // The basic cure now doesn't cure the base disease, only certain symptoms
                medInfo.curedSicknesses = new List<string>(new[] {FoodPoisonVomiting.ID, SlimeCoughSickness.ID});
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

                foreach (var tag in ___storage.items.Where(go => go.HasTag(GameTags.MedicalSupplies))
                                              .Select(go => go.PrefabID()))
                {
                    if (tag == "IntermediateCure")
                        docStation.CallMethod("AddTreatment", SlimeLethalSickness.ID, tag);
                    if (tag == "AdvancedCure")
                        docStation.CallMethod("AddTreatment", ZombieSickness.ID, tag);
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
            public static void Postfix(FoodSickness __instance, List<Sickness.SicknessComponent> ___components)
            {
                // Remove the old attr mods and replace with our values. Easier than modifying the AttrModSickness
                ___components.RemoveAll(comp => comp is AttributeModifierSickness);
                
                var trav = Traverse.Create(__instance);
                trav.CallMethod("AddSicknessComponent",
                    new AddSicknessComponent(FoodPoisonVomiting.ID, "Food poisoning"));
                trav.CallMethod("AddSicknessComponent",
                    new AttributeModifierSickness(new[]
                    {
                        // 200% more bladder/cycle
                        new AttributeModifier("BladderDelta", 0.3333333f, (string) DUPLICANTS.DISEASES.FOODSICKNESS.NAME),
                        // Twice the toilet use time
                        new AttributeModifier("ToiletEfficiency", -1f, (string) DUPLICANTS.DISEASES.FOODSICKNESS.NAME),
                        // -30% stamina/cycle
                        new AttributeModifier("StaminaDelta", -0.05f, (string) DUPLICANTS.DISEASES.FOODSICKNESS.NAME),
                        // 10% stress/cycle
                        new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 0.01666666666f, "Food poisoning")
                    }));
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
                ___components.RemoveAll(comp => comp is SlimeSickness.SlimeLungComponent);

                // Then replace it with our own
                sickness.CallMethod("AddSicknessComponent",
                    new AddSicknessComponent(SlimeCoughSickness.ID, "Slimelung"));
                sickness.CallMethod("AddSicknessComponent",
                    new AddSicknessComponent(SlimeLethalSickness.ID, "Slimelung"));
                // Also add some minor stress
                sickness.CallMethod("AddSicknessComponent",
                    new AttributeModifierSickness(new AttributeModifier[]
                    {
                        // 10% stress/cycle
                        new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 0.01666666666f, "Slimelung")
                    }));
            }
        }
        
        // Increases sunburn stress
        [HarmonyPatch(typeof(Sunburn), MethodType.Constructor)]
        public static class Sunburn_Constructor_Patch
        {
            public static void Postfix(ref List<Sickness.SicknessComponent> ___components)
            {
                var stressmod =
                    Mod_OnLoad.FindAttributeModifier(___components, Db.Get().Amounts.Stress.deltaAttribute.Id);
                Traverse.Create(stressmod).SetField("Value", .04166666666f); // 30% stress/cycle
            }
        }

        [HarmonyPatch(typeof(ZombieSickness), MethodType.Constructor)]
        public static class ZombieSickness_Constructor_Patch
        {
            public static void Postfix(ZombieSickness __instance)
            {
                // 20% stress/cycle
                Traverse.Create(__instance)
                        .CallMethod("AddSicknessComponent",
                    new AttributeModifierSickness(new AttributeModifier[]
                    {
                        new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 0.03333333333f, "Zombie spores")
                    }));
            }
        }
        

        // Enables skipping notifications when infected
        [HarmonyPatch(typeof(SicknessInstance.States), "InitializeStates")]
        public static class SicknessInstance_States_InitializeStates_Patch
        {
            public static void Postfix(SicknessInstance.States __instance)
            {
                List<StateMachine.Action> old_enterActions = __instance.infected.enterActions;
                List<StateMachine.Action> new_enterActions =
                    __instance.infected.enterActions = new List<StateMachine.Action>();
                if (old_enterActions == null) return;

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
                                  .CallMethod<
                                       GameStateMachine<SicknessInstance.States, SicknessInstance.StatesInstance,
                                           SicknessInstance, object>.TargetParameter>("GetStateTarget");
                __instance.infected.Enter("DoNotification()", smi =>
                {
                    // if it's not to be skipped, (reluctantly) do the notification.
                    if (SkipNotifications.SicknessIDs.Contains(smi.master.Sickness.Id)) return;

                    var notification = Traverse.Create(smi.master).GetField<Notification>("notification");
                    state_target.Get<Notifier>(smi).Add(notification, string.Empty);
                });
            }
        }

        // Make food poisoning rapidly die on gas
        [HarmonyPatch(typeof(FoodGerms), "PopulateElemGrowthInfo")]
        public static class FoodGerms_PopulateElemGrowthInfo
        {
            public static void Postfix(FoodGerms __instance)
            {
                var rules = __instance.growthRules;
                // Simplest method is to have food poisoning max population on air be 0
                rules.ForEach(rule =>
                {
                    if ((rule as StateGrowthRule)?.state == Element.State.Gas)
                    {
                        rule.maxCountPerKG = 0;
                        rule.minCountPerKG = 0;
                        rule.overPopulationHalfLife = 0.001f;
                    }
                });
                rules.Add(new ElementGrowthRule(SimHashes.Polypropylene)
                {
                    populationHalfLife = 300f,
                    overPopulationHalfLife = 300f
                });
            }
        }

        // Make slimelung die on plastic
        [HarmonyPatch(typeof(SlimeGerms), "PopulateElemGrowthInfo")]
        public static class SlimeGerms_PopulateElemGrowthInfo
        {
            public static void Postfix(SlimeGerms __instance)
            {
                __instance.growthRules.Add(new ElementGrowthRule(SimHashes.Polypropylene)
                {
                    populationHalfLife = 300f,
                    overPopulationHalfLife = 300f
                });
            }
        }
        
        // Buff zombie spores to diffuse on solids
        [HarmonyPatch(typeof(ZombieSpores), "PopulateElemGrowthInfo")]
        public static class ZombieSpores_PopulateElemGrowthInfo_Patch
        {
            public static void Postfix(ZombieSpores __instance)
            {
                var rules = __instance.growthRules;
                foreach (var rule in rules)
                    // Dying on Solid changed to spread around tiles
                    if (rule is StateGrowthRule stateRule && stateRule.state == Element.State.
                        Solid)
                    {
                        stateRule.minDiffusionCount = 20000;
                        stateRule.diffusionScale = 0.001f;
                        stateRule.minDiffusionInfestationTickCount = 1;
                    }
                // And it survives on lead and iron ore, but has a low overpop threshold
                rules.Add(new ElementGrowthRule(SimHashes.Lead)
                {
                    underPopulationDeathRate = 0.0f,
                    populationHalfLife = float.PositiveInfinity,
                    overPopulationHalfLife = 300.0f,
                    maxCountPerKG = 100.0f,
                    diffusionScale = 0.001f,
                    minDiffusionCount = 50000,
                    minDiffusionInfestationTickCount = 1
                });
                rules.Add(new ElementGrowthRule(SimHashes.IronOre)
                {
                    underPopulationDeathRate = 0.0f,
                    populationHalfLife = float.PositiveInfinity,
                    overPopulationHalfLife = 300.0f,
                    maxCountPerKG = 100.0f,
                    diffusionScale = 0.001f,
                    minDiffusionCount = 50000,
                    minDiffusionInfestationTickCount = 1
                });
                // But gets rekt on abyssalite and neutronium
                rules.Add(new ElementGrowthRule(SimHashes.Katairite)
                {
                    populationHalfLife = 5.0f,
                    overPopulationHalfLife = 5.0f,
                    minDiffusionCount = 1000000
                });
                rules.Add(new ElementGrowthRule(SimHashes.Unobtanium)
                {
                    populationHalfLife = 5.0f,
                    overPopulationHalfLife = 5.0f,
                    minDiffusionCount = 1000000
                });
                // -75% on plastic all germs
                rules.Add(new ElementGrowthRule(SimHashes.Polypropylene)
                {
                    populationHalfLife = 300f,
                    overPopulationHalfLife = 300f
                });
            }
        }
    }
}
