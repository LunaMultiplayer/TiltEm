using Harmony;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This harmony patch is intended to update the body rotation after KSP has done it's thing
    /// </summary>
    [HarmonyPatch(typeof(CelestialBody))]
    [HarmonyPatch("CBUpdate")]
    internal class CelestialBody_CBUpdate
    {
        [HarmonyPostfix]
        private static void PostFixCBUpdate(CelestialBody __instance)
        {
            TiltEm.ApplyTiltToCelestialBody(__instance);
        }
    }
}
