using System.Collections.Generic;
using System.Linq;
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
                ___components.RemoveAll(comp => comp is SlimeSickness.SlimeLungComponent);
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
                // Simplest method is to have food poisoning max population on air be 0
                foreach (var rule in __instance.growthRules.Where(rule =>
                    (rule as StateGrowthRule)?.state == Element.State.Gas))
                {
                    rule.maxCountPerKG = 0;
                    rule.minCountPerKG = 0;
                    rule.overPopulationHalfLife = 5f;
                }
            }
        }
    }
}