using System.Diagnostics.CodeAnalysis;

namespace TiltEm.Event
{
    public class RotatingFrameEvents : TiltEmBaseEvent
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public static EventData<GameEvents.HostTargetAction<CelestialBody, bool>> beforeRotatingFrameChange;
    }
}
