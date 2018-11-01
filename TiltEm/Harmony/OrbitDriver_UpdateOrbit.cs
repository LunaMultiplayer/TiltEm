﻿using Harmony;

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
        [HarmonyPrefix]
        private static void PreFixUpdateOrbit(OrbitDriver __instance, ref bool __state)
        {
            if (__instance.referenceBody && __instance.referenceBody.inverseRotation && TiltEm.TryGetTilt(__instance.referenceBody.bodyName, out var tilt))
            {
                __state = true;
                TiltEmUtil.ApplyTiltToFrame(ref __instance.orbit.OrbitFrame, tilt);
            }
        }

        [HarmonyPostfix]
        private static void PostFixUpdateOrbit(OrbitDriver __instance, ref bool __state)
        {
            if (__state)
            {
                Planetarium.CelestialFrame.OrbitalFrame(__instance.orbit.LAN, __instance.orbit.inclination, __instance.orbit.argumentOfPeriapsis, ref __instance.orbit.OrbitFrame);
            }
        }
    }
}