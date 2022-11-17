using HarmonyLib;
using Klei.AI;
using Klei.AI.DiseaseGrowthRules;
using PeterHan.PLib.Lighting;
using PeterHan.PLib.PatchManager;
using ReimaginationTeam.Reimagination;
using STRINGS;
using System;
using System.Collections.Generic;
using PeterHan.PLib.Detours;
using UnityEngine;

using static SkyLib.Logger;
using static SkyLib.OniUtils;
using Sicknesses = Database.Sicknesses;
using SicknessStateMachine = GameStateMachine<Klei.AI.SicknessInstance.States, Klei.AI.
    SicknessInstance.StatesInstance, Klei.AI.SicknessInstance, object>;

namespace DiseasesReimagined
{
    // Patches for disease changes
    public class DiseasesPatch : KMod.UserMod2
    {
        // misc bookkeeping
        public override void OnLoad(Harmony harmony)
        {
            StartLogging();

            AddDiseaseName(SlimeLethalSickness.ID, DUPLICANTS.DISEASES.SLIMESICKNESS.NAME +
                " (lethal)");
            AddDiseaseName(SlimeCoughSickness.ID, DUPLICANTS.DISEASES.SLIMESICKNESS.NAME +
                " (cough)");
            AddDiseaseName(FoodPoisonVomiting.ID, DUPLICANTS.DISEASES.FOODSICKNESS.NAME +
                " (vomiting)");
                
            SkipNotifications.Skip(SlimeLethalSickness.ID);
            SkipNotifications.Skip(SlimeCoughSickness.ID);
            SkipNotifications.Skip(FoodPoisonVomiting.ID);

            base.OnLoad(harmony);

            ImaginationLoader.Instance.Register(typeof(DiseasesPatch));
            var patchManager = new PPatchManager(harmony);
            patchManager.RegisterPatchClass(typeof(CompatPatch));
            patchManager.RegisterPatchClass(typeof(DiseasesPatch));
            BuildingsPatch.uvlight = new PLightManager().Register("SkyLib.LightShape.FixedSemi",
                BuildingsPatch.SemicircleLight);
            FrostbitePatch.Mod_OnLoad.OnLoad();
        }

        // Helper method to find a specific attribute modifier
        private static AttributeModifier FindAttributeModifier(List<Sickness.SicknessComponent> components, string id)
        {
            var attr_mod = (AttributeModifierSickness)components.Find(comp => comp is AttributeModifierSickness);
            return Array.Find(attr_mod.Modifers, mod => mod.AttributeId == id);
        }

        // Modifies the Curative Tablet's valid cures
        [HarmonyPatch(typeof(BasicCureConfig), "CreatePrefab")]
        public static class BasicCureConfig_CreatePrefab_Patch
        {
            public static void Postfix(GameObject __result)
            {
                var medInfo = __result.AddOrGet<MedicinalPill>().info;
                // The basic cure now doesn't cure the base disease, only certain symptoms
                medInfo.curedSicknesses = new List<string>(new[] {FoodPoisonVomiting.ID,
                    SlimeCoughSickness.ID});
            }
        }

        /// <summary>
        /// Applied to Db to add a germ resistance debuff to "Dead Tired".
        /// </summary>
        [PLibMethod(RunAt.AfterDbInit)]
        internal static void AfterDbInit()
        {
            var db = Db.Get();
            var effect = db.effects.Get("TerribleSleep");
            if (effect != null)
                effect.Add(new AttributeModifier(db.Attributes.GermResistance.Id,
                    GermExposureTuning.GERM_RESIST_TIRED, effect.Name));
        }

        // Adds custom disease cures to the doctor stations
        [HarmonyPatch(typeof(DoctorStation), "OnStorageChange")]
        public static class DoctorStation_OnStorageChange_Patch
        {
            private delegate void AddTreatment(DoctorStation station, string diseaseID,
                Tag cureTag);

            private static readonly AddTreatment ADD_TREATMENT = typeof(DoctorStation).
                Detour<AddTreatment>();

            public static bool Prefix(DoctorStation __instance, Dictionary<HashedString, Tag> ___treatments_available,
                                      Storage ___storage, DoctorStation.StatesInstance ___smi)
            {
                ___treatments_available.Clear();

                foreach (var go in ___storage.items)
                    if (go.TryGetComponent(out KPrefabID id) && id.HasTag(GameTags.
                        MedicalSupplies))
                    {
                        var tag = id.PrefabTag;
                        if (tag == "IntermediateCure")
                            ADD_TREATMENT.Invoke(__instance, SlimeLethalSickness.ID, tag);
                        if (tag == "AdvancedCure")
                            ADD_TREATMENT.Invoke(__instance, ZombieSickness.ID, tag);
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
            public static void Postfix(List<Sickness.SicknessComponent> ___components)
            {
                // Remove the old attr mods and replace with our values. Easier than modifying the AttrModSickness
                ___components.RemoveAll(comp => comp is AttributeModifierSickness);
                
                // "New Hope" prevents vomiting
                ___components.Add(
                    new AddSicknessComponent(FoodPoisonVomiting.ID, DUPLICANTS.DISEASES.FOODSICKNESS.NAME, "AnewHope"));
                ___components.Add(
                    new AttributeModifierSickness(new[]
                    {
                        // 200% more bladder/cycle
                        new AttributeModifier("BladderDelta", 0.3333333f, DUPLICANTS.DISEASES.FOODSICKNESS.NAME),
                        // Twice the toilet use time
                        new AttributeModifier("ToiletEfficiency", -1f, DUPLICANTS.DISEASES.FOODSICKNESS.NAME),
                        // -30% stamina/cycle
                        new AttributeModifier("StaminaDelta", -0.05f, DUPLICANTS.DISEASES.FOODSICKNESS.NAME),
                        // 10% stress/cycle
                        new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id, 0.01666666666f, DUPLICANTS.DISEASES.FOODSICKNESS.NAME)
                    }));
            }
        }

