using System.Collections.Generic;
using TUNING;
using UnityEngine;
using CREATURES = STRINGS.CREATURES;

namespace CarbonRevolution
{
    public class CoalPlantConfig : IEntityConfig
    {
        public const float LIFECYCLE = 3600f; // 6 cycles
        public const float CO2_PER_SECOND = 100 / LIFECYCLE;
        public const float K = 273.15f;
        public const float MIN_GROW_TEMP = K + 60;
        public const float MAX_GROW_TEMP = K + 125;
        
        public const string ID = "CoalPlant";
        public const string NAME = "Coalplant";
        public const string DESC = "Coalplants are curious crops that grow in hot, CO2-rich atmospheres and produce lumps of coal.";
        
        public const string SEED_ID = "CoalPlantSeed";
        public const string SEED_NAME = "Coal Nodule";
        public const string SEED_DESC = "Flaky and bonfire-scented, Coal Nodules grow into Coalplants when they're buried in the ground.'";

        public GameObject CreatePrefab()
        {
            var placedEntity = EntityTemplates.CreatePlacedEntity(ID, NAME, DESC, 1f, Assets.GetAnim("coalplant_kanim"),
                "idle_empty", Grid.SceneLayer.BuildingFront, 1, 2, DECOR.PENALTY.TIER1);
            EntityTemplates.ExtendEntityToBasicPlant(placedEntity, MIN_GROW_TEMP - 60,
                MIN_GROW_TEMP,
                MAX_GROW_TEMP ,
                MAX_GROW_TEMP + 80, 
                new [] { SimHashes.CarbonDioxide },
                false, 
                0f, 
                0.15f, 
                "Carbon");

            var pressureVulnerable = placedEntity.AddOrGet<PressureVulnerable>();
            pressureVulnerable.Configure((float) 0.15f, (float) 0f, 99000f, 99900f, new[]
            {
                SimHashes.CarbonDioxide
            });

            Storage storage = placedEntity.AddOrGet<Storage>();
            storage.showInUI = false;
            storage.capacityKg = 1f;

            var elementConsumer = placedEntity.AddOrGet<ElementConsumer>();
            elementConsumer.showInStatusPanel = true;
            elementConsumer.showDescriptor = true;
            elementConsumer.storeOnConsume = false;
            elementConsumer.elementToConsume = SimHashes.CarbonDioxide;
            elementConsumer.configuration = ElementConsumer.Configuration.Element;
            elementConsumer.consumptionRadius = (byte) 4;
            elementConsumer.sampleCellOffset = new Vector3(0.0f, -1f);
            elementConsumer.consumptionRate = CO2_PER_SECOND;
            
            placedEntity.AddOrGet<StandardCropPlant>();
            placedEntity.AddOrGet<IlluminationVulnerable>().SetPrefersDarkness(false);
            EntityTemplates.CreateAndRegisterPreviewForPlant(
                EntityTemplates.CreateAndRegisterSeedForPlant(placedEntity,
                        SeedProducer.ProductionType.Harvest,
                            SEED_ID,
                            SEED_NAME,
                            SEED_DESC,
                            Assets.GetAnim((HashedString) "seed_coalplant_kanim"),
                        "object", 0, new List<Tag>
                        {
                            GameTags.CropSeed
                        }, SingleEntityReceptacle.ReceptacleDirection.Top, new Tag(), 2,
                        CREATURES.SPECIES.PRICKLEFLOWER.DOMESTICATEDDESC, EntityTemplates.CollisionShape.CIRCLE, 0.25f,
                        0.25f,
                        null, string.Empty
                    ), 
                "Coalplant_preview", 
                Assets.GetAnim("coalplant_kanim"),
                "place", 1, 2);
            SoundEventVolumeCache.instance.AddVolume("bristleblossom_kanim", "Coalplant_harvest",
                NOISE_POLLUTION.CREATURES.TIER3);
            SoundEventVolumeCache.instance.AddVolume("bristleblossom_kanim", "Coalplant_grow",
                NOISE_POLLUTION.CREATURES.TIER3);
            placedEntity.AddOrGet<CoalPlant>();
            
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