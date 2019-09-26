using Harmony;
using PeterHan.PLib.Options;
using static SkyLib.Logger;
using Newtonsoft.Json;

namespace PrintPodRefund
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PrintingPodRefundSettings
    {
        [PeterHan.PLib.Option("Refund fraction", "Between 0 and 1. How much of the recharge time to refund when you reject a package. Smaller numbers means less refunded.")]
        [PeterHan.PLib.Limit(0, 1)]
        [JsonProperty]
        public float refundFraction { get; set; }

        public PrintingPodRefundSettings()
        {
            refundFraction = 0.66f;
        }

        public static PrintingPodRefundSettings GetSettings()
        {
            return POptions.ReadSettings<PrintingPodRefundSettings>();
        }
    }
    public class PrintingPodRefundPatch
    {
        public static string ModName = "PrintingPodRecharges";
        public static bool didStartUp_Building = false;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging(ModName);
                PeterHan.PLib.PUtil.LogModInit();
                POptions.RegisterOptions(typeof(PrintingPodRefundSettings));
            }
        }

        static float GetImmigrationTime()
        {
            var imm = Immigration.Instance;
            var idx = Traverse.Create(imm).Field("spawnIdx").GetValue<int>();
            var index = System.Math.Min(idx, imm.spawnInterval.Length - 1);
            return imm.spawnInterval[index];
        }

        [HarmonyPatch(typeof(Telepad), "RejectAll")]
        public static class Telepad_RejectAll_Path
        {
            public static void Postfix()
            {
                // Refund time if rejected all options.
                var settings = PrintingPodRefundSettings.GetSettings();
                if (settings == null)
                {
                    settings = new PrintingPodRefundSettings();
                }
                var f = settings.refundFraction;
                var waittime = GetImmigrationTime();
                Immigration.Instance.timeBeforeSpawn = (1 - f) * waittime;
            }
        }
    }
}
