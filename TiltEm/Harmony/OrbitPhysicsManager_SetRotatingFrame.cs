using Harmony;
using System.Collections;
using UnityEngine;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This harmony patch is intended to restore the planetarium or the planet tilt when changing the rotating frame
    /// </summary>
    [HarmonyPatch(typeof(OrbitPhysicsManager))]
    [HarmonyPatch("setRotatingFrame")]
    internal class OrbitPhysicsManager_SetRotatingFrame
    {
        [HarmonyPrefix]
        private static void PrefixSetRotatingFrame(OrbitPhysicsManager __instance, bool rotatingFrameState)
        {
            if (__instance.dominantBody.inverseRotation != rotatingFrameState)
            {
                if (rotatingFrameState)
                {
                    for (var i = 0; i < FlightGlobals.VesselsLoaded.Count; i++)
                    {
                        var vessel = FlightGlobals.VesselsLoaded[i];
                        if (vessel.orbitDriver.updateMode == OrbitDriver.UpdateMode.UPDATE && TiltEm.TryGetTilt(vessel.mainBody.bodyName, out var tilt))
                        {
                            TiltEmUtil.ApplyTiltToFrame(ref vessel.orbit.OrbitFrame, tilt);
                        }
                    }
                }
                else
                {
                    //for (var i = 0; i < FlightGlobals.VesselsLoaded.Count; i++)
                    //{
                    //    var vessel = FlightGlobals.VesselsLoaded[i];
                    //    if (TiltEm.TryGetTilt(vessel.mainBody.bodyName, out var tilt))
                    //    {
                    //        if (TiltEm.DebugSwitches[0])
                    //        {
                    //            TiltEmUtil.ApplyTiltToFrame(ref vessel.orbit.OrbitFrame, tilt);
                    //        }
                    //        if (TiltEm.DebugSwitches[1])
                    //        {
                    //            Planetarium.CelestialFrame.OrbitalFrame(vessel.orbit.LAN, vessel.orbit.inclination, vessel.orbit.argumentOfPeriapsis, ref vessel.orbit.OrbitFrame);
                    //        }
                    //    }
                    //}
                }
            }
        }

        public static IEnumerator SetOrbitUpdateModeNextFrame(Vessel vessel, OrbitDriver.UpdateMode updateMode)
        {
            yield return new WaitForSeconds(0.1f);
            vessel.orbitDriver.SetOrbitMode(updateMode);
        }
    }
}
