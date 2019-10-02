using System;
using System.Collections.Generic;
using Harmony;
using Klei.AI;
using UnityEngine;
using Sicknesses = Database.Sicknesses;
using static SkyLib.Logger;

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
                // The basic cure now doesn't cure the base disease, only the more severe symptoms
                medinfo.curedSicknesses = new List<string>(new []{ "FoodpoisonVomiting" });
            }
        }

        [HarmonyPatch(typeof(Sicknesses), MethodType.Constructor, typeof(ResourceSet))]
        public static class Sicknesses_Constructor_Patch
        {
            public static void Postfix(Sicknesses __instance)
            {
                __instance.Add(new VomitSickness());
                __instance.Add(new FoodpoisonVomiting());
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
                    .GetValue(new object[] {new AddVomitingSicknessComponent("Food poisoning")});
            }
        }
    }
}
