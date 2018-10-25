using Harmony;
// ReSharper disable All

namespace TiltEm
{
    /// <summary>
    /// This harmony patch is intended to update the body rotation after KSP has done it's thing
    /// </summary>
    [HarmonyPatch(typeof(CelestialBody))]
    [HarmonyPatch("CBUpdate")]
    public class CelestialBody_CBUpdate
    {
        [HarmonyPostfix]
        private static void PostFixCBUpdate(CelestialBody __instance)
        {
            TiltEmKopernicus.TiltEm.CelestialBodyUpdate(__instance);
        }
    }
}
