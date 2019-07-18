using Harmony;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This harmony patch is intended to apply the tilt when getting the Zup frame
    /// This method is called by Orbit.GetOrbitalStateVectorsAtTrueAnomaly, that's called by Orbit.UpdateFromUT and that method is called by the OrbitDriver.updateFromParameters
    /// If you don't call this method, the orbits of the UNLOADED vessels will be drawn correctly but the vessel itself in map view will be displaced
    /// </summary>
    [HarmonyPatch(typeof(Planetarium))]
    [HarmonyPatch("ZupAtT")]
    internal class Planetarium_ZupAtT
    {
        [HarmonyPrefix]
        private static bool PrefixZupAtT(double UT, CelestialBody body, ref Planetarium.CelestialFrame tempZup)
        {
            if (body != null && body.inverseRotation && TiltEm.TryGetTilt(body.bodyName, out var tilt))
            {
                var num = (body.initialRotation + 360 * body.rotPeriodRecip * UT) % 360;
                var num1 = (num - body.directRotAngle) % 360;
                
                Planetarium.CelestialFrame.PlanetaryFrame(tilt.z + Planetarium.InverseRotAngle, 90 + tilt.x, num1 - Planetarium.InverseRotAngle, ref tempZup);

                return false;
            }

            return true;
        }
    }
}
