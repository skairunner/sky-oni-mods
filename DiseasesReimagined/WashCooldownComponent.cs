using UnityEngine;

namespace DiseasesReimagined
{
    // A component which tracks the time since the last hand wash and prevents washing again
    // if it has not been long enough since then.
    public sealed class WashCooldownComponent : MonoBehaviour
    {
        public bool CanWash
        {
            get
            {
                return GameClock.Instance.GetTime() >= NextWashTime;
            }
        }

        // The game time when the next wash can occur
        public float NextWashTime { get; set; }

        public void OnWashComplete()
        {
            NextWashTime = GameClock.Instance.GetTime() + GermExposureTuning.WASH_COOLDOWN;
        }
    }
}
