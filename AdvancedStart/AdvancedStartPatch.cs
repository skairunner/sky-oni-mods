using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Harmony;
using static SkyLib.Logger;
using PeterHan.PLib;
using TUNING;
using UnityEngine;

namespace AdvancedStart
{
    public static class AdvancedStartPatch
    {
        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();
                PUtil.InitLibrary(false);
            }
        }

        [HarmonyPatch(typeof(NewBaseScreen), "SpawnMinions")]
        public static class NewBaseScreen_SpawnMinions_Transpiler
        {
            
            // reverse-engineered from MinionResume.CalculateTotalSkillPointsGained()
            public static float XPForSkillPoints(int skillPoints)
            {
                return (float)Math.Pow(Math.E, 
                    SKILLS.EXPERIENCE_LEVEL_POWER * Math.Log((float)skillPoints / SKILLS.TARGET_SKILLS_EARNED) + Math.Log(SKILLS.TARGET_SKILLS_CYCLE * 600f));
            }

            private static void DoXpGive(MinionStartingStats __instance, GameObject go)
            {
                __instance.Apply(go);
                var resume = go.GetComponent<MinionResume>();
                resume.AddExperience(XPForSkillPoints(7));
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> method)
            {
                return PPatchTools.ReplaceMethodCall(method,
                    typeof(MinionStartingStats).GetMethodSafe("Apply", false, typeof(GameObject)),
                    typeof(NewBaseScreen_SpawnMinions_Transpiler).GetMethodSafe("DoXpGive", true,
                        typeof(MinionStartingStats), typeof(GameObject))
                );
            }
        }

        [HarmonyPatch(typeof(MinionStartingStats), MethodType.Constructor, typeof(bool), typeof(string))]
        public static class MinionStartingStats_Constructor
        {
            public static void Postfix(MinionStartingStats __instance, bool is_starter_minion)
            {
                if (is_starter_minion)
                {
                    // Set all stats to 10.
                    foreach (var attribute in DUPLICANTSTATS.ALL_ATTRIBUTES)
                    {
                        __instance.StartingLevels[attribute] += 10;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(NewBaseScreen), "SpawnMinions")]
        public static class NewBaseScreen_SpawnMinions
        {
            public static void Postfix(int headquartersCell)
            {
                new CarePackageInfo("steel", 1001f, null).Deliver(Grid.CellToPosCBC(headquartersCell, Grid.SceneLayer.Move));
                foreach (var tech in Db.Get().Techs.resources)
                {
                    Research.Instance.GetOrAdd(tech).Purchased();
                    break;
                }
            }    
        }
    }
}