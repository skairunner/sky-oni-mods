using System.Collections.Generic;
using Harmony;
using TUNING;
using static SkyLib.Logger;

namespace OilFish
{
    public class OilFishPatch
    {
        public static bool didStartUp_Building = false;

        private static System.Action CreatePressureModifier(string id, Tag eggTag, float minpressure, float modifier)
        {
            var name = "Environment pressure";
            var desc = "Over {0}/tile";
            return () => Db.Get().CreateFertilityModifier(
                id,
                eggTag,
                name,
                null,
                src => string.Format(desc,
                    GameUtil.GetFormattedMass(minpressure)),
                (inst, eggType) =>
                {
                    var component = inst.master.gameObject.AddOrGet<CheckPressure>();
                    if (component != null)
                    {
                        component.pressure = minpressure;
                        component.OnPressure = dt => inst.AddBreedingChance(eggTag, dt * modifier);
                    }
                });
        }

        public static class Mod_OnLoad
        {
            public static void OnLoad()
            {
                StartLogging();

                CREATURES.EGG_CHANCE_MODIFIERS.MODIFIER_CREATORS.Add(CreatePressureModifier(
                    OilFishConfig.ID,
                    OilFishConfig.EGG_ID.ToTag(),
                    2f,
                    1f
                ));
            }
        }

        [HarmonyPatch(typeof(EntityTemplates), "ExtendEntityToFertileCreature")]
        public class EntityTemplates_ExtendEntityToFertileCreature_Patch
        {
            private static void Prefix(string eggId, List<FertilityMonitor.BreedingChance> egg_chances)
            {
                if (eggId.Equals("PacuCleanerEgg"))
                    egg_chances.Add(new FertilityMonitor.BreedingChance
                    {
                        egg = OilFishConfig.EGG_ID.ToTag(),
                        weight = 0.02f
                    });
            }
        }
    }
}