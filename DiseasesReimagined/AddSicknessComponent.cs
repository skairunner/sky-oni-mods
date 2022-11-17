using Klei.AI;
using UnityEngine;

namespace DiseasesReimagined
{
    internal class AddSicknessComponent : Sickness.SicknessComponent
    {
        private readonly string excluded_effect;
        private readonly string infection_source_info;
        private readonly string sickness_id;

        public AddSicknessComponent(string sickness_id, string infection_source_info,
            string excluded_effect = "")
        {
            this.excluded_effect = excluded_effect;
            this.sickness_id = sickness_id;
            this.infection_source_info = infection_source_info;
        }

        public override object OnInfect(GameObject go, SicknessInstance diseaseInstance)
        {
            if (go != null && diseaseInstance.GetPercentCured() < 0.001f)
                GameScheduler.Instance.Schedule("InfectWith" + sickness_id, 0.5f, (_) =>
                {
                    // Do not inflict the symptoms if the excluded effect is present
                    if (!go.TryGetComponent(out Effects effects) || string.IsNullOrEmpty(
                        excluded_effect) || !effects.HasEffect(excluded_effect))
                    {
                        var exposure_info = new SicknessExposureInfo(sickness_id,
                            infection_source_info);
                        if (go.TryGetComponent(out MinionModifiers modifiers))
                            modifiers.sicknesses?.Infect(exposure_info);
                    }
                });
            return null;
        }

        public override void OnCure(GameObject go, object instance_data)
        {
            if (go.TryGetComponent(out MinionModifiers modifiers))
            // Cure the added sickness if the original disease gets cured
                modifiers.sicknesses?.Cure(sickness_id);
        }
    }
}
