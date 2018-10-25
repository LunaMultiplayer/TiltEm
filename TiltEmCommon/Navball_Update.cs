using Harmony;
using KSP.UI.Screens.Flight;

// ReSharper disable All

namespace TiltEmCommon
{
    /// <summary>
    /// This harmony patch is intended to fix the rotation of the navball so it's tilted to the PLANET rotation axis
    /// </summary>
    [HarmonyPatch(typeof(NavBall))]
    [HarmonyPatch("Update")]
    internal class NavBall_Update
    {
        [HarmonyPostfix]
        private static void PostFixNavBall_Update(NavBall __instance)
        {
            __instance.navBall.rotation = TiltEmShared.ApplySpaceRotation(__instance.relativeGymbal, TiltEmShared.GetTilt(FlightGlobals.currentMainBody.bodyName));
        }
    }
}
