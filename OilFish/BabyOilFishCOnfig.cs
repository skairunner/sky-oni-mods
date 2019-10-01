using UnityEngine;

namespace OilFish
{

    public class BabyOilFishConfig : IEntityConfig
    {
        public const string ID = "PacuOilBaby";
        public const string NAME = "Slick Fry";
        public const string DESC = "";
        public GameObject CreatePrefab()
        {
            GameObject pacu = OilFishConfig.CreatePacu(ID, NAME, DESC, "baby_pacu_kanim", true);
            EntityTemplates.ExtendEntityToBeingABaby(pacu, OilFishConfig.ID, (string)null);
            return pacu;
        }

        public void OnPrefabInit(GameObject prefab)
        {
        }

        public void OnSpawn(GameObject inst)
        {
        }
    }

}
