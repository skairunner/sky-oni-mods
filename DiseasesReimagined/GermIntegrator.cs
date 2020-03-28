using Harmony;
using Klei.AI;
using KSerialization;
using PeterHan.PLib;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

using DiseaseSourceInfo = GermExposureMonitor.Instance.DiseaseSourceInfo;
using ExposureState = GermExposureMonitor.ExposureState;

namespace DiseasesReimagined
{
    // Replaces GermExposureMonitor with a sensible integrated exposure system.
    [SerializationConfig(MemberSerialization.OptIn)]
    public sealed class GermIntegrator : KMonoBehaviour, ISaveLoadable
    {
        // All sickness vectors that can be used to get sick.
        private static readonly Sickness.InfectionVector[] ALL_VECTORS =
        {
            Sickness.InfectionVector.Contact, Sickness.InfectionVector.Digestion,
            Sickness.InfectionVector.Exposure, Sickness.InfectionVector.Inhalation
        };

        // Adjusts the threshold for germ resistance.
        public static float AdjustedThreshold(int threshold, float resistance)
        {
            return (resistance > 0.0f) ? threshold * (1.0f + resistance) : threshold /
                (1.0f - resistance);
        }

        // Calculates the infection chance for a given integrated exposure.
        public static float GetInfectionChance(GermExposure ge, int total, float resistance)
        {
            /*
             * Allow one exposure count with 0 effects, then use exponential function:
             * 
             * Chance to infect = 1.0 - e^(FACTOR * -(totalOverThreshold / threshold))
             * 
             * With factor at 0.5:
             * Double exposure count ~ 40 %
             * Triple exposure count ~ 63 %
             * Quadruple exposure count ~ 77 %
             * 
             * Resistance adjusts the thresholds
             */
            int threshold = GermExposureTuning.ThresholdsFor(ge.Exposure).GetThreshold(ge.
                Vector);
            float adjThres = AdjustedThreshold(threshold, resistance), overTotal = Mathf.Floor(
                total - adjThres);
            // threshold of 1 is instant infection
            return (overTotal > 0.0f) ? ((threshold < 2) ? 1.0f : (1.0f - Mathf.Exp(
                -GermExposureTuning.CONTRACT_FACTOR * overTotal / adjThres))) : 0.0f;
        }

        // Cached attribute for germ resistance.
        private Klei.AI.Attribute germResistAttr;

        // Unserialized but fast way to look up the germs.
        private readonly IDictionary<GermExposure, int> integratedExposure;

        // Serialized way to look up the germs.
        [Serialize]
        private readonly List<SerializedGermExposure> totalExposure;

        // This is populated automatically
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649
        [MyCmpGet]
        private Traits traits;
#pragma warning restore CS0649
#pragma warning restore IDE0044 // Add readonly modifier

        public GermIntegrator()
        {
            integratedExposure = new Dictionary<GermExposure, int>(8);
            totalExposure = new List<SerializedGermExposure>(16);
        }

        // Performs the dirty work of counting germs on a Duplicant.
        private void DoInjectDisease(GermExposureMonitor.Instance monitor, Disease disease,
            int count, Tag source, GermExposure ge)
        {
            var exposure = ge.Exposure;
            int threshold = GermExposureTuning.ThresholdsFor(exposure).GetThreshold(ge.Vector);
            if (threshold > 0 && gameObject != null)
            {
                string germ = ge.GermID;
                var state = monitor.GetExposureState(germ);
                // Eligible addition to the integrated exposure calculations
                if (integratedExposure.TryGetValue(ge, out int total))
                    total += count;
                else
                    total = count;
#if DEBUG
                PUtil.LogDebug("{0} has {1:D} germs of {2} by {3} (threshold = {4:D})".F(
                    gameObject.name, count, germ, ge.Vector, threshold));
#endif
                integratedExposure[ge] = total;
                // Resistance is required to move the thresholds which is used for tier
                // calculation
                float resist = GetResistance(exposure), adjThres = AdjustedThreshold(
                    threshold, resist), tier = total / adjThres;
                switch (state)
                {
                case ExposureState.None:
                case ExposureState.Contact:
                    // Update location where they encountered the germ
                    monitor.lastDiseaseSources[disease.id] = new DiseaseSourceInfo(source,
                        ge.Vector, GetInfectionChance(ge, total, resist), transform.
                        GetPosition());
                    if (total > adjThres)
                    {
                        if (exposure.infect_immediately)
                        {
                            // The ultimate evil flag
#if DEBUG
                            PUtil.LogDebug("Infecting {0} with {1}".F(gameObject.name, germ));
#endif
                            monitor.InfectImmediately(exposure);
                        }
                        else
                        {
                            // "Exposed to slimelung"
                            monitor.SetExposureState(germ, ExposureState.Exposed);
                            monitor.SetExposureTier(germ, tier);
#if DEBUG
                            PUtil.LogDebug("{0} exposed to {1} tier {2:F1}".F(gameObject.name,
                                germ, tier));
#endif
                        }
                    }
                    else if (state == ExposureState.None)
                        // "Contact with slimelung"
                        monitor.SetExposureState(germ, ExposureState.Contact);
                    break;
                case ExposureState.Exposed:
                    // Upgrade the visual tier
                    if (total > threshold && tier > monitor.GetExposureTier(germ))
                    {
                        monitor.SetExposureTier(germ, tier);
#if DEBUG
                        PUtil.LogDebug("{0} exposure of {1} upgraded to {2:F1}".F(gameObject.
                            name, germ, tier));
#endif
                    }
                    break;
                case ExposureState.Contracted:
                case ExposureState.Sick:
                    // Already sick
                    break;
                default:
                    break;
                }
            }
        }

