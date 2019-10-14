using System;
using System.Collections.Generic;
using Klei;
using Klei.AI;
using STRINGS;
using UnityEngine;

namespace DiseasesReimagined
{
    public class FoodPoisonVomiting : Sickness
    {
        public const string ID = "FoodPoisonVomiting";
        public const string RECOVERY_ID = null; // don't need an immunity effect for a symptom
        public static List<InfectionVector> vectors = new List<InfectionVector>(new[] {InfectionVector.Digestion});

        public FoodPoisonVomiting()
            : base(ID, SicknessType.Ailment, Severity.Minor, 1f, vectors, 1020f)
        {
            AddSicknessComponent(new VomitComponent());
            AddSicknessComponent(new ModifyParentTimeComponent(FoodSickness.ID, .8f));
            AddSicknessComponent(new AttributeModifierSickness(new []
            {
                new AttributeModifier(Db.Get().Amounts.Stamina.deltaAttribute.Id, -0.08333333333f, "Vomiting")
            }));
        }
    }

    public class VomitComponent : Sickness.SicknessComponent
    {
        public override object OnInfect(GameObject go, SicknessInstance diseaseInstance)
        {
            var statesInstance = new StatesInstance(diseaseInstance);
            statesInstance.StartSM();
            return statesInstance;
        }

        public override void OnCure(GameObject go, object instance_data)
        {
            ((StateMachine.Instance) instance_data).StopSM("Cured");
        }

        public override List<Descriptor> GetSymptoms()
        {
            return new List<Descriptor>
            {
                new Descriptor("Vomiting", DUPLICANTS.DISEASES.FOODSICKNESS.
                    VOMIT_SYMPTOM_TOOLTIP, Descriptor.DescriptorType.SymptomAidable)
            };
        }

        public class StatesInstance : GameStateMachine<States, StatesInstance, SicknessInstance, object>.GameInstance
        {
            public float lastVomitTime;

            // The number of germs vomited.
            public const int GermCount = 10000;

            public StatesInstance(SicknessInstance master)
                : base(master)
            {
            }

            public void Vomit(GameObject vomiter)
            {
                var diseaseList = Db.Get().Diseases;
                var chore_provider = vomiter.GetComponent<ChoreProvider>();
                var sickness = Db.Get().Sicknesses.FoodSickness;
                if (chore_provider != null)
                {
                    var notification = new Notification("Vomiting",
                        NotificationType.Bad, HashedString.Invalid,
                        (notificationList, data) => "These Duplicants are vomiting from " +
                        sickness.Name + ":" + notificationList.ReduceMessages(false));
                    var diseaseInfo = new SimUtil.DiseaseInfo
                    {
                        idx = diseaseList.GetIndex(diseaseList.FoodGerms.id), count = 0
                    };
                    new DirtyVomitChore(Db.Get().ChoreTypes.Vomit, chore_provider,
                        Db.Get().DuplicantStatusItems.Vomiting, notification, diseaseInfo,
                        chore => { FinishedVomit(vomiter); });
                }
                // Decrease kcal as well
                var cals = Db.Get().Amounts.Calories.Lookup(vomiter);
                if (cals.value > 600)
                    cals.SetValue(cals.value - 500);
            }

            void FinishedVomit(GameObject vomiter)
            {
                sm.vomitFinished.Trigger(this);
            }
        }

        public class States : GameStateMachine<States, StatesInstance, SicknessInstance>
        {
            public VomitStates Vomit;
            public Signal vomitFinished;

            public override void InitializeStates(out BaseState default_state)
            {
                default_state = Vomit;
                Vomit.DefaultState(Vomit.normal); //.TagTransition(GameTags.NoOxygen, notbreathing);
                Vomit.normal.Enter("SetVomitTime", smi =>
                {
                    if (smi.lastVomitTime >= (double) Time.time)
                        return;
                    smi.lastVomitTime = Time.time;
                }).Update("Vomit", (smi, dt) =>
                {
                    if (Time.time - (double) smi.lastVomitTime <= 120.0)
                        return;
                    smi.GoTo(Vomit.vomit);
                }, UpdateRate.SIM_4000ms);
                Vomit.vomit.Enter("DoTheVomit", smi => { smi.Vomit(smi.GetMaster().gameObject); })
                     .OnSignal(vomitFinished, Vomit.normal);
            }

            public class VomitStates : State
            {
                public State normal;
                public State vomit;
            }
        }
    }
}
