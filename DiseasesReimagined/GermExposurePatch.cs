using Database;
using HarmonyLib;
using Klei.AI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DiseasesReimagined
{
    // Replaces GermExposureMonitor with a sensible version that integrates exposure over a
    // day to determine infection level.
    public static class GermExposurePatch
    {
        // Qualifier for total germ count
        public static LocString TOTAL_GERMS = " Total Germs";

        // Titles and tool tips for the revamped contraction rate screen
        public static LocString THRESHOLD_TITLE = "CONTRACTION THRESHOLDS";
        public static LocString THRESHOLD_IMMUNE = "Immune";
        public static LocString THRESHOLD_TOOLTIP_1 = "{0} will catch {1} upon any exposure to its germs";
        public static LocString THRESHOLD_TOOLTIP = "{1} must be exposed to {0:D} germs to have a chance at catching {2}";

        private static readonly StringBuilder EXPOSURE_TEXT = new StringBuilder(256);

        // Creates one line of the immune system information panel.
        private static void CreateOneImmuneInfo(Disease disease, GameObject target,
            GermIntegrator integrator, CollapsibleDetailContentPanel panel)
        {
            var exposure = GameUtil.GetExposureTypeForDisease(disease);
            var sickness = GameUtil.GetSicknessForDisease(disease);
            if (sickness == null)
                return;

            ICollection<string> required = exposure.required_traits, excluded = exposure.
                excluded_traits, noEffects = exposure.excluded_effects;
            var traits = target.GetComponent<Traits>();
            var effects = target.GetComponent<Effects>();
            // The part we actually changed
            float threshold = Mathf.Ceil(GermIntegrator.AdjustedThreshold(GermExposureTuning.
                ThresholdsFor(exposure).GetMinThreshold(), integrator.GetResistance(exposure)));
            string tooltip = "";
            // Check for required traits to catch the disease
            if (required != null && required.Count > 0)
            {
                string traitList = "";
                foreach (string trait in required)
                    if (!traits.HasTrait(trait))
                    {
                        if (traitList.Length > 0)
                            traitList += ", ";
                        traitList += Db.Get().traits.Get(trait).Name;
                    }
                if (traitList.Length > 0)
                {
                    // Immune: missing required traits
                    threshold = 0.0f;
                    tooltip = string.Format(STRINGS.DUPLICANTS.DISEASES.
                        IMMUNE_FROM_MISSING_REQUIRED_TRAIT, traitList);
                }
            }
            // Check for traits that prevent catching it
            if (excluded != null && excluded.Count > 0)
            {
                string traitList = "";
                foreach (string trait in excluded)
                    if (traits.HasTrait(trait))
                    {
                        if (traitList.Length > 0)
                            traitList += ", ";
                        traitList += Db.Get().traits.Get(trait).Name;
                    }
                if (traitList.Length > 0)
                {
                    // Immune: blocking trait
                    threshold = 0.0f;
                    if (tooltip.Length > 0)
                        tooltip += "\n";
                    tooltip += string.Format(STRINGS.DUPLICANTS.DISEASES.
                        IMMUNE_FROM_HAVING_EXLCLUDED_TRAIT, traitList);
                }
            }
            // Check for effects that prevent catching it
            if (noEffects != null && noEffects.Count > 0)
            {
                string effectList = "";
                foreach (string effect in noEffects)
                    if (effects.HasEffect(effect))
                    {
                        if (effectList.Length > 0)
                            effectList += ", ";
                        effectList += Db.Get().effects.Get(effect).Name;
                    }
                if (effectList.Length > 0)
                {
                    // Immune: blocking effect
                    threshold = 0.0f;
                    if (tooltip.Length > 0)
                        tooltip += "\n";
                    tooltip += string.Format(STRINGS.DUPLICANTS.DISEASES.
                        IMMUNE_FROM_HAVING_EXCLUDED_EFFECT, effectList);
                }
            }
            // Update the label and tooltip
            if (tooltip.Length == 0)
            {
                if (threshold > 1.0f)
                    tooltip = string.Format(THRESHOLD_TOOLTIP, GameUtil.GetFormattedSimple(
                        threshold), target.GetProperName(), sickness.Name);
                else
                    tooltip = string.Format(THRESHOLD_TOOLTIP_1, target.GetProperName(),
                        sickness.Name);
            }
            panel.SetLabel("disease_" + disease.Id, "    • " + disease.Name + ": " +
                (threshold == 0.0f ? THRESHOLD_IMMUNE.text : GameUtil.GetFormattedSimple(
                threshold)), tooltip);
        }

        // Creates the immune system information panel.
        private static void CreateImmuneInfo(CollapsibleDetailContentPanel immuneSystemPanel,
            GameObject target)
        {
            var integrator = target.GetComponent<GermIntegrator>();
            if (integrator != null)
            {
                var diseases = Db.Get().Diseases;
                // Create the immune system panel
                immuneSystemPanel.SetTitle(THRESHOLD_TITLE);
                immuneSystemPanel.SetLabel("germ_resistance", Db.Get().Attributes.
                    GermResistance.Name + ": " + integrator.GetDupeResistance(), STRINGS.
                    DUPLICANTS.ATTRIBUTES.GERMRESISTANCE.DESC);
                for (int i = 0; i < diseases.Count; i++)
                    CreateOneImmuneInfo(diseases[i], target, integrator, immuneSystemPanel);
            }
            immuneSystemPanel.Commit();
        }

        // Resolves the text for the "exposed" status item
        private static string ResolveExposureText(string str, object data)
        {
            var db = Db.Get();
            var exposureStatus = data as GermExposureMonitor.ExposureStatusData;
            var exposure = exposureStatus?.exposure_type;
            if (exposure == null)
                throw new ArgumentNullException(nameof(exposure));
            var dupe = exposureStatus.owner.gameObject;
            if (dupe != null)
            {
                var smi = dupe.GetSMI<GermExposureMonitor.Instance>();
                string name = db.Sicknesses.Get(exposure.sickness_id).Name, bonus;
                float baseResist = exposure.base_resistance, resist, chance;
                if (dupe.TryGetComponent(out GermIntegrator manager))
                {
                    bonus = GameUtil.GetFormattedSimple(manager.GetTotalGerms(exposure)) +
                        TOTAL_GERMS;
                    resist = manager.GetResistance(exposure);
                    chance = manager.GetWorstInfectionChance(exposure, resist);
                }
                else
                {
                    // If manager is unavailable, substitute placeholders
                    bonus = GameUtil.GetFormattedSimple(0);
                    resist = 0.0f;
                    chance = 0.5f;
                }
                int tier = (int)smi.GetExposureTier(exposure.germ_id) - 1;
                // Severity: low, mild, or high
                str = EXPOSURE_TEXT.Clear().Append(str).
                    Replace("{Severity}", STRINGS.DUPLICANTS.STATUSITEMS.EXPOSEDTOGERMS.
                        EXPOSURE_TIERS[tier]).
                    Replace("{Sickness}", name).
                    Replace("{Source}", exposureStatus.owner.GetLastDiseaseSource(exposure.
                        germ_id)).
                    Replace("{Base}", GameUtil.GetFormattedSimple(baseResist)).
                    Replace("{Dupe}", GameUtil.GetFormattedSimple(resist - baseResist)).
                    Replace("{Total}", GameUtil.GetFormattedSimple(resist)).
                    Replace("{ExposureLevelBonus}", bonus).
                    Replace("{Chance}", GameUtil.GetFormattedPercent(chance * 100f)).
                    ToString();
            }
            return str;
        }

        /// <summary>
        /// Applied to AdditionalDetailsPanel to replace the immune system information with our
        /// mod's version.
        /// </summary>
        [HarmonyPatch(typeof(AdditionalDetailsPanel), "RefreshImuneSystemPanel")]
        public static class AdditionalDetailsPanel_RefreshImuneSystemPanel_Patch
        {
            /// <summary>
            /// Applied before RefreshImuneSystemPanel runs.
            /// </summary>
            internal static bool Prefix(CollapsibleDetailContentPanel targetPanel, GameObject targetEntity)
            {
                CreateImmuneInfo(targetPanel, targetEntity);
                return false;
            }
        }

        /// <summary>
        /// Applied to DuplicantStatusItems to adjust the tool tip for disease contraction.
        /// </summary>
        [HarmonyPatch(typeof(DuplicantStatusItems), "CreateStatusItems")]
        public static class DuplicantStatusItems_CreateStatusItems_Patch
        {
            /// <summary>
            /// Applied after CreateStatusItems runs.
            /// </summary>
            internal static void Postfix(DuplicantStatusItems __instance)
            {
                __instance.ExposedToGerms.resolveStringCallback = ResolveExposureText;
            }
        }

        /// <summary>
        /// Applied to GermExposureMonitor to replace the disease injection with our integrated
        /// exposure monitor.
        /// </summary>
        [HarmonyPatch(typeof(GermExposureMonitor.Instance), "InjectDisease")]
        public static class GermExposureMonitor_Instance_InjectDisease_Patch
        {
            /// <summary>
            /// Applied before InjectDisease runs.
            /// </summary>
            internal static bool Prefix(GermExposureMonitor.Instance __instance, int count,
                Disease disease, Tag source, Sickness.InfectionVector vector)
            {
                var go = __instance.gameObject;
                bool run = go == null;
                if (!run) {
                    var manager = go.AddOrGet<GermIntegrator>();
                    manager.InjectDisease(__instance, disease, count, source, vector);
                }
                return run;
            }
        }

        /// <summary>
        /// Applied to GermExposureMonitor.Instance to infect using integrated exposure when
        /// the Duplicant wakes up.
        /// </summary>
        [HarmonyPatch(typeof(GermExposureMonitor.Instance), "OnSleepFinished")]
        public static class GermExposureMonitor_Instance_OnSleepFinished_Patch
        {
            /// <summary>
            /// Applied before OnSleepFinished runs.
            /// </summary>
            internal static bool Prefix(GermExposureMonitor.Instance __instance)
            {
                var go = __instance.gameObject;
                bool run = go == null;
                if (!run) {
                    var manager = go.AddOrGet<GermIntegrator>();
                    manager.OnSleepFinished(__instance);
                }
                return run;
            }
        }

        /// <summary>
        /// Applied to MinionConfig to add disease integration monitors to every Duplicant.
        /// </summary>
        [HarmonyPatch(typeof(MinionConfig), "CreatePrefab")]
        public static class MinionConfig_CreatePrefab_Patch
        {
            /// <summary>
            /// Applied after CreatePrefab runs.
            /// </summary>
            internal static void Postfix(GameObject __result)
            {
                if (__result != null)
                    __result.AddOrGet<GermIntegrator>();
            }
        }
    }
}
