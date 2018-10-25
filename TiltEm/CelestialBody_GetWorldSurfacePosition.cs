using Harmony;

// ReSharper disable All

namespace TiltEm
{
    /// <summary>
    /// This harmony patch is intended to return a correct world surface position that takes the rotation into the calcs
    /// </summary>
    [HarmonyPatch(typeof(CelestialBody))]
    [HarmonyPatch("GetWorldSurfacePosition")]
    public class CelestialBody_GetWorldSurfacePosition
    {
        [HarmonyPostfix]
        private static Vector3d PostGetWorldSurfacePosition(Vector3d __result, CelestialBody __instance)
        {
            return __result;
        }
    }
}
