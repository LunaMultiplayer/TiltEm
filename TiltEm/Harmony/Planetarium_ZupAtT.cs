using Harmony;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This harmony patch is intended to apply the tilt when getting the Zup frame
    /// This method is called by Orbit.GetOrbitalStateVectorsAtTrueAnomaly, that's called by Orbit.UpdateFromUT and that method is called by the OrbitDriver.updateFromParameters
    /// </summary>
    [HarmonyPatch(typeof(Planetarium))]
    [HarmonyPatch("ZupAtT")]
    internal class Planetarium_ZupAtT
    {
        [HarmonyPrefix]
        private static bool PrefixZupAtT(double UT, CelestialBody body, ref Planetarium.CelestialFrame tempZup)
        {
            if (body != null && TiltEm.TryGetTilt(body.bodyName, out var tilt) && body.inverseRotation)
            {
                var num = (body.initialRotation + 360 * body.rotPeriodRecip * UT) % 360;
                var num1 = (num - body.directRotAngle) % 360;
                Planetarium.CelestialFrame.PlanetaryFrame(0, 90, num1, ref tempZup);
                TiltEmUtil.ApplyTiltToFrame(ref tempZup, tilt);

                return false;
            }

            return true;
        }
    }
}
