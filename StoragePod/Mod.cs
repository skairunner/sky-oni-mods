using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Options;

namespace StoragePod
{
    public class Mod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            PUtil.InitLibrary(false);
            POptions opt = new POptions();
            opt.RegisterOptions(this, typeof(StoragePodOptions));

            base.OnLoad(harmony);
        }
    }
}
