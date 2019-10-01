using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OilFish
{
    class CheckPressure : KMonoBehaviour, ISim4000ms
    {
        public float pressure = 1f;

        public System.Action<float> OnPressure;

        public void Sim4000ms(float dt)
        {
            // Average the pressure in the surrounding nine tiles, excluding solids.
            int num_cells = 0;
            float sum = 0;
            int root_cell = Grid.PosToCell(transform.GetPosition());
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var cell = Grid.OffsetCell(root_cell, new CellOffset(dx, dy));
                    if (Grid.IsValidCell(cell) && !Grid.IsSolidCell(cell) && !Grid.Element[cell].IsSolid)
                    {
                        num_cells += 1;
                        sum += Grid.Mass[cell];
                    }
                }
            }
            float average = num_cells == 0 ? 0 : sum / num_cells;
            if (average >= pressure)
            {
                OnPressure?.Invoke(dt);
            }
        }
    }
}
