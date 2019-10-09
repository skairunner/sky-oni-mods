
using System;
using System.Reflection;
using Harmony;
using JetBrains.Annotations;
using UnityEngine;

namespace CarbonRevolution
{
    public class CompatabilityPatches
    {
        public static bool GEN_STORE_OUTPUTS = false;
        public static void DoPatches(HarmonyInstance harmony)
        {
            // Check for Piped Output
            if (TryGetClass("Nightinggale.PipedOutput.ApplyExhaust") != null)
            {
                GEN_STORE_OUTPUTS = true;
            }
        }
    
        [CanBeNull]
        public static Type TryGetClass(string conf_name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = asm.GetType(conf_name);
                if (type != null) return type;
            }

            return null;
        }
    }
}