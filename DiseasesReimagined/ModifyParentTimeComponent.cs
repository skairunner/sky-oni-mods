using Harmony;
using Klei.AI;
using UnityEngine;

namespace DiseasesReimagined
{
    // This sickness component modifies the amount of time left until the disease is cured, on the "parent disease"
    // of the disease that has this component. Whew, what a mouthful.
    class ModifyParentTimeComponent : Sickness.SicknessComponent
    {
        public string parentname;
        public float percent_reduced;

        public ModifyParentTimeComponent(string parentname, float percent_reduced)
        {
            this.parentname = parentname;
            this.percent_reduced = percent_reduced;
        }

        public override object OnInfect(GameObject go, SicknessInstance diseaseInstance)
        {
            // Doesn't do anything on infect
            return null;
        }

        public override void OnCure(GameObject go, object instance_data)
        {
            // Modifies parent time on cure
            var sicknesses = go.GetComponent<Modifiers>().sicknesses;
            var parent = Db.Get().Sicknesses.Get(parentname);
            if (parent != null && sicknesses.Has(parent))
            {
                var sickness = sicknesses.Get(parent);
                var smi = Traverse.Create(sickness).Field("smi").GetValue<SicknessInstance.StatesInstance>();
                var fraction_left = 1 - smi.sm.percentRecovered.Get(smi);
                smi.sm.percentRecovered.Delta(fraction_left * percent_reduced, smi);
            }
            return;
        }
    }
}
