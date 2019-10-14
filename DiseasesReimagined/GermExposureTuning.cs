using Klei.AI;
using PeterHan.PLib;
using System.Collections.Generic;

namespace DiseasesReimagined
{
    // Tuning for different types of integrated exposures.
    public static class GermExposureTuning
    {
        // The thresholds for each disease.
        private static readonly IDictionary<ExposureType, GermExposureThresholds> thresholds;

        static GermExposureTuning()
        {
            var types = TUNING.GERM_EXPOSURE.TYPES;
            if (types != null && types.Length > 4)
                // Note that Sicknesses contains lists of available exposure vectors which
                // must also be modified to add a new exposure method
                thresholds = new Dictionary<ExposureType, GermExposureThresholds>(16)
                {
                    // [0] = Food Poisoning (+2)
                    { types[0], new GermExposureThresholds(0, 100, 0) },
                    // [1] = Slimelung (+4)
                    { types[1], new GermExposureThresholds(0, 1000, 1000) },
                    // [2] = Zombie spores (-2)
                    { types[2], new GermExposureThresholds(100, 100, 100) },
                    // [3] = Allergies (0)
                    { types[3], new GermExposureThresholds(0, 0, 50) },
                    // [4] = Smelled flowers (0)
                    { types[4], new GermExposureThresholds(0, 0, 1) }
                };
            else
                PUtil.LogWarning("Germ exposure deactivated, invalid default germ exposures!");
        }

        // Gets the threshold for infecting a duplicant via each vector.
        public static GermExposureThresholds ThresholdsFor(ExposureType exposure)
        {
            if (!thresholds.TryGetValue(exposure, out GermExposureThresholds threshold))
                threshold = new GermExposureThresholds(0, 0, 0);
            return threshold;
        }
    }

    // Stores the thresholds for exposure via each vector.
    public sealed class GermExposureThresholds
    {
        // The threshold for contact exposure. Also used for sunlight exposure.
        public int ContactThreshold { get; }

        // The threshold for ingestion exposure.
        public int IngestionThreshold { get; }

        // The threshold for inhalation exposure.
        public int InhalationThreshold { get; }

        public GermExposureThresholds(int contact, int ingestion, int inhalation)
        {
            ContactThreshold = contact;
            IngestionThreshold = ingestion;
            InhalationThreshold = inhalation;
        }

        // Returns the smallest nonzero threshold.
        public int GetMinThreshold()
        {
            int min = int.MaxValue;
            if (ContactThreshold > 0 && ContactThreshold < min)
                min = ContactThreshold;
            if (IngestionThreshold > 0 && IngestionThreshold < min)
                min = IngestionThreshold;
            if (InhalationThreshold > 0 && InhalationThreshold < min)
                min = InhalationThreshold;
            // No threshold
            if (min == int.MaxValue)
                min = 0;
            return min;
        }

        // Retrieves the threshold for a given infection vector.
        public int GetThreshold(Sickness.InfectionVector vector)
        {
            int threshold = 0;
            switch (vector)
            {
            case Sickness.InfectionVector.Contact:
            case Sickness.InfectionVector.Exposure:
                // Exposure is only used for sunburn
                threshold = ContactThreshold;
                break;
            case Sickness.InfectionVector.Digestion:
                threshold = IngestionThreshold;
                break;
            case Sickness.InfectionVector.Inhalation:
                threshold = InhalationThreshold;
                break;
            }
            return threshold;
        }

        public override string ToString()
        {
            return "GermExposureThresholds[contact={0:D},ingestion={1:D},inhalation={2:D}]".F(
                ContactThreshold, IngestionThreshold, InhalationThreshold);
        }
    }
}