        // Gets the duplicant disease resistance.
        internal float GetDupeResistance()
        {
            return germResistAttr.Lookup(gameObject).GetTotalValue();
        }

        // Gets the total resistance to a disease factoring in disease base and dupe traits.
        internal float GetResistance(ExposureType exposure)
        {
            return exposure.base_resistance + GetDupeResistance();
        }

        // Retrieves the total germ count for a given germ type across ALL infection vectors.
        internal int GetTotalGerms(ExposureType exposure)
        {
            int total = 0;
            if (exposure == null)
                throw new ArgumentNullException("exposure");
            foreach (var vector in ALL_VECTORS)
                if (integratedExposure.TryGetValue(new GermExposure(vector, exposure),
                        out int byVector))
                    total += byVector;
            return total;
        }

        // Retrieves the worst chance for germ infection across all vectors.
        internal float GetWorstInfectionChance(ExposureType exposure, float resist)
        {
            float worstChance = 0.0f;
            if (exposure == null)
                throw new ArgumentNullException("exposure");
            foreach (var vector in ALL_VECTORS)
            {
                var ge = new GermExposure(vector, exposure);
                if (integratedExposure.TryGetValue(ge, out int byVector))
                {
                    // Resistance should be calculated by the SMI
                    float chance = GetInfectionChance(ge, byVector, resist);
                    if (chance > worstChance)
                        worstChance = chance;
                }
            }
            return worstChance;
        }

        // Called by GermExposureMonitor when a Duplicant encounters germs.
        internal void InjectDisease(GermExposureMonitor.Instance monitor, Disease disease,
            int count, Tag source, Sickness.InfectionVector vector)
        {
            var sicknesses = Db.Get().Sicknesses;
            foreach (var exposure in TUNING.GERM_EXPOSURE.TYPES)
                if (disease.id == exposure.germ_id && IsExposureValidForTraits(exposure))
                {
                    // Null sickness ID for smelled flowers
                    string id = exposure.sickness_id;
                    var sickness = (id == null) ? null : sicknesses.Get(id);
                    if (sickness == null || sickness.infectionVectors.Contains(vector))
                        DoInjectDisease(monitor, disease, count, source, new GermExposure(
                            vector, exposure));
                }
        }

        // Checks to see if the Duplicant is immune to a certain exposure.
        internal bool IsExposureValidForTraits(ExposureType exposure)
        {
            bool valid = true;
            ICollection<string> required = exposure.required_traits, excluded = exposure.
                excluded_traits, noEffects = exposure.excluded_effects;
            if (required != null && required.Count > 0)
                foreach (string traitID in required)
                    if (!traits.HasTrait(traitID))
                    {
                        valid = false;
                        break;
                    }
            if (valid && excluded != null && excluded.Count > 0)
                foreach (string traitID in excluded)
                    if (traits.HasTrait(traitID))
                    {
                        valid = false;
                        break;
                    }
            if (valid && noEffects != null && noEffects.Count > 0)
            {
                var effects = gameObject.GetComponent<Effects>();
                if (effects != null)
                    foreach (string effect_id in noEffects)
                        if (effects.HasEffect(effect_id))
                        {
                            valid = false;
                            break;
                        }
            }
            return valid;
        }

        // When loaded, this method is called.
        [OnDeserialized]
        internal void OnDeserialized()
        {
            integratedExposure.Clear();
            foreach (var info in totalExposure)
            {
                string germID = info.germID;
                ExposureType exposure = null;
                // Look for a matching germ exposure
                foreach (var candidate in TUNING.GERM_EXPOSURE.TYPES)
                    if (candidate.germ_id == germID)
                    {
                        exposure = candidate;
                        break;
                    }
                if (exposure == null)
                    // Diagnostic for removed diseases
                    PUtil.LogWarning("Found unknown germ exposure ID {0} when loading".F(
                        germID));
                else
                    integratedExposure.Add(new GermExposure(info.vector, exposure), info.total);
            }
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            germResistAttr = Db.Get().Attributes.GermResistance;
        }

