using Klei.AI;
using UnityEngine;

namespace DiseasesReimagined
{
    class AddSicknessComponent: Sickness.SicknessComponent
    {
        public string infection_source_info;
        public string sickness_id;

        public AddSicknessComponent(string sickness_id, string infection_source_info)
        {
            this.sickness_id = sickness_id;
            this.infection_source_info = infection_source_info;
        }
        public override object OnInfect(GameObject go, SicknessInstance diseaseInstance)
        {
            SicknessExposureInfo exposure_info = new SicknessExposureInfo(sickness_id, infection_source_info);
            go.GetComponent<MinionModifiers>().sicknesses.Infect(exposure_info);
            return null;
        }

        public override void OnCure(GameObject go, object instance_data)
        {
            return;
        }
    }
}
