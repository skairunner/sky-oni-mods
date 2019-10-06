using KSerialization;
using STRINGS;
using System;
using System.Collections.Generic;
using Klei;
using Klei.AI;
using UnityEngine;

namespace DiseasesReimagined
{
    [SerializationConfig(MemberSerialization.OptIn)]
    public class UVCleaner : KMonoBehaviour, IEffectDescriptor, ISim200ms
    {
        private SunburnReactable reactable;
        #pragma warning disable CS0649
        // These are set magically, so we need to ignore the "never assigned to" warning.
        [MyCmpReq] private BuildingComplete building;
        [MyCmpReq] private ConduitConsumer consumer;
        [MyCmpReq] private KSelectable selectable;
        #pragma warning restore CS0649

        private int waterOutputCell = -1;

        [MyCmpReq] public Operational operational;

        private Guid statusHandle;

        [MyCmpReq] protected Storage storage;

        public void Sim200ms(float dt)
        {
            if (operational != null && !operational.IsOperational)
                operational.SetActive(false);
            else
                UpdateState(dt);
        }

        protected override void OnPrefabInit()
        {
            base.OnPrefabInit();
            Subscribe(-592767678, OnOperationalChangedDelegate);
            Subscribe(824508782, OnActiveChangedDelegate);
        }

        protected override void OnSpawn()
        {
            base.OnSpawn();
            waterOutputCell = building.GetUtilityOutputCell();
            CreateNewReactable();
        }

        protected override void OnCleanUp()
        {
            if (reactable != null)
            {
                reactable.Cleanup();
                reactable = null;
            }
        }

        public void CreateNewReactable()
        {
            reactable = new SunburnReactable(this);
        }

        private void UpdateState(float dt)
        {
            var flag = consumer.IsSatisfied;

            var items = storage.items;
            for (var index = 0; index < items.Count; ++index)
            {
                var component = items[index].GetComponent<PrimaryElement>();
                if (component.Mass > 0.0 && component.Element.IsLiquid)
                {
                    flag = true;
                    var num1 =
                        Game.Instance.liquidConduitFlow
                       .AddElement(waterOutputCell, component.ElementID, component.Mass, component.Temperature,
                                 SimUtil.DiseaseInfo.Invalid.idx, 0);
                    component.KeepZeroMassObject = true;
                    var num2 = num1 / component.Mass;
                    var num3 = (int) (component.DiseaseCount * (double) num2);
                    component.Mass -= num1;
                    component.ModifyDiseaseCount(-num3, "UVCleaner.UpdateState");
                    break;
                }
            }

            operational.SetActive(flag);
            UpdateStatus();
        }

        private static void OnOperationalChanged(UVCleaner component, object data)
        {
            if (component.operational.IsOperational)
                component.UpdateState(0.0f);
        }

        private static void OnActiveChanged(UVCleaner component, object data)
        {
            component.UpdateStatus();
        }

        private void UpdateStatus()
        {
            if (operational.IsActive)
            {
                if (statusHandle == Guid.Empty)
                {
                    statusHandle = selectable.SetStatusItem(Db.Get().StatusItemCategories.Main,
                        Db.Get().BuildingStatusItems.Working);
                }
                else
                {
                    selectable.ReplaceStatusItem(statusHandle, Db.Get().BuildingStatusItems.Working);
                }
            }
            else
            {
                if (statusHandle != Guid.Empty)
                {
                    statusHandle = selectable.RemoveStatusItem(statusHandle);
                }
            }
        }

        public List<Descriptor> GetDescriptors(BuildingDef def)
        {
            List<Descriptor> descriptorList = new List<Descriptor>();
            return descriptorList;
        }

        private static readonly EventSystem.IntraObjectHandler<UVCleaner> OnOperationalChangedDelegate =
            new EventSystem.IntraObjectHandler<UVCleaner>(OnOperationalChanged);

        private static readonly EventSystem.IntraObjectHandler<UVCleaner> OnActiveChangedDelegate =
            new EventSystem.IntraObjectHandler<UVCleaner>(OnActiveChanged);
    }

    public class SunburnReactable : Reactable
    {
        private UVCleaner cleaner;
        
        public SunburnReactable(UVCleaner cleaner)
            : base(cleaner.gameObject, nameof(SunburnReactable), Db.Get().ChoreTypes.Checkpoint, 3, 3)
        {
            this.cleaner = cleaner;
            preventChoreInterruption = false;
        }

        public override bool InternalCanBegin(GameObject reactor, Navigator.ActiveTransition transition)
        {
            return cleaner.operational.IsOperational && !reactor.GetSicknesses().Has(Db.Get().Sicknesses.Sunburn);
        }

        public override void Update(float dt)
        {
            Cleanup(); // immediately cleanup, since getting the disease is all we wanted
        }

        protected override void InternalBegin()
        {
            var sickness = new SicknessExposureInfo(Db.Get().Sicknesses.Sunburn.Id, "UV Cleaner");
            reactor.GetSicknesses().Infect(sickness);
            cleaner.CreateNewReactable();
        }

        protected override void InternalEnd()
        {
        }

        protected override void InternalCleanup()
        {
        }
    }
}
