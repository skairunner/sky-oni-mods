using static SkyLib.Logger;

namespace OneWayVent
{
    public class OneWayVentPatch
    {
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
            }
        }
    }
}