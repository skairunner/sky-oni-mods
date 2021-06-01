using System;
using Klei;
using Klei.AI;
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
                null, null, PriorityScreen.PriorityClass.compulsory)
        {
            smi = new StatesInstance(this, target.gameObject, status_item, notification, diseaseInfo);
        }

        public class StatesInstance : SicknessGameStateMachine.GameInstance
        {
            readonly AmountInstance bodyTemperature;
            public readonly Notification notification;
            public readonly StatusItem statusItem;
            readonly SafetyQuery vomitCellQuery;
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
                sm.vomiter.Set(vomiter, smi);
                bodyTemperature = Db.Get().Amounts.Temperature.Lookup(vomiter);
                statusItem = status_item;
                this.notification = notification;
                vomitCellQuery = new SafetyQuery(Game.Instance.safetyConditions.VomitCellChecker,
                    GetComponent<KMonoBehaviour>(), 10);
            }

            static bool CanEmitLiquid(int cell)
            {
                return !(Grid.Solid[cell] || (Grid.Properties[cell] & 2) != 0);
            }

            public void SpawnDirtyWater(float dt)
            {
                if (dt <= 0.0)
                    return;
                var timeRatio = dt / GetComponent<KBatchedAnimController>().CurrentAnim.totalTime;

                var orientation = sm.vomiter.Get(smi).GetComponent<Facing>();
                var posCell = Grid.PosToCell(orientation.transform.GetPosition());
                int frontCell = orientation.GetFrontCell();
                if (!CanEmitLiquid(frontCell))
                    frontCell = posCell;

                var suit = GetComponent<SuitEquipper>().IsWearingAirtightSuit();
                if (suit != null)
                    suit.GetComponent<Storage>().AddLiquid(SimHashes.DirtyWater, STRESS.VOMIT_AMOUNT * timeRatio,
                        bodyTemperature.value, diseaseInfo.idx, diseaseInfo.count);
                else
                    SimMessages.AddRemoveSubstance(frontCell, SimHashes.DirtyWater, CellEventLogger.Instance.Vomit,
                        STRESS.VOMIT_AMOUNT * timeRatio, bodyTemperature.value, diseaseInfo.idx, diseaseInfo.count);
            }

            public int GetVomitCell()
            {
                var nav = GetComponent<Navigator>();
                vomitCellQuery.Reset();
                nav.RunQuery(vomitCellQuery);

                var pos = vomitCellQuery.GetResultCell();
                return pos == Grid.InvalidCell ? Grid.PosToCell(nav) : pos;
            }
        }

        public class States : SicknessGameStateMachine
        {
            public State complete;

            public State moveto;
            public State recover;
            public State recover_pst;
            public VomitState vomit;
            public TargetParameter vomiter;

            public override void InitializeStates(out BaseState default_state)
            {
                default_state = moveto;
                Target(vomiter);
                root.ToggleAnims("anim_emotes_default_kanim");
                moveto.TriggerOnEnter(GameHashes.BeginWalk)
                      .TriggerOnExit(GameHashes.EndWalk).ToggleAnims("anim_loco_vomiter_kanim").MoveTo(
                           smi => smi.GetVomitCell(),
                           vomit,
                           vomit
                       );
                vomit.DefaultState(vomit.buildup).ToggleAnims("anim_vomit_kanim")
                     .ToggleStatusItem(smi => smi.statusItem)
                     .DoNotification(smi => smi.notification)
                     .DoTutorial(Tutorial.TutorialMessages.TM_Mopping);
                vomit.buildup.PlayAnim("vomit_pre", KAnim.PlayMode.Once).OnAnimQueueComplete(vomit.release);
                vomit.release.ToggleEffect("Vomiting").PlayAnim("vomit_loop", KAnim.PlayMode.Once)
                     .Update("SpawnDirtyWater",
                          (smi, dt) => smi.SpawnDirtyWater(dt)).OnAnimQueueComplete(vomit.release_pst);
                vomit.release_pst.PlayAnim("vomit_pst", KAnim.PlayMode.Once).OnAnimQueueComplete(recover);
                recover.PlayAnim("breathe_pre")
                       .QueueAnim("breathe_loop", true)
                       .ScheduleGoTo(8f, recover_pst);
                recover_pst.QueueAnim("breathe_pst")
                           .OnAnimQueueComplete(complete);
                complete.ReturnSuccess();
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
