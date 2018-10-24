using Harmony;
using UnityEngine;

namespace TiltEm
{
    /// <summary>
    /// This harmony patch is intended to not allow you generating contracts in case you don't have the contract lock
    /// </summary>
    [HarmonyPatch(typeof(CelestialBody))]
    [HarmonyPatch("CBUpdate")]
    public class CelestialBody_CBUpdate
    {
        [HarmonyPostfix]
        private static void PostFixCBUpdate(CelestialBody __instance)
        {
            if (!__instance.bodyName.Contains("Kerbin")) return;

            if (__instance.bodyTransform.rotation.eulerAngles.x == 0)
            {
                __instance.bodyTransform.Rotate(new Vector3(45, 0, 0));
            }
        }
    }
}
