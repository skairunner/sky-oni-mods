using HarmonyLib;
using PeterHan.PLib.PatchManager;
using UnityEngine;

namespace DiseasesReimagined
{
    // Patches for cross-mod compatibility
    public static class CompatPatch
    {
        [PLibPatch(RunAt.AfterModsLoad, "MustStop", IgnoreOnFail = true, PatchType =
            HarmonyPatchType.Postfix, RequireType = "PeterHan.QueueForSinks.SinkCheckpoint",
            RequireAssembly = "QueueForSink")]
        internal static void QueueForSinksPatch(GameObject reactor, ref bool __result)
        {
            // Prevent OCD hand washing by allowing duplicants who cannot use a sink again to
            // pass QfS
            if (reactor.TryGetComponent(out WashCooldownComponent cooldown) && !cooldown.
                    CanWash)
                __result = false;
        }
    }
}
