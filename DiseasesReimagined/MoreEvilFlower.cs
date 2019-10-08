using Klei;
using UnityEngine;

namespace DiseasesReimagined
{
    // Makes zombie flowers even more evil if wild, causing them to spread spores into the
    // tile on which they stand.
    public sealed class MoreEvilFlower : KMonoBehaviour, ISim4000ms
    {
        // How many germs per second to infect the tile it is standing on.
        private const float GERMS_PER_SECOND = 1000.0f;

        // The cached disease index to use for infection.
        private byte disease = SimUtil.DiseaseInfo.Invalid.idx;

        // These are populated automatically
#pragma warning disable IDE0044 // Add readonly modifier
        [MyCmpGet]
        private WiltCondition isWilted;

        [MyCmpGet]
        private UprootedMonitor uprooted;
#pragma warning restore IDE0044 // Add readonly modifier

        protected override void OnSpawn()
        {
            base.OnSpawn();
            disease = Db.Get().Diseases.GetIndex("ZombieSpores");
        }

        public void Sim4000ms(float dt)
        {
            var obj = gameObject;
            int cell;
            if (obj != null && Grid.IsValidCell(cell = Grid.PosToCell(obj)) && isWilted?.
                IsWilting() == false && disease != SimUtil.DiseaseInfo.Invalid.idx &&
                uprooted != null)
            {
                cell = Grid.OffsetCell(cell, uprooted.monitorCell);
                if (Grid.IsValidCell(cell) && Grid.Solid[cell])
                    // Flower is growing and on a solid cell, infect it!
                    SimMessages.ModifyDiseaseOnCell(cell, disease, Mathf.RoundToInt(
                        GERMS_PER_SECOND / dt));
            }
        }
    }
}
