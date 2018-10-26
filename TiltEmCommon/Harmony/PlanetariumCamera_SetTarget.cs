using Harmony;
using System;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable All

namespace TiltEmCommon
{
    /// <summary>
    /// This harmony patch is intended to remove the renderer of the old target when siwtching to another target in the tracking station
    /// </summary>
    [HarmonyPatch(typeof(PlanetariumCamera))]
    [HarmonyPatch("SetTarget")]
    [HarmonyPatch(new Type[] { typeof(MapObject) })]
    internal class PlanetariumCamera_SetTarget
    {
        [HarmonyPrefix]
        private static void PreFixSetTarget(PlanetariumCamera __instance, MapObject tgt)
        {
            if (__instance.target.celestialBody != null)
            {
                Object.Destroy(__instance.target.celestialBody.scaledBody.GetComponent<LineRenderer>());
            }
        }
    }
}
