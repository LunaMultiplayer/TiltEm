using HarmonyLib;
using TiltEm.Event;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This harmony patch is intended to trigger an event before setting the rotating frame
    /// </summary>
    [HarmonyPatch(typeof(OrbitPhysicsManager))]
    [HarmonyPatch("setRotatingFrame")]
    internal class OrbitPhysicsManager_SetRotatingFrame
    {
        [HarmonyPrefix]
        private static void PrefixSetRotatingFrame(OrbitPhysicsManager __instance, bool rotatingFrameState)
        {
            RotatingFrameEvents.beforeRotatingFrameChange.Fire(new GameEvents.HostTargetAction<CelestialBody, bool>(__instance.dominantBody, rotatingFrameState));
        }
    }
}