        // Use a valid class when serializing!
        [OnSerializing]
        internal void OnSerializing()
        {
            totalExposure.Clear();
            foreach (var pair in integratedExposure)
            {
                var ge = pair.Key;
                totalExposure.Add(new SerializedGermExposure(ge.GermID, ge.Vector, pair.Value));
            }
        }

        // When the Duplicant wakes up, check the chances and infect with valid diseases
        internal void OnSleepFinished(GermExposureMonitor.Instance monitor)
        {
            SeededRandom random;
            // Use the same state as the vanilla game does
            var inst = GermExposureTracker.Instance;
            if (inst == null)
                random = new SeededRandom(GameClock.Instance.GetCycle());
            else
                random = Traverse.Create(inst).GetField<SeededRandom>("rng");
            foreach (var pair in integratedExposure)
            {
                var ge = pair.Key;
                var exposure = ge.Exposure;
                // Avoid double dipping on zombie spores
                if (!exposure.infect_immediately && gameObject != null)
                {
                    string germ = ge.GermID;
                    int total = pair.Value;
#if DEBUG
                    PUtil.LogDebug("{0} waking up with {1:D} total exposure to {2} via {3}".F(
                        gameObject.name, total, germ, ge.Vector));
#endif
                    switch (monitor.GetExposureState(germ))
                    {
                    case ExposureState.Exposed:
                        // Calculate resistance now
                        if (IsExposureValidForTraits(exposure) && random.NextDouble() <
                            GetInfectionChance(ge, total, GetResistance(exposure)))
                        {
                            // Gotcha!
#if DEBUG
                            PUtil.LogDebug("Infecting {0} with {1}".F(gameObject.name, germ));
#endif
                            monitor.SetExposureState(germ, ExposureState.Sick);
                            monitor.InfectImmediately(exposure);
                        }
                        else
                            // Not this time...
                            monitor.SetExposureState(germ, ExposureState.None);
                        monitor.SetExposureTier(germ, 0.0f);
                        break;
                    case ExposureState.Contracted:
                        // Make them sick to avoid getting stuck here (e.g. if DR was installed
                        // on an existing save)
                        monitor.SetExposureState(germ, ExposureState.Sick);
                        monitor.SetExposureTier(germ, 0.0f);
                        monitor.InfectImmediately(exposure);
                        break;
                    case ExposureState.Sick:
                        // Already sick
                        break;
                    case ExposureState.Contact:
                        // Reset state to none if contact and no exposure
                        monitor.SetExposureState(germ, ExposureState.None);
                        break;
                    default:
                        // None = continue on
                        break;
                    }
                }
            }
            ResetExposure();
        }

        public void ResetExposure()
        {
            integratedExposure.Clear();
        }

        // Serializes the status of Duplicant infection.
        [SerializationConfig(MemberSerialization.OptOut)]
        private sealed class SerializedGermExposure : ISaveLoadable
        {
            // The ID of the germ that this Duplicant was exposed to.
            public string germID;

            // The total germ count exposed.
            public int total;

            // The vector of infection.
            public Sickness.InfectionVector vector;

            internal SerializedGermExposure(string germID, Sickness.InfectionVector vector,
                int total)
            {
                this.germID = germID;
                this.total = total;
                this.vector = vector;
            }
        }
    }

    // A properly hashable class which includes the germ type and exposure vector.
    public sealed class GermExposure
    {
        // The infector vector where the germs came in.
        public Sickness.InfectionVector Vector { get; }

        // The exposure type information including the germ and sickness name.
        public ExposureType Exposure { get; }

        // The germ ID exposed.
        public string GermID
        {
            get
            {
                return Exposure.germ_id;
            }
        }

        public GermExposure(Sickness.InfectionVector vector, ExposureType exposure)
        {
            Exposure = exposure ?? throw new ArgumentNullException("exposure");
            Vector = vector;
        }

        public override bool Equals(object obj)
        {
            return obj is GermExposure other && other.Vector == Vector && other.Exposure.
                germ_id == Exposure.germ_id;
        }

        public override int GetHashCode()
        {
            return Exposure.germ_id.GetHashCode() * 37 + (int)Vector;
        }

        public override string ToString()
        {
            return "GermExposure[vector={0},germ={1}]".F(Vector, Exposure.germ_id);
        }
    }
}
