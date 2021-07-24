using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace DiseasesReimagined
{
    // This sickness component modifies the amount of time left until the disease is cured, on the "parent disease"
    // of the disease that has this component. Whew, what a mouthful.
    class ModifyParentTimeComponent : Sickness.SicknessComponent
    {
        public readonly string parentName;
        public readonly float percentReduced;

        public ModifyParentTimeComponent(string parentName, float percentReduced)
        {
            this.parentName = parentName;
            this.percentReduced = percentReduced;
        }

        public override object OnInfect(GameObject go, SicknessInstance diseaseInstance)
        {
            // Doesn't do anything on infect
            return null;
        }

        public override void OnCure(GameObject go, object instance_data)
        {
            // Modifies parent time on cure
            var parent = Db.Get().Sicknesses.Get(parentName);
            var sicknesses = go.GetComponent<Modifiers>().sicknesses;
            if (parent == null || !sicknesses.Has(parent)) return;

            var sickness = sicknesses.Get(parent);
            var smi = Traverse.Create(sickness).Field("smi").GetValue<SicknessInstance.StatesInstance>();
            var fraction_left = 1 - smi.sm.percentRecovered.Get(smi);
            smi.sm.percentRecovered.Delta(fraction_left * percentReduced, smi);
        }
    }
}