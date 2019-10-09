namespace CarbonRevolution
{
    public class CoalPlant : StateMachineComponent<CoalPlant.StatesInstance>
    {
        private static readonly EventSystem.IntraObjectHandler<CoalPlant> OnWiltDelegate = 
            new EventSystem.IntraObjectHandler<CoalPlant>(OnWilt);
        private static readonly EventSystem.IntraObjectHandler<CoalPlant> OnWiltRecoverDelegate = 
            new EventSystem.IntraObjectHandler<CoalPlant>(OnWiltRecover);

        protected override void OnSpawn()
        {
            base.OnSpawn();
            Subscribe(-724860998, OnWiltDelegate);
            Subscribe(712767498, OnWiltRecoverDelegate);
        }

        private static void OnWilt(CoalPlant plant, object data = null)
        {
            plant.gameObject.GetComponent<ElementConsumer>().EnableConsumption(false);
        }

        private static void OnWiltRecover(CoalPlant plant, object data = null)
        {
            plant.gameObject.GetComponent<ElementConsumer>().EnableConsumption(true);
        }

        public class StatesInstance : GameStateMachine<States, StatesInstance, CoalPlant, object>.GameInstance
        {
            public StatesInstance(CoalPlant master)
                : base(master)
            {
            }
        }

        public class States : GameStateMachine<States, StatesInstance, CoalPlant>
        {
            public State alive;

            public override void InitializeStates(out BaseState default_state)
            {
                serializable = true;
                default_state = alive;
                alive.DoNothing();
            }
        }
    }

}