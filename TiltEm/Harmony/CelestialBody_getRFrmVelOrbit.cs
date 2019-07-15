//using Harmony;
//using System;

//// ReSharper disable All

//namespace TiltEm.Harmony
//{
//    /// <summary>
//    /// This harmony patch is intended to 
//    /// </summary>
//    [HarmonyPatch(typeof(CelestialBody))]
//    [HarmonyPatch("getRFrmVelOrbit")]
//    internal class CelestialBody_getRFrmVelOrbit
//    {
//        [HarmonyPostfix]
//        private static void PostfixgetRFrmVelOrbit(CelestialBody __instance, ref Vector3d __result, Orbit o)
//        {
//            __result = Vector3d.Cross(Vector3d.down * (Math.PI * 2 * __instance.rotPeriodRecip), o.pos.xzy);
//        }
//    }
//}
