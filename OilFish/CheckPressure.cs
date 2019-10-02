using System;

namespace OilFish
{
    internal class CheckPressure : KMonoBehaviour, ISim4000ms
    {
        public Action<float> OnPressure;
        public float pressure = 1f;

        public void Sim4000ms(float dt)
        {
            // Average the pressure in the surrounding nine tiles, excluding solids.
            var num_cells = 0;
            float sum = 0;
            var root_cell = Grid.PosToCell(transform.GetPosition());
            for (var dx = -1; dx <= 1; dx++)
            for (var dy = -1; dy <= 1; dy++)
            {
                var cell = Grid.OffsetCell(root_cell, new CellOffset(dx, dy));
                if (Grid.IsValidCell(cell) && !Grid.IsSolidCell(cell) && !Grid.Element[cell].IsSolid)
                {
                    num_cells += 1;
                    sum += Grid.Mass[cell];
                }
            }

            var average = num_cells == 0 ? 0 : sum / num_cells;
            if (average >= pressure) OnPressure?.Invoke(dt);
        }
    }
}