        // Enables Slimelung to give different symptoms when infected with it.
        [HarmonyPatch(typeof(SlimeSickness), MethodType.Constructor)]
        public static class SlimeSickness_Constructor_Patch
        {
            public static void Postfix(List<Sickness.SicknessComponent> ___components, ref float ___sicknessDuration)
            {
                ___sicknessDuration = 3600f;

                // Remove the vanilla SlimelungComponent
                ___components.RemoveAll(comp => comp is SlimeSickness.SlimeLungComponent);

                // Then replace it with our own
                ___components.Add(
                    new AddSicknessComponent(SlimeCoughSickness.ID, DUPLICANTS.DISEASES.SLIMESICKNESS.NAME, "AnewHope"));
                ___components.Add(
                    new AddSicknessComponent(SlimeLethalSickness.ID, DUPLICANTS.DISEASES.SLIMESICKNESS.NAME));
                // Also add some minor stress
                ___components.Add(
                    new AttributeModifierSickness(new AttributeModifier[]
                    {
                        // 10% stress/cycle
                        new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id,
                            0.01666666666f, DUPLICANTS.DISEASES.SLIMESICKNESS.NAME)
                    }));
            }
        }
        
        // Increases sunburn stress
        [HarmonyPatch(typeof(Sunburn), MethodType.Constructor)]
        public static class Sunburn_Constructor_Patch
        {
            public static void Postfix(List<Sickness.SicknessComponent> ___components)
            {
                var stressmod = FindAttributeModifier(___components, Db.Get().Amounts.Stress.
                    deltaAttribute.Id);
                // 30% stress/cycle
                Traverse.Create(stressmod).Field("Value").SetValue(.04166666666f);
            }
        }

        [HarmonyPatch(typeof(ZombieSickness), MethodType.Constructor)]
        public static class ZombieSickness_Constructor_Patch
        {
            public static void Postfix(List<Sickness.SicknessComponent> ___components)
            {
                // 20% stress/cycle
                ___components.Add(
                    new AttributeModifierSickness(new AttributeModifier[]
                    {
                        new AttributeModifier(Db.Get().Amounts.Stress.deltaAttribute.Id,
                            0.03333333333f, DUPLICANTS.DISEASES.ZOMBIESICKNESS.NAME)
                    }));
            }
        }
        

        // Enables skipping notifications when infected
        [HarmonyPatch(typeof(SicknessInstance.States), "InitializeStates")]
        public static class SicknessInstance_States_InitializeStates_Patch
        {
            private delegate SicknessStateMachine.TargetParameter GetStateTarget(
                SicknessStateMachine.State state);

            private static readonly GetStateTarget GET_STATE_TARGET = typeof(
                SicknessStateMachine.State).Detour<GetStateTarget>();

            private static readonly IDetouredField<SicknessInstance, Notification> NOTIFICATION_FIELD =
                PDetours.DetourField<SicknessInstance, Notification>("notification");

            public static void Postfix(SicknessInstance.States __instance)
            {
                var old_enterActions = __instance.infected.enterActions;
                var new_enterActions = (__instance.infected.enterActions =
                    new List<StateMachine.Action>());
                if (old_enterActions != null)
                    for (var i = 0; i < old_enterActions.Count; i++)
                    {
                        if (old_enterActions[i].name != "DoNotification()")
                            new_enterActions.Add(old_enterActions[i]);
                        else
                            DoNotification(__instance);
                    }
            }

            // DoNotification but with a custom version that checks the whitelist.
            private static void DoNotification(SicknessInstance.States __instance)
            {
                var state_target = GET_STATE_TARGET.Invoke(__instance.infected);

                __instance.infected.Enter("DoNotification()", smi =>
                {
                    // if it's not to be skipped, (reluctantly) do the notification.
                    if (!SkipNotifications.SicknessIDs.Contains(smi.master.Sickness.Id))
                    {
                        var notification = NOTIFICATION_FIELD.Get(smi.master);
                        if (notification != null)
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
                var rules = __instance.growthRules;
                // Simplest method is to have food poisoning max population on air be 0
                foreach (var rule in rules)
                {
                    if (rule is StateGrowthRule state && state.state == Element.State.Gas)
                    {
                        rule.maxCountPerKG = 0;
                        rule.minCountPerKG = 0;
                        rule.overPopulationHalfLife = 1f;
                    }
                }
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
