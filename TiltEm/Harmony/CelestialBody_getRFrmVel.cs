//using Harmony;
//using System;

//// ReSharper disable All

//namespace TiltEm.Harmony
//{
//    /// <summary>
//    /// This harmony patch is intended to 
//    /// </summary>
//    [HarmonyPatch(typeof(CelestialBody))]
//    [HarmonyPatch("getRFrmVel")]
//    internal class CelestialBody_getRFrmVel
//    {
//        [HarmonyPostfix]
//        private static void PostfixgetRFrmVel(CelestialBody __instance, ref Vector3d __result, Vector3d worldPos)
//        {
//            __result = Vector3d.Cross(Vector3d.down * (Math.PI * 2 * __instance.rotPeriodRecip), worldPos - __instance.position);
//        }
//    }
//}
