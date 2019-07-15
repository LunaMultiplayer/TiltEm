//using Harmony;
//using System;

//// ReSharper disable All

//namespace TiltEm.Harmony
//{
//    /// <summary>
//    /// This harmony patch is intended to 
//    /// </summary>
//    [HarmonyPatch(typeof(Orbit))]
//    [HarmonyPatch("GetRotFrameVel")]
//    internal class Orbit_GetRotFrameVel
//    {
//        [HarmonyPostfix]
//        private static void PostfixGetRotFrameVelAtPos(Orbit __instance, ref Vector3d __result, CelestialBody refBody)
//        {
//            if (__result != Vector3d.zero && TiltEm.TryGetTilt(refBody.bodyName, out _))
//            {
//                __result = Vector3d.Cross(Vector3d.back * (Math.PI * 2 * refBody.rotPeriodRecip), -__instance.pos);
//            }
//        }
//    }
//}
