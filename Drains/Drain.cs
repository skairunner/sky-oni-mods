using Harmony;
using PeterHan.PLib;

namespace Drains
{
    public class Drain : KMonoBehaviour, ISim1000ms
    {
        private const float OperationalUpdateInterval = 1f;

        private static readonly Operational.Flag DrainableFlag =
            new Operational.Flag("Drainable", Operational.Flag.Type.Requirement);

        private HandleVector<int>.Handle accumulatorHandle = HandleVector<int>.InvalidHandle;
        private float elapsedTime;

        public void Sim1000ms(float dt)
        {
            elapsedTime += dt;
            if (elapsedTime >= OperationalUpdateInterval)
            {
                UpdateOperational();
                elapsedTime = 0.0f;
            }

            if (operational.IsOperational)
            {
                operational.SetActive(true);
                anim.Play("built", KAnim.PlayMode.Paused);
            }
            else
            {
                anim.Play("clogged", KAnim.PlayMode.Paused);
            }

            selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.PumpingLiquidOrGas, operational.IsOperational,
                accumulatorHandle);
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            consumer.EnableConsumption(false);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            UpdateOperational();
            dispenser.GetConduitManager().AddConduitUpdater(OnConduitUpdate, ConduitFlowPriority.Last);
            accumulatorHandle = Traverse.Create(consumer).GetField<HandleVector<int>.Handle>("accumulator");
        }

        protected override void OnCleanUp()
        {
            dispenser.GetConduitManager().RemoveConduitUpdater(OnConduitUpdate);
            base.OnCleanUp();
        }

        private void UpdateOperational()
        {
            var pos = Grid.PosToCell(transform.GetPosition());
            var flag = !storage.IsFull() && Grid.Element[pos].IsState(Element.State.Liquid);
            selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.NoLiquidElementToPump, !flag);
            operational.SetFlag(DrainableFlag, flag);
        }

        private void OnConduitUpdate(float dt)
        {
            selectable.ToggleStatusItem(Db.Get().BuildingStatusItems.ConduitBlocked,
                dispenser.ConduitContents.mass > 0f);
        }

#pragma warning disable 649
        [MyCmpGet] private ElementConsumer consumer;
        [MyCmpGet] private ConduitDispenser dispenser;
        [MyCmpGet] private Storage storage;
        [MyCmpReq] private Operational operational;
        [MyCmpGet] private KSelectable selectable;
        [MyCmpGet] private KAnimControllerBase anim;
#pragma warning restore 649
    }
}