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
        private static string Title => Localizer.TryGetStringByTag("#autoLOC_TiltEm_AxialTilt", out var localizedTitle) ? localizedTitle : "Obliquity";

        private static string GetLocalizedDegrees(string bodyName)
        {
            if (Localizer.TryGetStringByTag("#autoLOC_TiltEm_AxialTiltDisplay", out _))
            {
                return Localizer.Format("#autoLOC_TiltEm_AxialTiltDisplay", TiltEmShared.GetTiltForDisplay(bodyName));
            }

            return $"{TiltEmShared.GetTiltForDisplay(bodyName)} deg";
        }

        [HarmonyPostfix]
        private static void PostFixCreatePhysicalCharacteristics(KbApp_PlanetParameters __instance, List<UIListItem> __result)
        {
            var title = Localizer.TryGetStringByTag("#autoLOC_TiltEm_AxialTilt", out var localizedTitle) ? localizedTitle : "Obliquity";
            
            var uIListItem = __instance.cascadingList.CreateBody(Title, string.Concat(new string[] { "<color=#b8f4d1>",
                GetLocalizedDegrees(__instance.currentBody.bodyName), "</color>" }));

            __result.Add(uIListItem);
        }
    }
}
