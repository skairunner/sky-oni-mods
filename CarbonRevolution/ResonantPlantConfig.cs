using System.Collections.Generic;
using TUNING;
using UnityEngine;
using CREATURES = STRINGS.CREATURES;

namespace CarbonRevolution
{
    public class ResonantPlantConfig : IEntityConfig
    {
        public const float LIFECYCLE = 2400f; // 4 cycles
        public const float COAL_PRODUCED_TOTAL = 1000f;
        public const float COAL_PER_SEED = 250f;
        public const float CO2_PER_SECOND = COAL_PRODUCED_TOTAL / LIFECYCLE * 1.25f;
        public const float K = 273.15f;
        public const float MIN_GROW_TEMP = K + 140;
        public const float MAX_GROW_TEMP = K + 300;
        
        public const string ID = "ResonantPlant";
        public const string NAME = "Smokestalk";
        public const string DESC = "Smokestalks thrive best planted directly in flue or chimney outputs. The large amount of carbon they sequester can be recovered as coal by crushing the seeds. Smokestalks can also be eaten as light, summery salad, paired best with asbestos, because asbestos are the bestos.";
        
        public const string SEED_ID = "ResonantPlantSeed";
        public const string SEED_NAME = "Smokey Core";
        public const string SEED_DESC = "Smokey Cores grow into Smokestalks when they're planted in hot, CO2-rich environments. Experiments have concluded that they cannot be made into popcorn.";

        public GameObject CreatePrefab()
        {
            var placedEntity = EntityTemplates.CreatePlacedEntity(ID, NAME, DESC, 1f, Assets.GetAnim("resonantplant_kanim"),
                "idle_empty", Grid.SceneLayer.BuildingFront, 1, 2, DECOR.NONE);
            EntityTemplates.ExtendEntityToBasicPlant(placedEntity, K,
                MIN_GROW_TEMP,
                MAX_GROW_TEMP ,
                MAX_GROW_TEMP + 80, 
                new [] { SimHashes.CarbonDioxide },
                false, 
                0f, 
                0.15f, 
                SEED_ID);

            var pressureVulnerable = placedEntity.AddOrGet<PressureVulnerable>();
            pressureVulnerable.Configure((float) 0.15f, (float) 0f, 99000f, 99900f, new[]
            {
                SimHashes.CarbonDioxide
            });

            Storage storage = placedEntity.AddOrGet<Storage>();
            storage.showInUI = false;
            storage.capacityKg = 2f;

            var elementConsumer = placedEntity.AddOrGet<ElementConsumer>();
            elementConsumer.showInStatusPanel = true;
            elementConsumer.showDescriptor = true;
            elementConsumer.storeOnConsume = false;
            elementConsumer.configuration = ElementConsumer.Configuration.Element;
            elementConsumer.elementToConsume = SimHashes.CarbonDioxide;
            elementConsumer.consumptionRadius = 4;
            elementConsumer.sampleCellOffset = new Vector3(0.0f, 1f);
            elementConsumer.consumptionRate = CO2_PER_SECOND;
            
            placedEntity.AddOrGet<StandardCropPlant>();
            placedEntity.AddOrGet<IlluminationVulnerable>().SetPrefersDarkness(false);
            EntityTemplates.CreateAndRegisterPreviewForPlant(
                EntityTemplates.CreateAndRegisterSeedForPlant(placedEntity,
                        SeedProducer.ProductionType.Harvest,
                            SEED_ID,
                            SEED_NAME,
                            SEED_DESC,
                            Assets.GetAnim((HashedString) "seed_resonantplant_kanim"),
                        "object", 0, new List<Tag>
                        {
                            GameTags.CropSeed
                        }, SingleEntityReceptacle.ReceptacleDirection.Top, new Tag(), 2,
                        CREATURES.SPECIES.PRICKLEFLOWER.DOMESTICATEDDESC, EntityTemplates.CollisionShape.CIRCLE, 0.25f,
                        0.25f,
                        null, string.Empty
                    ), 
                "Resonantplant_preview", 
                Assets.GetAnim("resonantplant_kanim"),
                "place", 1, 2);
            SoundEventVolumeCache.instance.AddVolume("bristleblossom_kanim", "resonantplant_harvest",
                NOISE_POLLUTION.CREATURES.TIER3);
            SoundEventVolumeCache.instance.AddVolume("bristleblossom_kanim", "resonantplant_grow",
                NOISE_POLLUTION.CREATURES.TIER3);
            placedEntity.AddOrGet<CoalPlant>();
            placedEntity.AddOrGet<PingSometimes>();
            
            return placedEntity;
        }

        public void OnPrefabInit(GameObject inst)
        {
        }

        public void OnSpawn(GameObject inst)
        {    
            inst.GetComponent<ElementConsumer>().EnableConsumption(true);
        }
    }
}
