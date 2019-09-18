using System;
using System.Collections.Generic;
using UnityEngine;

using static SkyLib.Utility;

namespace Drain
{
    // Essentially Pump but with a couple tiny tweaks. Had to do this because C# doesn't support monkey patches or flexible subclassing.

    public class Drain : KMonoBehaviour, ISim1000ms
    {
        public static readonly Operational.Flag PumpableFlag = new Operational.Flag("vent", Operational.Flag.Type.Requirement);
        [MyCmpReq]
        private Operational operational;
        [MyCmpGet]
        private KSelectable selectable;
        [MyCmpGet]
        private ElementConsumer consumer;
        [MyCmpGet]
        private ConduitDispenser dispenser;
        [MyCmpGet]
        private Storage storage;
        private const float OperationalUpdateInterval = 1f;
        private float elapsedTime;
        private bool pumpable;

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            this.consumer.EnableConsumption(false);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            this.elapsedTime = 0.0f;
            this.pumpable = this.UpdateOperational();
            this.dispenser.GetConduitManager().AddConduitUpdater(new System.Action<float>(this.OnConduitUpdate), ConduitFlowPriority.Last);
        }

        protected override void OnCleanUp()
        {
            this.dispenser.GetConduitManager().RemoveConduitUpdater(new System.Action<float>(this.OnConduitUpdate));
            base.OnCleanUp();
        }

        public void Sim1000ms(float dt)
        {
            this.elapsedTime += dt;
            if ((double)this.elapsedTime >= 1.0)
            {
                this.pumpable = this.UpdateOperational();
                this.elapsedTime = 0.0f;
            }
            if (this.operational.IsOperational && this.pumpable)
            {
                this.operational.SetActive(true, false);
            }
            else
            {
                this.selectable.RemoveStatusItem(Db.Get().BuildingStatusItems.PumpingLiquidOrGas, false);
                this.operational.SetActive(false, false);
            }
        }

        private bool UpdateOperational()
        {
            Element.State expected_state = Element.State.Vacuum;
            switch (this.dispenser.conduitType)
            {
                case ConduitType.Gas:
                    expected_state = Element.State.Gas;
                    break;
                case ConduitType.Liquid:
                    expected_state = Element.State.Liquid;
                    break;
            }
            bool flag = this.IsPumpable(expected_state, (int)this.consumer.consumptionRadius);
            selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NoLiquidElementToPump, false, null);
            operational.SetFlag(Pump.PumpableFlag, !storage.IsFull() && flag);
            return flag;
        }

        private bool IsPumpable(Element.State expected_state, int radius)
        {
            int cell = Grid.PosToCell(this.transform.GetPosition());
            for (int index1 = 0; index1 < (int)this.consumer.consumptionRadius; ++index1)
            {
                for (int index2 = 0; index2 < (int)this.consumer.consumptionRadius; ++index2)
                {
                    int index3 = cell + index2 + Grid.WidthInCells * index1;
                    if (Grid.Element[index3].IsState(expected_state))
                        return true;
                }
            }
            return false;
        }

        private void OnConduitUpdate(float dt)
        {
            this.selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.ConduitBlocked, (double)this.dispenser.ConduitContents.mass > 0.0, (object)null);
        }

        public ConduitType conduitType
        {
            get
            {
                return this.dispenser.conduitType;
            }
        }
    }

}