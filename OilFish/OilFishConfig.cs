using System.Collections.Generic;
using STRINGS;
using UnityEngine;
using BUILDINGS = TUNING.BUILDINGS;
using CREATURES = TUNING.CREATURES;

namespace OilFish
{
    public class OilFishConfig : IEntityConfig
    {
        public const string ID = "PacuOil";
        public const string DESC = "";
        public const string EGGDESC = "";
        public const string BASE_TRAIT_ID = "PacuOilBaseTrait";
        public const string EGG_ID = "PacuOilEgg";

        public const float CRUDE_OIL_CONVERTED_PER_CYCLE = 120f;
        public const SimHashes INPUT_ELEMENT = SimHashes.CrudeOil;
        public const SimHashes OUTPUT_ELEMENT = SimHashes.Petroleum;
        public const int EGG_SORT_ORDER = 501;
        public static readonly EffectorValues DECOR = BUILDINGS.DECOR.NONE;
        public static string NAME = "Deep Pacu";
        public static string EGGNAME = UI.FormatAsLink("Slick Fry Egg", EGG_ID);

        public static List<FertilityMonitor.BreedingChance> EGG_CHANCES_OIL = new List<FertilityMonitor.BreedingChance>
        {
            new FertilityMonitor.BreedingChance
            {
                egg = "PacuEgg".ToTag(),
                weight = 0.2f
            },
            new FertilityMonitor.BreedingChance
            {
                egg = "PacuCleanerEgg".ToTag(),
                weight = 0.32f
            },
            new FertilityMonitor.BreedingChance
            {
                egg = EGG_ID.ToTag(),
                weight = 0.65f
            }
        };

        public GameObject CreatePrefab()
        {
            return EntityTemplates.ExtendEntityToFertileCreature(
                EntityTemplates.ExtendEntityToWildCreature(
                    CreatePacu(ID,
                        NAME,
                        DESC,
                        "pacu_kanim", false),
                    PacuTuning.PEN_SIZE_PER_CREATURE, 25f),
                EGG_ID,
                EGGNAME,
                EGGDESC,
                "egg_pacu_kanim",
                PacuTuning.EGG_MASS,
                BabyOilFishConfig.ID,
                15f, 5f, EGG_CHANCES_OIL, 502, false, true, false, 0.75f
            );
        }

        public void OnPrefabInit(GameObject prefab)
        {
        }

        public void OnSpawn(GameObject inst)
        {
            var component = inst.GetComponent<ElementConsumer>();
            if (!(component != null))
                return;
            component.EnableConsumption(true);
        }

        public static GameObject CreatePacu(
            string id,
            string name,
            string desc,
            string anim_file,
            bool is_baby)
        {
            var prefab = BasePacuConfig.CreatePrefab(id, "PacuOilBaseTrait", name, desc, anim_file, is_baby, "glp_",
                243.15f, 278.15f);

            var def4 = prefab.AddOrGetDef<CreatureCalorieMonitor.Def>();
            def4.diet = new Diet(new Diet.Info(new HashSet<Tag>
                {
                    SimHashes.Carbon.CreateTag()
                }, SimHashes.Sulfur.CreateTag(),
                PacuTuning.STANDARD_CALORIES_PER_CYCLE / 140f
                , CREATURES.CONVERSION_EFFICIENCY.NORMAL));

            var wildCreature =
                EntityTemplates.ExtendEntityToWildCreature(prefab, PacuTuning.PEN_SIZE_PER_CREATURE, 25f);
            if (!is_baby)
            {
                wildCreature.AddComponent<Storage>().capacityKg = 10f;
                var elementConsumer = (ElementConsumer) wildCreature.AddOrGet<PassiveElementConsumer>();
                elementConsumer.elementToConsume = INPUT_ELEMENT;
                elementConsumer.consumptionRate = 0.2f;
                elementConsumer.capacityKG = 10f;
                elementConsumer.consumptionRadius = 3;
                elementConsumer.showInStatusPanel = true;
                elementConsumer.sampleCellOffset = new Vector3(0.0f, 0.0f, 0.0f);
                elementConsumer.isRequired = false;
                elementConsumer.storeOnConsume = true;
                elementConsumer.showDescriptor = false;
                wildCreature.AddOrGet<UpdateElementConsumerPosition>();
                var bubbleSpawner = wildCreature.AddComponent<BubbleSpawner>();
                bubbleSpawner.element = OUTPUT_ELEMENT;
                bubbleSpawner.emitMass = 2f;
                bubbleSpawner.emitVariance = 0.5f;
                bubbleSpawner.initialVelocity = new Vector2f(0, 1);
                var elementConverter = wildCreature.AddOrGet<ElementConverter>();
                elementConverter.consumedElements = new ElementConverter.ConsumedElement[1]
                {
                    new ElementConverter.ConsumedElement(INPUT_ELEMENT.CreateTag(), 0.2f)
                };
                elementConverter.outputElements = new ElementConverter.OutputElement[1]
                {
                    new ElementConverter.OutputElement(0.2f, OUTPUT_ELEMENT, 0.0f, true, true)
                };
            }

            return wildCreature;
        }
    }
}