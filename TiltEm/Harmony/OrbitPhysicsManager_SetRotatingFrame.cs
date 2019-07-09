using Harmony;
using System.Collections;
using System.Reflection;
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

        [HarmonyPostfix]
        private static void PostfixSetRotatingFrame(OrbitPhysicsManager __instance, bool rotatingFrameState)
        {
            if (TiltEm.TryGetTilt(__instance.dominantBody.bodyName, out var tilt))
            {
                foreach (var vessel in FlightGlobals.VesselsLoaded)
                {
                    vessel.GoOnRails();
                    OrbitPhysicsManager.HoldVesselUnpack(10);
                }
            }
        }
    }
}
