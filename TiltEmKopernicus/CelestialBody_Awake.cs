using Harmony;
// ReSharper disable All

namespace TiltEm
{
    /// <summary>
    /// This harmony patch is intended to update the body rotation just when they awake
    /// </summary>
    [HarmonyPatch(typeof(CelestialBody))]
    [HarmonyPatch("Awake")]
    public class CelestialBody_Awake
    {
        [HarmonyPostfix]
        private static void PostFixCBUpdate(CelestialBody __instance)
        {
            TiltEmKopernicus.TiltEm.CelestialBodyAwake(__instance);
        }
    }
}
