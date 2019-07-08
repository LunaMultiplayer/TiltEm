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
        private static bool PrefixSetRotatingFrame(OrbitPhysicsManager __instance, bool rotatingFrameState)
        {
            if (__instance.dominantBody.inverseRotation != rotatingFrameState)
            {
                for (var i = 0; i < FlightGlobals.VesselsLoaded.Count; i++)
                {
                    var vessel = FlightGlobals.VesselsLoaded[i];
                    if (vessel.orbitDriver.updateMode == OrbitDriver.UpdateMode.UPDATE && TiltEm.TryGetTilt(vessel.mainBody.bodyName, out var tilt))
                    {
                        if (rotatingFrameState)
                        {
                            TiltEmUtil.ApplyTiltToFrame(ref vessel.orbit.OrbitFrame, tilt);
                        }
                        else
                        {
                            //Something must be done here...
                        }
                    }
                }
            }

            return true;
        }
    }
}
