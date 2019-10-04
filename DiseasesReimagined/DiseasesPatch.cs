using System.Collections.Generic;
using System.Linq;
using Harmony;
using Klei.AI;
using PeterHan.PLib;
using UnityEngine;
using static SkyLib.Logger;
using Sicknesses = Database.Sicknesses;

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
                medinfo.curedSicknesses = new List<string>(new[] {FoodPoisonVomiting.ID, SlimeCoughSickness.ID});
            }
        }

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
                    if (tag == "IntermediateCure")
                        docStation.CallMethod("AddTreatment", SlimeLethalSickness.ID, tag);
                    else if (tag == "AdvancedCure")
                        docStation.CallMethod("AddTreatment", ZombieSickness.ID, tag);

                ___smi.sm.hasSupplies.Set(___treatments_available.Count > 0, ___smi);

                return false;
            }
        }

        [HarmonyPatch(typeof(DoctorStation), "IsTreatmentAvailable")]
        public static class DoctorStation_IsTreatmentAvailable_Patch
        {
            public static bool Prefix(GameObject target, Dictionary<HashedString, Tag> ___treatments_available,
                                      ref bool __result)
            {
                __result = false;

                var sicknesses = target.GetSicknesses();
                if (sicknesses == null) return false;

                foreach (var sicknessInstance in sicknesses)
                {
                    if (!___treatments_available.ContainsKey(sicknessInstance.Sickness.id)) continue;
                    __result = true;
                    break;
                }

                return false;
            }
        }

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

        [HarmonyPatch(typeof(SlimeSickness), MethodType.Constructor)]
        public static class SlimeSickness_Constructor_Patch
        {
            public static void Postfix(SlimeSickness __instance)
            {
                var sickness = Traverse.Create(__instance);

                // Remove the vanilla SlimelungComponent
                var comps = sickness.GetField<List<Sickness.SicknessComponent>>("components");
                foreach (var comp in comps.Where(comp => comp.GetType() == typeof(SlimeSickness.SlimeLungComponent)))
                    comps.Remove(comp);

                // Then replace it with our own
                sickness.CallMethod("AddSicknessComponent",
                    new AddSicknessComponent(SlimeCoughSickness.ID, "Slimelung"));
                sickness.CallMethod("AddSicknessComponent",
                    new AddSicknessComponent(SlimeLethalSickness.ID, "Slimelung"));
            }
        }
    }
}