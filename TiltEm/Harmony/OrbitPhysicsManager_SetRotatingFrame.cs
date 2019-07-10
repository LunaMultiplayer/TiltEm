using Harmony;

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
            //TODO: This is just a hack and not the nicest way of tilting the orbits of the vessels that are in TRACK_Phys mode...
            //More investigation is needed...

            if (TiltEm.TryGetTilt(__instance.dominantBody.bodyName, out var tilt))
            {
                foreach (var vessel in FlightGlobals.VesselsLoaded)
                {
                    if (vessel.orbitDriver.updateMode == OrbitDriver.UpdateMode.TRACK_Phys)
                    {
                        vessel.SetRotation(TiltEmUtil.ApplyWorldRotation(FlightGlobals.ActiveVessel.transform.rotation, rotatingFrameState ? tilt : -tilt));

                        vessel.CustomGoOnRails();
                        OrbitPhysicsManager.HoldVesselUnpack(10);
                        vessel.IgnoreGForces(20);
                    }
                }
            }
        }
    }
}
