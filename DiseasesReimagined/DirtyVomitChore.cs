using Klei;
using Klei.AI;
using System;
using TUNING;
using UnityEngine;


namespace DiseasesReimagined
{
    using SicknessGameStateMachine =
        GameStateMachine<DirtyVomitChore.States, DirtyVomitChore.StatesInstance, DirtyVomitChore, object>;

    // Unlike the vanilla vomit chore, this lets you set an arbitrary illness.
    public class DirtyVomitChore : Chore<DirtyVomitChore.StatesInstance>
    {
        public DirtyVomitChore(
            ChoreType chore_type,
            IStateMachineTarget target,
            StatusItem status_item,
            Notification notification,
            SimUtil.DiseaseInfo diseaseInfo,
            Action<Chore> on_complete = null)
            : base(Db.Get().ChoreTypes.Vomit, target, target.GetComponent<ChoreProvider>(), true, on_complete,
                (System.Action<Chore>) null, (System.Action<Chore>) null, PriorityScreen.PriorityClass.compulsory, 5,
                false, true, 0, false, ReportManager.ReportType.WorkTime)
        {
            smi = new StatesInstance(this, target.gameObject, status_item, notification, diseaseInfo);
        }

        public class StatesInstance : SicknessGameStateMachine.GameInstance
        {
            public StatusItem statusItem;
            private AmountInstance bodyTemperature;
            public Notification notification;
            private SafetyQuery vomitCellQuery;
            public SimUtil.DiseaseInfo diseaseInfo;

            public StatesInstance(
                DirtyVomitChore master,
                GameObject vomiter,
                StatusItem status_item,
                Notification notification,
                SimUtil.DiseaseInfo diseaseInfo)
                : base(master)
            {
                this.diseaseInfo = diseaseInfo;
                sm.vomiter.Set(vomiter, this.smi);
                bodyTemperature = Db.Get().Amounts.Temperature.Lookup(vomiter);
                statusItem = status_item;
                this.notification = notification;
                vomitCellQuery = new SafetyQuery(Game.Instance.safetyConditions.VomitCellChecker, GetComponent<KMonoBehaviour>(), 10);
            }

            private static bool CanEmitLiquid(int cell)
            {
                return !(Grid.Solid[cell] || ((int) Grid.Properties[cell] & 2) != 0);
            }

            public void SpawnDirtyWater(float dt)
            {
                if (dt <= 0.0)
                    return;
                float totalTime = GetComponent<KBatchedAnimController>().CurrentAnim.totalTime;
                float num1 = dt / totalTime;
                Sicknesses sicknesses = this.master.GetComponent<MinionModifiers>().sicknesses;
                int index = 0;
                while (index < sicknesses.Count &&
                       sicknesses[index].modifier.sicknessType != Sickness.SicknessType.Pathogen)
                    ++index;
                Facing component = sm.vomiter.Get(smi).GetComponent<Facing>();
                int cell = Grid.PosToCell(component.transform.GetPosition());
                int num2 = component.GetFrontCell();
                if (!CanEmitLiquid(num2))
                    num2 = cell;
                Equippable equippable = GetComponent<SuitEquipper>().IsWearingAirtightSuit();
                if ((UnityEngine.Object) equippable != (UnityEngine.Object) null)
                    equippable.GetComponent<Storage>().AddLiquid(SimHashes.DirtyWater, STRESS.VOMIT_AMOUNT * num1,
                        this.bodyTemperature.value, diseaseInfo.idx, diseaseInfo.count, false, true);
                else
                    SimMessages.AddRemoveSubstance(num2, SimHashes.DirtyWater, CellEventLogger.Instance.Vomit,
                        STRESS.VOMIT_AMOUNT * num1, bodyTemperature.value, diseaseInfo.idx, diseaseInfo.count, true, -1);
            }

            public int GetVomitCell()
            {
                this.vomitCellQuery.Reset();
                Navigator component = this.GetComponent<Navigator>();
                component.RunQuery((PathFinderQuery) this.vomitCellQuery);
                int num = this.vomitCellQuery.GetResultCell();
                if (Grid.InvalidCell == num)
                    num = Grid.PosToCell((KMonoBehaviour) component);
                return num;
            }
        }

        public class States : SicknessGameStateMachine
        {
            public TargetParameter vomiter;

            public State moveto;
            public VomitState vomit;
            public State recover;
            public State recover_pst;
            public State complete;

            public override void InitializeStates(out BaseState default_state)
            {
                default_state = moveto;
                this.Target(this.vomiter);
                this.root.ToggleAnims("anim_emotes_default_kanim", 0.0f);
                this.moveto.TriggerOnEnter(GameHashes.BeginWalk, null)
                    .TriggerOnExit(GameHashes.EndWalk).ToggleAnims("anim_loco_vomiter_kanim", 0.0f).MoveTo(
                        smi => smi.GetVomitCell(),
                        vomit,
                        vomit
                        );
                this.vomit.DefaultState(this.vomit.buildup).ToggleAnims("anim_vomit_kanim", 0.0f)
                    .ToggleStatusItem(smi => smi.statusItem, null)
                    .DoNotification(smi => smi.notification)
                    .DoTutorial(Tutorial.TutorialMessages.TM_Mopping);
                this.vomit.buildup.PlayAnim("vomit_pre", KAnim.PlayMode.Once).OnAnimQueueComplete(this.vomit.release);
                this.vomit.release.ToggleEffect("Vomiting").PlayAnim("vomit_loop", KAnim.PlayMode.Once)
                    .Update("SpawnDirtyWater",
                        (smi, dt) => smi.SpawnDirtyWater(dt),
                        UpdateRate.SIM_200ms, false).OnAnimQueueComplete(vomit.release_pst);
                this.vomit.release_pst.PlayAnim("vomit_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(this.recover);
                this.recover.PlayAnim("breathe_pre")
                    .QueueAnim("breathe_loop", true, null)
                    .ScheduleGoTo(8f, (StateMachine.BaseState) recover_pst);
                this.recover_pst.QueueAnim("breathe_pst", false, null)
                    .OnAnimQueueComplete(complete);
                this.complete.ReturnSuccess();
            }

            public class VomitState : State
            {
                public State buildup;
                public State release;
                public State release_pst;
            }
        }
    }
}