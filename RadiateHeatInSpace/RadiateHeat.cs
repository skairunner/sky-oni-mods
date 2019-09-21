using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RadiateHeat
{
    class RadiatesHeat : KMonoBehaviour, ISim1000ms
    {
        public float emissivity = .9f;
        public float surface_area = 1f;
        private static double stefanBoltzmanConstant = 5.67e-8;
        private Guid handle_radiating; // essentially a reference to a statusitem in particular
        private Guid handle_notinspace;
        [MyCmpReq]
        private KSelectable selectable; // does tooltip-related stuff
        public CellOffset[] OccupyOffsets;

        public float CurrentCooling { get; private set; }
        private HandleVector<int>.Handle structureTemperature;

        public StatusItem _radiating_status;
        public StatusItem _no_space_status;

        protected override void OnSpawn()
        {
            base.OnSpawn();

            OccupyOffsets = new[] { new CellOffset(0, 0) }; // i am lazy and will only check building root bc it's annoying to account for rotation
            structureTemperature = GameComps.StructureTemperatures.GetHandle(gameObject);
        }

        public void Sim1000ms(float dt)
        {
            float temp = gameObject.GetComponent<PrimaryElement>().Temperature;
            double cooling = radiative_heat(temp);
            if (CheckInSpace())
            {
                if (cooling > 1f)
                {
                    CurrentCooling = (float)cooling;
                    GameComps.StructureTemperatures.ProduceEnergy(structureTemperature, (float)-cooling / 1000, "Radiated", 1f);
                }
                UpdateStatusItem(true);
            } else
            {
                GameComps.StructureTemperatures.ProduceEnergy(structureTemperature, (float) 0, "Radiated", 1f);
                UpdateStatusItem(false);
            }
        }

        private double radiative_heat(float temp)
        {
            return Math.Pow(temp, 4) * stefanBoltzmanConstant * emissivity * surface_area;
        }

        private bool CheckInSpace()
        {
            // Check whether in spaaace
            var root_cell = Grid.PosToCell(this);
            foreach (var offset in OccupyOffsets)
            {
                if (!SkyLib.OniUtils.IsCellExposedToSpace(Grid.OffsetCell(root_cell, offset)))
                {
                    return false;
                }
            }
            return true;
        }
        private static string _FormatStatusCallback(string formatstr, object data)
        {
            var radiate = (RadiatesHeat)data;
            var radiation_rate = GameUtil.GetFormattedHeatEnergyRate(radiate.CurrentCooling);
            return string.Format(formatstr, radiation_rate);
        }

        private void UpdateStatusItem(bool in_space = false)
        {
            // if it's in space, update status.
            if (in_space)
            {
                // Remove outdated status, if it exists
                handle_notinspace = selectable.RemoveStatusItem(handle_notinspace);
                // Update the existing callback
                _radiating_status = new StatusItem($"{RadiatePatch.ModName}_RADIATING", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.HeatFlow.ID);
                _radiating_status.resolveTooltipCallback = _FormatStatusCallback;
                _radiating_status.resolveStringCallback = _FormatStatusCallback;
                if (handle_radiating == Guid.Empty)
                {
                    handle_radiating = selectable.AddStatusItem(_radiating_status, this);
                }
            } else 
            { 
                // Remove outdated status-
                 handle_radiating = selectable.RemoveStatusItem(handle_radiating);

                _no_space_status = new StatusItem($"{RadiatePatch.ModName}_NOTINSPACE", "MISC", "", StatusItem.IconType.Info, NotificationType.Neutral, false, OverlayModes.HeatFlow.ID);
                // add the status item!
                if (handle_notinspace == Guid.Empty)
                {
                    handle_notinspace = selectable.AddStatusItem(_no_space_status, this);
                }
            }
        }
    }
}
