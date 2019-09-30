using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using JetBrains.Annotations;
using UnityEngine;
using static SkyLib.Logger;

namespace RadiateHeat
{
    public class Patch : Attribute
    {
        public string ConfName { get; set; }

        public Patch(string confName)
        {
            ConfName = confName;
        }
    }

    public class VeryLatePatches
    {
        public static void DoVeryLatePatches(HarmonyInstance harmony)
        {
            LogLine("Starting very late patches.");
            foreach (var methodInfo in typeof(VeryLatePatches).GetMethods())
            {
                var attrs = methodInfo.GetCustomAttributes(typeof(Patch), false);
                if (attrs.Length > 0)
                {
                    var confName = ((Patch) attrs[0]).ConfName;
                    PatchBuilding(harmony, confName, methodInfo);
                }
            }
            LogLine("Finished very late patches.");
        }

        [CanBeNull]
        public static Type MaybeGetBuilding(string conf_name)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(conf_name);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }
        public static void PatchBuilding(HarmonyInstance harmony, string conf_name, MethodInfo postfix)
        {
            var type = MaybeGetBuilding(conf_name);
            if (type != null)
            {
                harmony.Patch(type.GetMethod("DoPostConfigureComplete"), postfix: new HarmonyMethod(postfix));
            }
            else
            {
                LogLine($"Couldn't find '{conf_name}' to patch, skipping.");
            }
        }

        [Patch("ExpandedLights.FloodlightConfig")]
        public static void Patch_Floodlights(GameObject go)
        {
            RadiatePatch.Mod_OnLoad.AttachHeatComponent(go, 0.9f, 1f);
        }

        [Patch("ExpandedLights.TileLightConfig")]
        public static void Patch_TileLight(GameObject go)
        {
            RadiatePatch.Mod_OnLoad.AttachHeatComponent(go, 0.9f, 0.2f);
        }

        [Patch("WaterproofTransformer.WaterproofTransformerConfig")]
        public static void Patch_WaterproofTransformer(GameObject go)
        {
            RadiatePatch.Mod_OnLoad.AttachHeatComponent(go, 0.6f, 0.4f);
        }

        [Patch("WaterproofTransformer.WaterproofBatteryConfig")]
        public static void Patch_WaterproofBattery(GameObject go)
        {
            RadiatePatch.Mod_OnLoad.AttachHeatComponent(go, 0.6f, 0.4f);
        }
    }
}
