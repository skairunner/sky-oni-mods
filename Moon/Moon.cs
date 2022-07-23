using Harmony;
using PeterHan.PLib;
using ProcGen;
using static SkyLib.Logger;

namespace Moon
{
    public static class Moon
    {
        public static void OnLoad()
        {
            StartLogging();
        }
        
        // Load world strings
        [HarmonyPatch(typeof(Db), "Initialize")]
        public static class Db_Initialize_Patch
        {
            static void Postfix()
            {
                Strings.Add("STRINGS.WORLDS.MOON.NAME", "The Moon?");
                Strings.Add("STRINGS.WORLDS.MOON.DESCRIPTION", "SPAAAAACE");
            }
        }
    }
}
