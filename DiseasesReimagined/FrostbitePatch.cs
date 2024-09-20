using HarmonyLib;
using Klei.AI;
using PeterHan.PLib.Core;
using UnityEngine;
using PeterHan.PLib.Detours;
using STRINGS;

namespace DiseasesReimagined
{
    // Frostbite and scald damage depending on temperature
    [HarmonyPatch(typeof(ScaldingMonitor.Instance), "TemperatureDamage")]
    public static class ScaldingMonitor_Instance_TemperatureDamage_Patch
    {
        private static readonly IDetouredField<ScaldingMonitor.Instance, Health> HEALTH =
            PDetours.DetourField<ScaldingMonitor.Instance, Health>("health");

        private static readonly IDetouredField<ScaldingMonitor.Instance, OccupyArea> OCCUPY_AREA =
            PDetours.DetourField<ScaldingMonitor.Instance, OccupyArea>("occupyArea");

        // Gets the minimum external pressure of the cells occupied by the creature
        private static float GetCurrentExternalPressure(ScaldingMonitor.Instance instance)
        {
            int cell = Grid.PosToCell(instance.transform.position);
            var area = OCCUPY_AREA.Get(instance);
            float pressure = Grid.Pressure[cell];
            if (area != null)
            {
                foreach (var offset in area.OccupiedCellsOffsets)
                {
                    int newCell = Grid.OffsetCell(cell, offset);
                    if (Grid.IsValidCell(newCell))
                    {
                        float newPressure = Grid.Pressure[newCell];
                        if (newPressure < pressure)
                            pressure = newPressure;
                    }
                }
            }
            return pressure;
        }

        public static bool Prefix(ScaldingMonitor.Instance __instance, float dt,
                                  ref float ___lastScaldTime)
        {
            float now = Time.time;
            var hp = HEALTH.Get(__instance);
            // Avoid damage for pressures < threshold
            if (hp != null && now - ___lastScaldTime > 5.0f &&
                GetCurrentExternalPressure(__instance) > GermExposureTuning.MIN_PRESSURE)
            {
                float temp = __instance.AverageExternalTemperature;
                // For every 5 C outside the limits, damage 1HP more
                float damage = System.Math.Max(0.0f, GermExposureTuning.DAMAGE_PER_K *
                    (temp - __instance.GetScaldingThreshold())) + System.Math.Max(0.0f,
                    GermExposureTuning.DAMAGE_PER_K * (__instance.GetScoldingThreshold() -
                    temp));
                if (damage > 0.0f) {
                    hp.Damage((GermExposureTuning.DAMAGE_BASE + damage) * dt);
                    ___lastScaldTime = now;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Applied to MinionConfig to set the default frostbite temperature to something a little
    /// more diabolical.
    /// </summary>
    [HarmonyPatch(typeof(RationalAi), nameof(RationalAi.InitializeStates))]
    public static class RationalAi_InitializeStates_Patch
    {
        /// <summary>
        /// Applied after InitializeStates runs.
        /// </summary>
        internal static void Postfix(RationalAi __instance)
        {
            __instance.alive.Enter(AdjustScalding);
        }

        private static void AdjustScalding(RationalAi.Instance smi) {
            var go = smi.master.gameObject;
            if (go != null) {
                var scaldingSMI = go.GetSMI<ScaldingMonitor.Instance>();
                if (scaldingSMI.def is ScaldingMonitor.Def def) {
                    float oldValue = def.defaultScoldingTreshold;
                    def.defaultScoldingTreshold = GermExposureTuning.BASE_FROSTBITE_THRESHOLD;
                    // The SM is already started, injecting in between the instantiation and
                    // start call is very hard, so fix it up post hoc
                    go.GetAttributes()?.Get(Db.Get().Attributes.ScoldingThreshold)?.Add(
                        new AttributeModifier("ScoldingThreshold", GermExposureTuning.
                        BASE_FROSTBITE_THRESHOLD - oldValue, DUPLICANTS.STATS.
                        SKIN_DURABILITY.NAME));
                }
            }
        }
    }
}
