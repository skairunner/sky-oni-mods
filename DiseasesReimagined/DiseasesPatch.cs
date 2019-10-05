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
    using TempMonitorStateMachine = GameStateMachine<ExternalTemperatureMonitor, ExternalTemperatureMonitor.Instance,
        IStateMachineTarget,
        object>;
    
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
                
                AddStatusItem("FROSTBITTEN", "NAME", "Frostbite", "CREATURES");
                AddStatusItem("FROSTBITTEN",
                    "TOOLTIP",
                    "Current external " + UI.PRE_KEYWORD + "Temperature" + UI.PST_KEYWORD + " is perilously low [<b>{ExternalTemperature}</b> / <b>{TargetTemperature}</b>]",
                    "CREATURES");
                AddStatusItem("FROSTBITTEN", "NOTIFICATION_NAME", "Frostbite", "CREATURES");
                AddStatusItem("FROSTBITTEN", "NOTIFICATION_TOOLTIP", "Freezing " + UI.PRE_KEYWORD + "Temperatures" + UI.PST_KEYWORD + " are hurting these Duplicants:", "CREATURES");
                
                Strings.Add("STRINGS.DUPLICANTS.ATTRIBUTES.FROSTBITETHRESHOLD.NAME", "Frostbite Threshold");
                Strings.Add("STRINGS.DUPLICANTS.ATTRIBUTES.FROSTBITETHRESHOLD.TOOLTIP", "Determines the " + UI.PRE_KEYWORD + "Temperature" + UI.PST_KEYWORD + " at which a Duplicant will get frostbitten.");
                
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
            }
        }
        
        // Add Frostbite that is Scalding but for cold
        [HarmonyPatch(typeof(ExternalTemperatureMonitor), "InitializeStates")]
        public static class ExternalTemperatureMonitor_InitializeStates_Patch
        {
            
            public const float BaseFrostbiteThreshold = 253.1f;
            public static TempMonitorStateMachine.State frostbite = new TempMonitorStateMachine.State();
            public static TempMonitorStateMachine.State transitionToFrostbite = new TempMonitorStateMachine.State();
            
            public static StatusItem Frostbitten = new StatusItem(
                "FROSTBITTEN",
                "CREATURES",
                string.Empty,
                StatusItem.IconType.Exclamation,
                NotificationType.DuplicantThreatening,
                true,
                OverlayModes.None.ID
            ) { resolveTooltipCallback = (str, data) =>
            {
                float externalTemperature = ((ExternalTemperatureMonitor.Instance) data).AverageExternalTemperature;
                float frostbiteThreshold = GetFrostbiteThreshold((ExternalTemperatureMonitor.Instance)data);
                str = str.Replace("{ExternalTemperature}", GameUtil.GetFormattedTemperature(externalTemperature));
                str = str.Replace("{TargetTemperature}", GameUtil.GetFormattedTemperature(frostbiteThreshold));
                return str;
            }};
            
            public static float GetFrostbiteThreshold(ExternalTemperatureMonitor.Instance data)
            {
                return data.attributes.GetValue("FrostbiteThreshold") + BaseFrostbiteThreshold;
            }

            public static bool isFrostbite(ExternalTemperatureMonitor.Instance data)
            {
                // a bit of a kludge, because for some reason Average External Temperature doesn't update for Frostbite
                // even though it does for Scalding.
                var exttemp = data.GetCurrentExternalTemperature;
                return exttemp < GetFrostbiteThreshold(data);
            }

            public static void Postfix(ExternalTemperatureMonitor __instance)
            {
                Frostbitten.AddNotification();
                var trav = Traverse.Create(frostbite);
                trav.SetField("sm", __instance);
                frostbite.defaultState = __instance.GetDefaultState();
                trav.SetField("root", __instance.root);
                
                trav = Traverse.Create(transitionToFrostbite);
                transitionToFrostbite.defaultState = __instance.GetDefaultState();
                trav.SetField("sm", __instance);
                trav.SetField("root", __instance.root);
                // if we're in frostbite temps, we might be frostbited.

                __instance.tooCool.Transition(transitionToFrostbite, isFrostbite);

                transitionToFrostbite
                    .Transition(__instance.tooCool, smi => !isFrostbite(smi))
                    .Transition(frostbite, smi =>
                    {
                        // If in a frostbite-valid state and stays there for 1s, we are now frostbitten
                        return isFrostbite(smi) && smi.timeinstate > 1.0;
                    });

                frostbite
                   .Transition(__instance.tooCool, smi => !isFrostbite(smi)) // to leave frostbite state
                   .ToggleExpression(Db.Get().Expressions.Cold) // brr
                   .ToggleThought(Db.Get().Thoughts.Cold) // I'm thinking of brr
                   .ToggleStatusItem(Frostbitten, smi => smi)
                   .Update("ColdDamage", (smi, dt) => smi.ScaldDamage(dt), UpdateRate.SIM_1000ms);
            }
        }

        // Frostbite attribute setup
        [HarmonyPatch(typeof(Database.Attributes), MethodType.Constructor, typeof(ResourceSet))]
        public static class Database_Attributes_Attributes_Patch
        {
            public static void Postfix(Database.Attributes __instance)
            {
                var frostbiteThreshold = new Attribute("FrostbiteThreshold", false, Attribute.Display.General, false);
                frostbiteThreshold.SetFormatter(new StandardAttributeFormatter(GameUtil.UnitClass.Temperature, GameUtil.TimeSlice.None));
                __instance.Add(frostbiteThreshold);
            }
        }
        
        // Add Atmo Suit frostbite immunity
        [HarmonyPatch(typeof(AtmoSuitConfig), "CreateEquipmentDef")]
        public static class AtmosuitConfig_CreateEquipmentDef_Patch
        {
            public static void Postfix(EquipmentDef __result)
            {
                __result.AttributeModifiers.Add(new AttributeModifier("FrostbiteThreshold", -1000f,
                    EQUIPMENT.PREFABS.ATMO_SUIT.NAME));
            }
        }
    }
}