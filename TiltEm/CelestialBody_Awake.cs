using Harmony;
// ReSharper disable All

namespace TiltEm
{
    /// <summary>
    /// This harmony patch is intended to update the body rotation after KSP has done it's thing
    /// </summary>
    [HarmonyPatch(typeof(CelestialBody))]
    [HarmonyPatch("Awake")]
    public class CelestialBody_Awake
    {
        [HarmonyPostfix]
        private static void PostFixCBUpdate(CelestialBody __instance)
        {
            TiltEm.CelestialBodyAwake(__instance);
        }
    }
}
