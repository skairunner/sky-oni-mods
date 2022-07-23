using HarmonyLib;

using KMod;

using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace Drains
{
    public class Mod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            SkyLib.Logger.StartLogging();
            PUtil.InitLibrary(false);
            POptions pOpt = new POptions();
            pOpt.RegisterOptions(this, typeof(DrainOptions));

            base.OnLoad(harmony);
        }
    }
}
