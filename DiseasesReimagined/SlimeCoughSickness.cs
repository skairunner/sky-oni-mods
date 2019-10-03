﻿using System.Collections.Generic;
using Klei.AI;

namespace DiseasesReimagined
{
    class SlimeCoughSickness : Sickness
    {
        public const string ID = "SlimeCough";

        public SlimeCoughSickness()
            : base(ID, SicknessType.Ailment, Severity.Minor, 0.00025f,
                new List<InfectionVector>()
                {
                    InfectionVector.Inhalation
                }, 6000f, null)
        {
            AddSicknessComponent(new SlimeSickness.SlimeLungComponent());
        }
    }
}
