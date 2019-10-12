using Harmony;
using PeterHan.PLib;
using System;
using UnityEngine;

namespace DiseasesReimagined
{
    // Patches for cross-mod compatibility
    public static class CompatPatch
    {
        internal static void CompatPatches(HarmonyInstance instance)
        {
#if DEBUG
            PUtil.LogDebug("Starting compatibility patches");
#endif
            // QfS: observe cooldown
            try
            {
                var sinkType = Type.GetType("PeterHan.QueueForSinks.SinkCheckpoint, QueueForSink",
                    false);
                if (sinkType != null)
                {
                    PUtil.LogDebug("Compatibility patching for Queue for Sinks");
                    instance.Patch(sinkType, "MustStop", null, new HarmonyMethod(
                        typeof(CompatPatch), nameof(MustStop_Postfix)));
                }
            }
            catch (TypeLoadException)
            {
                // Ignore
#if DEBUG
                PUtil.LogDebug("Queue For Sinks not found.");
#endif
            }
            catch (Exception e)
            {
                PUtil.LogExcWarn(e);
            }
        }

        private static void MustStop_Postfix(GameObject reactor, ref bool __result)
        {
            // Prevent OCD hand washing by allowing duplicants who cannot use a sink again to
            // pass QfS
            var cooldown = reactor.GetComponent<WashCooldownComponent>();
            if (cooldown != null && !cooldown.CanWash)
                __result = false;
        }
    }
}
