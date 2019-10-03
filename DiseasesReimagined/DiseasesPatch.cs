using System;
using System.Collections.Generic;
using Harmony;
using Klei.AI;
using UnityEngine;
using Sicknesses = Database.Sicknesses;
using static SkyLib.Logger;
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
                            docstation.CallMethod("AddTreatment", "SlimeLethalSickness", tag);
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
            public static void Postfix(SlimeSickness __instance)
            {
                // Remove the vanilla SlimelungComponent
                var comps = Traverse
                    .Create(__instance)
                    .Field("components")
                    .GetValue<List<Sickness.SicknessComponent>>();
                var idx = -1;
                for (var i = 0; i < comps.Count; i++)
                {
                    if (comps[i].GetType().Name == "SlimelungComponent")
                    {
                        idx = i;
                        break;
                    }
                }

                if (idx != -1)
                {
                    comps.RemoveAt(idx);
                }
                // Then replace it with our own
                var addcomp = Traverse
                    .Create(__instance)
                    .Method("AddSicknessComponent", new Type[] { typeof(Sickness.SicknessComponent) });
                addcomp.GetValue(new object[] { new AddSicknessComponent(SlimeCoughSickness.ID, "Slimelung") });
                addcomp.GetValue(new object[] { new AddSicknessComponent(SlimeLethalSickness.ID, "Slimelung") });
            }
        }
    }
}
