using Harmony;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This harmony patch is intended to update the orbits and apply them the planet tilt
    /// </summary>
    [HarmonyPatch(typeof(OrbitDriver))]
    [HarmonyPatch("UpdateOrbit")]
    internal class OrbitDriver_UpdateOrbit
    {
        private static Vector3d Tilt;

        [HarmonyPrefix]
        private static void PreFixUpdateOrbit(OrbitDriver __instance, ref bool __state)
        {
            if (__instance.vessel == FlightGlobals.ActiveVessel) return;

            __state = false;
            if (__instance.referenceBody && __instance.referenceBody.inverseRotation && TiltEm.TryGetTilt(__instance.referenceBody.bodyName, out Tilt))
            {
                __state = true;

                TiltEmUtil.ApplyTiltToFrame(ref __instance.orbit.OrbitFrame, -1 * Tilt);
            }
        }

        [HarmonyPostfix]
        private static void PostFixUpdateOrbit(OrbitDriver __instance, ref bool __state)
        {
            if (__state)
            {
                TiltEmUtil.ApplyTiltToFrame(ref __instance.orbit.OrbitFrame, Tilt);
            }
        }
    }
}
