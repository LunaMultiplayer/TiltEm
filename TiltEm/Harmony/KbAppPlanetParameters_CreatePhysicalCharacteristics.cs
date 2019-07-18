using Harmony;
using KSP.Localization;
using KSP.UI;
using KSP.UI.Screens;
using System.Collections.Generic;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This harmony patch is intended to display the axial tilt in the tracking station knowledge base
    /// </summary>
    [HarmonyPatch(typeof(KbApp_PlanetParameters))]
    [HarmonyPatch("CreatePhysicalCharacteristics")]
    internal class KbAppPlanetParameters_CreatePhysicalCharacteristics
    {
        private static string Title => Localizer.TryGetStringByTag("#autoLOC_TiltEm_AxialTilt", out var localizedTitle) ? localizedTitle : "Obliquity";
        
        [HarmonyPostfix]
        private static void PostfixCreatePhysicalCharacteristics(KbApp_PlanetParameters __instance, List<UIListItem> __result)
        {
            var uIListItem = __instance.cascadingList.CreateBody(Title, string.Concat(new string[] { "<color=#b8f4d1>",
                $"{TiltEm.GetTiltForDisplay(__instance.currentBody.bodyName)}°", "</color>" }));

            __result.Add(uIListItem);
        }
    }
}
