using Klei.AI;
using PeterHan.PLib.Detours;
using UnityEngine;

namespace DiseasesReimagined
{
    // This sickness component modifies the amount of time left until the disease is cured, on the "parent disease"
    // of the disease that has this component. Whew, what a mouthful.
    internal sealed class ModifyParentTimeComponent : Sickness.SicknessComponent
    {
        private static readonly IDetouredField<SicknessInstance, SicknessInstance.
            StatesInstance> SMI_FIELD = PDetours.DetourField<SicknessInstance,
            SicknessInstance.StatesInstance>("smi");

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
            if (parent != null && sicknesses.Has(parent))
            {
                var sickness = sicknesses.Get(parent);
                var smi = SMI_FIELD.Get(sickness);
                if (smi != null) {
                    var fraction_left = 1 - smi.sm.percentRecovered.Get(smi);
                    smi.sm.percentRecovered.Delta(fraction_left * percentReduced, smi);
                }
            }
        }
    }
}
