using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ExpandedLights.LightSystemPatch;

namespace ExpandedLights
{
    class RotatableLight: KMonoBehaviour
    {
        [MyCmpReq]
        Rotatable rotatable;
        [MyCmpReq]
        Light2D light2D;
        protected override void OnSpawn()
        {
            SkyLib.Logger.LogLine("DEBUG", rotatable.GetOrientation().ToString());
            var pending_state = Traverse.Create(light2D).Field("pending_emitter_state").GetValue< LightGridManager.LightGridEmitter.State>();
            pending_state.falloffRate = 0f;
            light2D.emitter.Refresh(pending_state, true);
            switch (rotatable.GetOrientation())
            {
                case Orientation.R90:
                    light2D.shape = (LightShape)ExtendedLightShapes.ConeRight;
                    break;
                case Orientation.R180:
                    light2D.shape = LightShape.Cone;
                    break;
                case Orientation.R270:
                    light2D.shape = (LightShape)ExtendedLightShapes.ConeLeft;
                    break;
                case Orientation.Neutral:
                    light2D.shape = (LightShape)ExtendedLightShapes.ConeUp;
                    break;
                default:
                    light2D.shape = LightShape.Circle;
                    break;
            }
        }
    }
}
