using Harmony;
using KSP.Localization;
using KSP.UI;
using KSP.UI.Screens;
using System.Collections.Generic;

// ReSharper disable All

namespace TiltEmCommon
{
    /// <summary>
    /// This harmony patch is intended to display the axial tilt in the track station knowledge base
    /// </summary>
    [HarmonyPatch(typeof(KbApp_PlanetParameters))]
    [HarmonyPatch("CreatePhysicalCharacteristics")]
    internal class KbAppPlanetParameters_CreatePhysicalCharacteristics
    {
        [HarmonyPostfix]
        private static void PostFixCreatePhysicalCharacteristics(KbApp_PlanetParameters __instance, List<UIListItem> __result)
        {
            var deg = TiltEmShared.GetTiltForDisplay(__instance.currentBody.bodyName);
            var uIListItem = __instance.cascadingList.CreateBody(Localizer.Format("#autoLOC_TiltEm_AxialTilt"), string.Concat(new string[] { "<color=#b8f4d1>",
                Localizer.Format("#autoLOC_TiltEm_AxialTiltDisplay", deg), "</color>" }));

            __result.Add(uIListItem);
        }
    }
}
