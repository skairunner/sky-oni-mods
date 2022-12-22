using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Lighting;
using PeterHan.PLib.PatchManager;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace ExpandedLights
{
    public class ExpandedLightsPatch : UserMod2
    {
        public static bool didStartUp_Building;
        public static bool didStartUp_Db;

        /// <summary>
        ///     Light shape: Directed cone according to component rotation
        /// </summary>
        public static ILightShape DirectedCone,
            Beam5,
            SmoothCircle,
            OffsetCone,
            FixedSemi,
            Semicircle,
            OffsetSemi;

        public override void OnLoad(Harmony harmony)
        {
            base.OnLoad(harmony);
            PUtil.InitLibrary(false);
            var patchManager = new PPatchManager(harmony);
            patchManager.RegisterPatchClass(typeof(ExpandedLightsPatch));
            StartLogging();

            var lightManager = new PLightManager();
            DirectedCone = lightManager.Register("SkyLib.LightShape.Cone", LightDefs.LightCone);
            Beam5 = lightManager.Register("SkyLib.LightShape.Beam5", LightDefs.LinearLight5);
            SmoothCircle = lightManager.Register("SkyLib.LightShape.Circle", LightDefs.LightCircle, LightShape.Circle);
            OffsetCone = lightManager.Register("SkyLib.LightShape.OffsetCone", LightDefs.OffsetCone);
            FixedSemi = lightManager.Register("SkyLib.LightShape.FixedSemi", LightDefs.FixedLightSemicircle, LightShape.Cone);
            Semicircle = lightManager.Register("SkyLib.LightShape.Semicircle", LightDefs.LightSemicircle);
            OffsetSemi = lightManager.Register("SkyLib.LightShape.OffsetSemi", LightDefs.OffsetSemicircle);
        }

        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class GeneratedBuildings_LoadGeneratedBuildings_Path
        {
            public static void Prefix()
            {
                if (!didStartUp_Building)
                {
                    AddBuildingStrings(FloodlightConfig.Id, FloodlightConfig.DisplayName, FloodlightConfig.Description,
                        FloodlightConfig.Effect);
                    AddBuildingStrings(LEDLightConfig.Id, LEDLightConfig.DisplayName, LEDLightConfig.Description,
                        LEDLightConfig.Effect);
                    AddBuildingStrings(TileLightConfig.Id, TileLightConfig.DisplayName, TileLightConfig.Description,
                        TileLightConfig.Effect);

                    ModUtil.AddBuildingToPlanScreen("Furniture", FloodlightConfig.Id, "lights", CeilingLightConfig.ID);
                    ModUtil.AddBuildingToPlanScreen("Furniture", LEDLightConfig.Id, "lights", FloorLampConfig.ID);
                    ModUtil.AddBuildingToPlanScreen("Furniture", TileLightConfig.Id, "lights", CeilingLightConfig.ID);
                    didStartUp_Building = true;
                }
            }
        }

        [PLibMethod(RunAt.AfterDbInit)]
        internal static void DbInitPostfix()
        {
            if (!didStartUp_Db)
            {
                AddBuildingToTech("PrettyGoodConductors", FloodlightConfig.Id);
                AddBuildingToTech("Artistry", TileLightConfig.Id);
                AddBuildingToTech("Catalytics", LEDLightConfig.Id);
                didStartUp_Db = true;
            }
        }
    }
}
