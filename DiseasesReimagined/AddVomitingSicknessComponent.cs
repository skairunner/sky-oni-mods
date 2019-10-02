using Klei.AI;
using UnityEngine;

namespace DiseasesReimagined
{
    class AddVomitingSicknessComponent: Sickness.SicknessComponent
    {
        public string infection_source_info;
        public AddVomitingSicknessComponent(string infection_source_info)
        {
            this.infection_source_info = infection_source_info;
        }
        public override object OnInfect(GameObject go, SicknessInstance diseaseInstance)
        {
            SicknessExposureInfo exposure_info = new SicknessExposureInfo(FoodpoisonVomiting.ID, infection_source_info);
            go.GetComponent<MinionModifiers>().sicknesses.Infect(exposure_info);
            return null;
        }

        public override void OnCure(GameObject go, object instance_data)
        {
            return;
        }
    }
}
