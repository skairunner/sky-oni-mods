using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using Klei.AI;
using UnityEngine;
using Sicknesses = Database.Sicknesses;
using static SkyLib.Logger;
using static SkyLib.OniUtils;
using PeterHan.PLib;

namespace DiseasesReimagined
{
    class DiseasesPatch
    {
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
                
                AddDiseaseName(SlimeLethalSickness.ID, "Slimelung - lethal");
                AddDiseaseName(SlimeCoughSickness.ID, "Slimelung - cough");
                AddDiseaseName(FoodpoisonVomiting.ID, "Food poisoning - vomiting");
                
                SkipNotifications.Skip(SlimeLethalSickness.ID);
                SkipNotifications.Skip(SlimeCoughSickness.ID);
                SkipNotifications.Skip(FoodpoisonVomiting.ID);
            }
        }

        [HarmonyPatch(typeof(BasicCureConfig), "CreatePrefab")]
        public static class BasicCureConfig_CreatePrefab_Patch
        {
            public static void Postfix(GameObject __result)
            {
                var medinfo = __result.AddOrGet<MedicinalPill>().info;
                // The basic cure now doesn't cure the base disease, only certain symptoms
                medinfo.curedSicknesses = new List<string>(new []{ FoodpoisonVomiting.ID, SlimeCoughSickness.ID });
            }
        }

        [HarmonyPatch(typeof(DoctorStation), "OnStorageChange")]
        public static class DoctorStation_OnStorageChange_Patch
        {
            public static bool Prefix(DoctorStation __instance, Dictionary<HashedString, Tag> ___treatments_available, Storage ___storage, DoctorStation.StatesInstance ___smi)
            {
                var docstation = Traverse.Create(__instance);
                ___treatments_available.Clear();
                foreach (GameObject go in ___storage.items)
                {
                    if (go.HasTag(GameTags.MedicalSupplies))
                    {
                        Tag tag = go.PrefabID();
                        if (tag == "IntermediateCure")
                        {
                            docstation.CallMethod("AddTreatment", SlimeLethalSickness.ID, tag);
                        }
                        if (tag == "AdvancedCure")
                            docstation.CallMethod("AddTreatment", "ZombieSickness", tag);
                    }
                }

                ___smi.sm.hasSupplies.Set(___treatments_available.Count > 0, ___smi);

                return false;
            }
        }

        [HarmonyPatch(typeof(Sicknesses), MethodType.Constructor, typeof(ResourceSet))]
        public static class Sicknesses_Constructor_Patch
        {
            public static void Postfix(Sicknesses __instance)
            {
                __instance.Add(new FoodpoisonVomiting());
                __instance.Add(new SlimeCoughSickness());
                __instance.Add(new SlimeLethalSickness());
            }
        }

        [HarmonyPatch(typeof(FoodSickness), MethodType.Constructor)]
        public static class FoodSickness_Constructor_Patch
        {
            public static void Postfix(FoodSickness __instance)
            {
                Traverse
                    .Create(__instance)
                    .Method("AddSicknessComponent", new Type[] {typeof(Sickness.SicknessComponent)})
                    .GetValue(new object[] {new AddSicknessComponent(FoodpoisonVomiting.ID, "Food poisoning")});
            }
        }

        [HarmonyPatch(typeof(SlimeSickness), MethodType.Constructor)]
        public static class SlimeSickness_Constructor_Patch
        {
            public static void Postfix(SlimeSickness __instance, ref List<Sickness.SicknessComponent> ___components)
            {
                // Remove the vanilla SlimelungComponent
                ___components = ___components.Where(comp => !(comp is SlimeSickness.SlimeLungComponent)).ToList();

                // Then replace it with our own
                var addcomp = Traverse
                    .Create(__instance)
                    .Method("AddSicknessComponent", new Type[] { typeof(Sickness.SicknessComponent) });
                addcomp.GetValue(new object[] { new AddSicknessComponent(SlimeCoughSickness.ID, "Slimelung") });
                addcomp.GetValue(new object[] { new AddSicknessComponent(SlimeLethalSickness.ID, "Slimelung") });
            }
        }

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
                    if (!SkipNotifications.SicknessIDs.Contains(smi.master.Sickness.Id))
                    {
                        Notification notification = Traverse.Create(smi.master).GetField<Notification>("notification");
                        state_target.Get<Notifier>(smi).Add(notification, string.Empty);
                    }
                });
            }
        }
    }
}
