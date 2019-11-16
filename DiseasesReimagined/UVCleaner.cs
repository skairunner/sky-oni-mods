using KSerialization;
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
        // The germ fraction to remove.
        public const float GERM_REMOVAL = 0.95f;

        // Fewer than this many germs left are destroyed completely.
        public const float MIN_GERMS_PER_KG = 50.0f;

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
            Subscribe((int)GameHashes.OperationalChanged, OnOperationalChangedDelegate);
            Subscribe((int)GameHashes.ActiveChanged, OnActiveChangedDelegate);
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

        private void UpdateState(float _)
        {
            var hasLiquid = consumer.IsSatisfied;
            byte invalid = SimUtil.DiseaseInfo.Invalid.idx;
            foreach (var item in storage.items)
            {
                var pe = item.GetComponent<PrimaryElement>();
                float mass = pe.Mass;
                // Is it liquid?
                if (mass > 0.0f && pe.Element.IsLiquid)
                {
                    byte germID = pe.DiseaseIdx;
                    // Remove the fraction and destroy if too few
                    int oldGerms = pe.DiseaseCount, newGerms = Mathf.RoundToInt((1.0f -
                        GERM_REMOVAL) * oldGerms);
                    if (germID == invalid || newGerms < MIN_GERMS_PER_KG * 5.0f)
                    {
                        newGerms = 0;
                        germID = invalid;
                    }
                    float newMass = Game.Instance.liquidConduitFlow.AddElement(waterOutputCell,
                        pe.ElementID, mass, pe.Temperature, germID, newGerms);
                    hasLiquid = true;
                    pe.KeepZeroMassObject = true;
                    // Remove the germs from the input
                    int removeGerms = Mathf.RoundToInt(oldGerms * (newMass / mass));
                    pe.Mass = mass - newMass;
                    pe.ModifyDiseaseCount(-removeGerms, "UVCleaner.UpdateState");
                    break;
                }
            }
            operational.SetActive(hasLiquid);
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
            return new List<Descriptor>();
        }

        private static readonly EventSystem.IntraObjectHandler<UVCleaner> OnOperationalChangedDelegate =
            new EventSystem.IntraObjectHandler<UVCleaner>(OnOperationalChanged);

        private static readonly EventSystem.IntraObjectHandler<UVCleaner> OnActiveChangedDelegate =
            new EventSystem.IntraObjectHandler<UVCleaner>(OnActiveChanged);
    }

    /// <summary>
    /// Burns Duplicants who dare pass too close to a running cleaner.
    /// </summary>
    public sealed class SunburnReactable : Reactable
    {
        private readonly UVCleaner cleaner;
        
        public SunburnReactable(UVCleaner cleaner)
            : base(cleaner.gameObject, nameof(SunburnReactable), Db.Get().ChoreTypes.Checkpoint, 3, 3)
        {
            this.cleaner = cleaner;
            preventChoreInterruption = false;
        }

        public override bool InternalCanBegin(GameObject reactor, Navigator.ActiveTransition transition)
        {
            return cleaner.operational.IsActive && !reactor.GetSicknesses().Has(Db.Get().
                Sicknesses.Sunburn);
        }

        public override void Update(float dt)
        {
            Cleanup(); // immediately cleanup, since getting the disease is all we wanted
        }

        protected override void InternalBegin()
        {
            var sickness = new SicknessExposureInfo(Db.Get().Sicknesses.Sunburn.Id,
                UVCleanerConfig.DISPLAY_NAME);
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
