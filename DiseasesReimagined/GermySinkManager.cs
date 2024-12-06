using System;
using System.Collections.Generic;
using UnityEngine;
using Klei;

namespace DiseasesReimagined
{
    // Manages transferring germs from sink or shower materials to their users.
    internal sealed class GermySinkManager : IDisposable
    {
        // The only instance of this class.
        public static GermySinkManager Instance { get; private set; }

        public static void CreateInstance()
        {
            DestroyInstance();
            Instance = new GermySinkManager();
        }

        public static void DestroyInstance()
        {
            Instance?.Dispose();
            Instance = null;
        }
        
        // Searches through storage for the specified element and quantity, reporting how many
        // germs there would be if this amount was consumed and returning the amount that could
        // actually be consumed
        private static float GetGermsInStorage(Storage storage, Tag itemTag, float required,
            out SimUtil.DiseaseInfo germs)
        {
            float diseaseCount = 0.0f;
            byte disease = 0;
            float remaining = required;
            // Add up items that would be used, and calculate amount that would be
            // consumed
            PrimaryElement item;
            while (remaining > 0.0f && (item = storage.FindFirstWithMass(itemTag)) != null)
            {
                if (item.TryGetComponent(out PrimaryElement element))
                {
                    float mass = element.Mass, toConsume = Mathf.Min(mass, remaining);
                    // Calculate germs to be transferred
                    byte eDisease = element.DiseaseIdx;
                    float eCount = element.DiseaseCount * toConsume / mass;
                    if (eCount > 0.0f && (diseaseCount <= 0.0f || disease ==
                        eDisease))
                    {
                        disease = eDisease;
                        diseaseCount += eCount;
                    }
                    remaining -= toConsume;
                }
            }
            germs = new SimUtil.DiseaseInfo()
            {
                count = Mathf.RoundToInt(diseaseCount), idx = disease
            };
            return required - remaining;
        }

        // For each sink/shower chore, stores how many germs were consumed during it
        private readonly IDictionary<Workable, SimUtil.DiseaseInfo> germsConsumed;

        private GermySinkManager()
        {
            germsConsumed = new Dictionary<Workable, SimUtil.DiseaseInfo>(16);
        }

        public void Dispose()
        {
            germsConsumed.Clear();
        }

        // Transfer a fraction of consumed germs to the worker and finish the chore
        public void FinishGermyWork(Workable work, KMonoBehaviour worker)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));
            if (worker == null)
                throw new ArgumentNullException(nameof(worker));
            var obj = worker.gameObject;
            if (germsConsumed.TryGetValue(work, out var germs) && obj != null)
            {
                // Less than 1 germ = no transfer
                int count = germs.count;
                if (count > 0 && obj.TryGetComponent(out PrimaryElement dupe))
                {
                    dupe.AddDisease(germs.idx, Mathf.RoundToInt(GermExposureTuning.
                        SINK_GERMS_TO_TRANSFER * count), "GermySinkManager.FinishGermyWork");
                    obj.AddOrGet<WashCooldownComponent>().OnWashComplete();
                }
            }
        }

        // Accumulate how many germs remain
        public void ShowerWorkTick(Workable work, float dt)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));
            var obj = work.gameObject;
            if (germsConsumed.TryGetValue(work, out var germs) && obj != null && obj.
                TryGetComponent(out ElementConverter converter) && obj.TryGetComponent(
                out Storage storage))
            {
                int diseaseCount = germs.count;
                byte disease = germs.idx;
                // Only if a converter with available contents
                foreach (var element in converter.consumedElements)
                {
                    float required = element.MassConsumptionRate * dt;
                    // See how many germs would be removed, and if the element converter
                    // can run, add those germs
                    if (GetGermsInStorage(storage, element.Tag, required,
                        out var itemGerm) >= required && itemGerm.count > 0 &&
                        (diseaseCount < 1 || itemGerm.idx == disease))
                    {
                        diseaseCount += itemGerm.count;
                        disease = itemGerm.idx;
                    }
                }
                germs.count = diseaseCount;
                germs.idx = disease;
                // It is a struct, must store it back
                germsConsumed[work] = germs;
            }
        }

        // Accumulate how many germs remain
        public void SinkWorkTick(Workable work, float dt)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));
            var obj = work.gameObject;
            if (germsConsumed.TryGetValue(work, out var germs) && obj != null && obj.
                TryGetComponent(out HandSanitizer sanitizer) && obj.TryGetComponent(
                out Storage storage))
            {
                // Only if a hand sanitizer with available contents
                float time = work.workTime, qty = sanitizer.massConsumedPerUse * dt / time;
                GetGermsInStorage(storage, ElementLoader.FindElementByHash(sanitizer.
                    consumedElement).tag, qty, out var itemGerm);
                // Only transfer if disease matches and germ count is nonzero
                if (itemGerm.count > 0 && (germs.count < 1 || germs.idx == itemGerm.idx))
                {
                    germs.count += itemGerm.count;
                    germs.idx = itemGerm.idx;
                    // It is a struct, must store it back
                    germsConsumed[work] = germs;
                }
            }
        }

        // Starts a sink/shower work entry for germ tracking
        public void StartGermyWork(Workable work)
        {
            if (work == null)
                throw new ArgumentNullException(nameof(work));
            // Be charitable and trash any germs from aborted chores
            germsConsumed[work] = new SimUtil.DiseaseInfo();
        }
    }
}
