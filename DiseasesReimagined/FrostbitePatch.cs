using STRINGS;
using static SkyLib.OniUtils;
using Harmony;
using Klei.AI;
using PeterHan.PLib;

namespace DiseasesReimagined
{
    using TempMonitorStateMachine = GameStateMachine<ExternalTemperatureMonitor, ExternalTemperatureMonitor.Instance,
        IStateMachineTarget,
        object>;

    // Patches for frostbite-related things
    class FrostbitePatch
    {
        // Add strings and status items for Frostbite
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                AddStatusItem("FROSTBITTEN", "NAME", "Frostbite", "CREATURES");
                AddStatusItem("FROSTBITTEN",
                    "TOOLTIP",
                    "Current external " + UI.PRE_KEYWORD + "Temperature" + UI.PST_KEYWORD + " is perilously low [<b>{ExternalTemperature}</b> / <b>{TargetTemperature}</b>]",
                    "CREATURES");
                AddStatusItem("FROSTBITTEN", "NOTIFICATION_NAME", "Frostbite", "CREATURES");
                AddStatusItem("FROSTBITTEN", "NOTIFICATION_TOOLTIP", "Freezing " + UI.PRE_KEYWORD + "Temperatures" + UI.PST_KEYWORD + " are hurting these Duplicants:", "CREATURES");
                
                Strings.Add("STRINGS.DUPLICANTS.ATTRIBUTES.FROSTBITETHRESHOLD.NAME", "Frostbite Threshold");
                Strings.Add("STRINGS.DUPLICANTS.ATTRIBUTES.FROSTBITETHRESHOLD.TOOLTIP", "Determines the " + UI.PRE_KEYWORD + "Temperature" + UI.PST_KEYWORD + " at which a Duplicant will get frostbitten.");
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