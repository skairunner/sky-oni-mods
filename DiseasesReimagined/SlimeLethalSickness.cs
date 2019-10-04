using System.Collections.Generic;
using Klei.AI;

namespace DiseasesReimagined
{
    class SlimeLethalSickness : Sickness
    {
        public const string ID = "SlimeLethal";

        public SlimeLethalSickness()
            : base(ID, SicknessType.Ailment, Severity.Major, 0.00025f,
                new List<InfectionVector>
                {
                    InfectionVector.Inhalation
                }, 6100f)
        {
            fatalityDuration = 6000f;
            AddSicknessComponent(new ModifyParentTimeComponent(SlimeSickness.ID, -1));
        }
    }
}