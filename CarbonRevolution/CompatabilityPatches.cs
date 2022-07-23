using PeterHan.PLib;

namespace CarbonRevolution
{
    public class CompatabilityPatches
    {
        public static bool GEN_STORE_OUTPUTS = false;

        [PLibMethod(RunAt.AfterModsLoad, RequireType = "Nightinggale.PipedOutput.ApplyExhaust")]
        // Check for Piped Output
        public static void DoPatches()
        {
            GEN_STORE_OUTPUTS = true;
        }
    }
}
