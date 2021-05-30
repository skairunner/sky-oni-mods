using System;
using Harmony;
using Newtonsoft.Json;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using static SkyLib.Logger;

namespace PrintingPodRefund
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PrintingPodRefundSettings
    {
        public PrintingPodRefundSettings()
        {
            refundFraction = 0.66f;
        }

        [Option("Refund fraction",
            "Between 0 and 1. How much of the recharge time to refund when you reject a package. Smaller numbers means less refunded.")]
        [Limit(0, 1)]
        [JsonProperty]
        public float refundFraction { get; set; }

        public static PrintingPodRefundSettings GetSettings()
        {
            return POptions.ReadSettings<PrintingPodRefundSettings>();
        }
    }

    public class PrintingPodRefundPatch
    {
        public static bool didStartUp_Building = false;

        private static float GetImmigrationTime()
        {
            var imm = Immigration.Instance;
            var idx = Traverse.Create(imm).Field("spawnIdx").GetValue<int>();
            var index = Math.Min(idx, imm.spawnInterval.Length - 1);
            return imm.spawnInterval[index];
        }

        public static void OnLoad()
        {
            StartLogging();
            PUtil.InitLibrary(false);
            POptions.RegisterOptions(typeof(PrintingPodRefundSettings));
        }

        [HarmonyPatch(typeof(Telepad), "RejectAll")]
        public static class Telepad_RejectAll_Path
        {
            public static void Postfix()
            {
                // Refund time if rejected all options.
                var settings = PrintingPodRefundSettings.GetSettings();
                if (settings == null) settings = new PrintingPodRefundSettings();

                var f = settings.refundFraction;
                var waittime = GetImmigrationTime();
                Immigration.Instance.timeBeforeSpawn = (1 - f) * waittime;
            }
        }
    }
}
