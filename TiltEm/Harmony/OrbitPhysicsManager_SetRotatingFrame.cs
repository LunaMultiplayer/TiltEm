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
        [HarmonyPrefix]
        private static void PrefixSetRotatingFrame(OrbitPhysicsManager __instance, bool rotatingFrameState)
        {
            if (__instance.dominantBody.inverseRotation != rotatingFrameState)
            {
                if (rotatingFrameState)
                {
                    TiltEmUtil.RestorePlanetTilt(__instance.dominantBody);
                    
                    //For all the vessels in physics range, we must set the orbit mode to UPDATE for 1 frame. This is needed as otherwise their position won't be correct after
                    //switching the frame
                    for (var i = 0; i < FlightGlobals.VesselsLoaded.Count; i++)
                    {
                        if (FlightGlobals.VesselsLoaded[i].orbitDriver.updateMode == OrbitDriver.UpdateMode.TRACK_Phys && TiltEm.DebugSwitches[0])
                        {
                            FlightGlobals.VesselsLoaded[i].orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.UPDATE);
                            CallbackUtil.DelayedCallback(1, () => { FlightGlobals.VesselsLoaded[i].orbitDriver.SetOrbitMode(OrbitDriver.UpdateMode.TRACK_Phys); });
                        }
                    }
                }
                else
                {
                    TiltEmUtil.RestorePlanetariumTilt();
                }
            }
        }
    }
}
