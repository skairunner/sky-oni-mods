using Harmony;
using System.Collections.Generic;
using UnityEngine;
using static SkyLib.Logger;
using static SkyLib.OniUtils;

namespace ExpandedLights
{
    public static class LightSystemPatch
    {
        public static string ModName = "LightSystemPatch";
        public static bool didLightingHack = false;
        public static Traverse ScanOctant;

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging(ModName);
                ScanOctant = Traverse.Create(typeof(DiscreteShadowCaster))
                    .Method("ScanOctant", new[] {
                        typeof(Vector2I),
                        typeof(int),
                        typeof(int),
                        typeof(DiscreteShadowCaster.Octant),
                        typeof(double),
                        typeof(double),
                        typeof(List<int>)
                    });
            }
        }

        public static class ExtendedLightShapes
        {
            public const int ConeLeft = 991;
            public const int ConeUp = 992;
            public const int ConeRight = 993;
            public const int Linear = 994;
        }

         [HarmonyPatch(typeof(DiscreteShadowCaster), "GetVisibleCells")]
        public static class DiscreteShadowCaster_GetVisibleCells_Patch
        {
            public static bool Prefix(int cell, List<int> visiblePoints, int range, LightShape shape)
            {
                if ((int)shape < 2)
                {
                    return true;
                }
                // do the rest of the method
                visiblePoints.Add(cell);
                Vector2I xy = Grid.CellToXY(cell);

                switch ((int)shape)
                {
                    case ExtendedLightShapes.ConeRight:
                        ScanOctant.GetValue(xy, range, 1, DiscreteShadowCaster.Octant.E_NE, 1.0, 0.0, visiblePoints);
                        ScanOctant.GetValue(xy, range, 1, DiscreteShadowCaster.Octant.E_SE, 1.0, 0.0, visiblePoints);
                        break;
                    case ExtendedLightShapes.ConeLeft:
                        ScanOctant.GetValue(xy, range, 1, DiscreteShadowCaster.Octant.W_NW, 1.0, 0.0, visiblePoints);
                        ScanOctant.GetValue(xy, range, 1, DiscreteShadowCaster.Octant.W_SW, 1.0, 0.0, visiblePoints);
                        break;
                    case ExtendedLightShapes.ConeUp:
                        ScanOctant.GetValue(xy, range, 1, DiscreteShadowCaster.Octant.N_NE, 1.0, 0.0, visiblePoints);
                        ScanOctant.GetValue(xy, range, 1, DiscreteShadowCaster.Octant.N_NW, 1.0, 0.0, visiblePoints);
                        break;
                    case ExtendedLightShapes.Linear:
                        break;
                    default:
                        return true; // just give up :D
                }
                return false;
            }
        }


        [HarmonyPatch(typeof(LightGridManager), "CalculateFalloff")]
        public static class LightGridManager_CalculateFalloff_Patch
        {
            public static int Postfix(int result, float falloffRate, int cell, int origin)
            {
                return Mathf.Max(1, Mathf.RoundToInt(.3f * Mathf.Max(Grid.GetCellDistance(origin, cell), 1)));
            }
        }
    }
}
