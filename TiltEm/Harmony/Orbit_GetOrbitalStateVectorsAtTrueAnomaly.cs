using Harmony;
using System;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This harmony patch is intended to apply the tilt when getting the state vectors at true anomaly
    /// This method is called by Orbit.UpdateFromUT and that method is called by the OrbitDriver.updateFromParameters
    /// </summary>
    [HarmonyPatch(typeof(Orbit))]
    [HarmonyPatch("GetOrbitalStateVectorsAtTrueAnomaly")]
    internal class Orbit_GetOrbitalStateVectorsAtTrueAnomaly
    {
        [HarmonyPrefix]
        private static bool PrefixGetOrbitalStateVectorsAtTrueAnomaly(Orbit __instance, ref double __result, double tA, double UT, out Vector3d pos, out Vector3d vel)
        {
            var tACos = Math.Cos(tA);
            var tASin = Math.Sin(tA);

            var num2 = __instance.semiMajorAxis * (1 - __instance.eccentricity * __instance.eccentricity);
            var num3 = Math.Sqrt(__instance.referenceBody.gravParameter / num2);
            var num4 = -tASin * num3;
            var num5 = (tACos + __instance.eccentricity) * num3;
            var num6 = num2 / (1 + __instance.eccentricity * tACos);
            var num7 = tACos * num6;
            var num8 = tASin * num6;

            var celestialFrame = new Planetarium.CelestialFrame();
            Planetarium.ZupAtT(UT, __instance.referenceBody, ref celestialFrame);

            if (TiltEm.TryGetTilt(__instance.referenceBody.bodyName, out var tilt) && __instance.referenceBody != null && __instance.referenceBody.inverseRotation)
            {
                TiltEmUtil.ApplyTiltToFrame(ref celestialFrame, tilt);
            }

            pos = celestialFrame.WorldToLocal((__instance.OrbitFrame.X * num7) + (__instance.OrbitFrame.Y * num8));
            vel = celestialFrame.WorldToLocal((__instance.OrbitFrame.X * num4) + (__instance.OrbitFrame.Y * num5));
            __result = num6;

            return false;
        }
    }
}
