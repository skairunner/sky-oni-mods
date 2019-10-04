using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Klei.AI;

namespace DiseasesReimagined
{
    class SlimeLethalSickness : Sickness
    {
        public const string ID = "SlimeLethal";
        public SlimeLethalSickness()
            : base(ID, SicknessType.Ailment, Severity.Major, 0.00025f,
                new List<InfectionVector>()
                {
                    InfectionVector.Inhalation
                }, 6100f, null)
        {
            fatalityDuration = 6000f;
        }
    }
}
