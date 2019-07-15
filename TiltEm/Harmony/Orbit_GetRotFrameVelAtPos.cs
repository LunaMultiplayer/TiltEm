//using Harmony;
//using System;

//// ReSharper disable All

//namespace TiltEm.Harmony
//{
//    /// <summary>
//    /// This harmony patch is intended to 
//    /// </summary>
//    [HarmonyPatch(typeof(Orbit))]
//    [HarmonyPatch("GetRotFrameVelAtPos")]
//    internal class Orbit_GetRotFrameVelAtPos
//    {
//        [HarmonyPostfix]
//        private static void PostfixGetRotFrameVelAtPos(Orbit __instance, ref Vector3d __result, CelestialBody refBody, Vector3d refPos)
//        {
//            if (__result != Vector3d.zero && TiltEm.TryGetTilt(refBody.bodyName, out _))
//            {
//                __result = Vector3d.Cross(Vector3d.back * (Math.PI * 2 * refBody.rotPeriodRecip), -refPos);
//            }
//        }
//    }
//}
