using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TiltEm.Event
{
    public class RotatingFrameEvents : TiltEmBaseEvent
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public static EventData<GameEvents.HostTargetAction<CelestialBody, bool>> beforeRotatingFrameChange;
    }
}
