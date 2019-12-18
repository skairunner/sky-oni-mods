namespace Drains
{
    // Essentially Pump but with a couple tiny tweaks. Had to do this because C# doesn't support monkey patches or flexible subclassing.

    public class Drain : KMonoBehaviour, ISim1000ms
    {
        private const float OperationalUpdateInterval = 1f;

        public static readonly Operational.Flag PumpableFlag =
            new Operational.Flag("vent", Operational.Flag.Type.Requirement);

        [MyCmpGet] private ElementConsumer consumer;
        [MyCmpGet] private ConduitDispenser dispenser;
        private float elapsedTime;

        [MyCmpReq] private Operational operational;
        private bool pumpable;
        [MyCmpGet] private KSelectable selectable;
        [MyCmpGet] private Storage storage;

        public ConduitType conduitType => dispenser.conduitType;

        public void Sim1000ms(float dt)
        {
            elapsedTime += dt;
            if (elapsedTime >= 1.0)
            {
                pumpable = UpdateOperational();
                elapsedTime = 0.0f;
            }

            if (operational.IsOperational && pumpable)
            {
                operational.SetActive(true);
            }
            else
            {
                selectable.RemoveStatusItem(Db.Get().BuildingStatusItems.PumpingLiquidOrGas);
                operational.SetActive(false);
            }
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            consumer.EnableConsumption(false);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            elapsedTime = 0.0f;
            pumpable = UpdateOperational();
            dispenser.GetConduitManager().AddConduitUpdater(OnConduitUpdate,
                ConduitFlowPriority.LastPostUpdate);
        }

        protected override void OnCleanUp()
        {
            dispenser.GetConduitManager().RemoveConduitUpdater(OnConduitUpdate);
            base.OnCleanUp();
        }

        private bool UpdateOperational()
        {
            var expected_state = Element.State.Vacuum;
            switch (dispenser.conduitType)
            {
                case ConduitType.Gas:
                    expected_state = Element.State.Gas;
                    break;
                case ConduitType.Liquid:
                    expected_state = Element.State.Liquid;
                    break;
            }

            var flag = IsPumpable(expected_state, consumer.consumptionRadius);
            selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NoLiquidElementToPump, false);
            operational.SetFlag(Pump.PumpableFlag, !storage.IsFull() && flag);
            return flag;
        }

        private bool IsPumpable(Element.State expected_state, int radius)
        {
            var cell = Grid.PosToCell(transform.GetPosition());
            for (var index1 = 0; index1 < (int) consumer.consumptionRadius; ++index1)
            for (var index2 = 0; index2 < (int) consumer.consumptionRadius; ++index2)
            {
                var index3 = cell + index2 + Grid.WidthInCells * index1;
                if (Grid.Element[index3].IsState(expected_state))
                    return true;
            }

            return false;
        }

        private void OnConduitUpdate(float dt)
        {
            selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.ConduitBlocked,
                dispenser.ConduitContents.mass > 0.0);
        }
    }
}
