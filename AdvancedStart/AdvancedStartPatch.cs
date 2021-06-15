using Harmony;
using PeterHan.PLib;
using PeterHan.PLib.Options;
using System;
using System.Collections.Generic;
using TUNING;
using UnityEngine;
using static SkyLib.Logger;

namespace AdvancedStart
{
    public static class AdvancedStartPatch
    {
        public static void OnLoad()
        {
            StartLogging();
            PUtil.InitLibrary(false);
            POptions.RegisterOptions(typeof(AdvancedStartOptions));
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
                var config = AdvancedStartOptions.GetConfig();
                resume.AddExperience(XPForSkillPoints(config.startSkillPoints));
            }

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> method)
            {
                return PPatchTools.ReplaceMethodCall(method,
                    typeof(MinionStartingStats).GetMethodSafe("Apply", false, typeof(GameObject)),
                    typeof(NewBaseScreen_SpawnMinions_Transpiler).GetMethodSafe(nameof(DoXpGive), true,
                        typeof(MinionStartingStats), typeof(GameObject))
                );
            }
        }

        [HarmonyPatch(typeof(MinionStartingStats), MethodType.Constructor, typeof(bool), typeof(string))]
        public static class MinionStartingStats_Constructor
        {
            public static void Postfix(MinionStartingStats __instance, bool is_starter_minion)
            {
                var config = AdvancedStartOptions.GetConfig();
                if (is_starter_minion)
                {
                    // Set all stats to 10.
                    foreach (var attribute in DUPLICANTSTATS.ALL_ATTRIBUTES)
                    {
                        __instance.StartingLevels[attribute] += config.startAttributeBoost;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(NewBaseScreen), "SpawnMinions")]
        public static class NewBaseScreen_SpawnMinions
        {
            public static void Postfix(int headquartersCell)
            {
                var config = AdvancedStartOptions.GetConfig();
                var techMap = new Dictionary<string, Tech>();
                foreach (var tech in Db.Get().Techs.resources)
                {
                    techMap[tech.Id] = tech;
                }
                foreach (var tech in config.startTechs)
                {
                    Research.Instance.GetOrAdd(techMap[tech]).Purchased();
                }

                var target = Grid.CellToPosCBC(headquartersCell, Grid.SceneLayer.Move);
                foreach (var entry in config.startItems)
                {
                    new CarePackageInfo(entry.Key, entry.Value, null).Deliver(target);
                }
            }    
        }
    }
}
