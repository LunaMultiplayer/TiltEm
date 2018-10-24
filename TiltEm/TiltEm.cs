using Harmony;
using KSP.UI.Screens;
using System.Reflection;
using UnityEngine;

namespace TiltEm
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class TiltEm : MonoBehaviour
    {
        public static CelestialBody Kerbin => FlightGlobals.GetBodyByName("Kerbin");
        public static HarmonyInstance HarmonyInstance = HarmonyInstance.Create("TiltEm");

        public static int Tilt = 0;

        public static bool RotatingFrame => FlightGlobals.currentMainBody != null && FlightGlobals.currentMainBody.inverseRotation;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            GameEvents.onGUIApplicationLauncherReady.Add(EnableToolBar);
        }
        
        // ReSharper disable once InconsistentNaming
        public void OnGUI()
        {
            TiltEmGui.SetStyles();
            TiltEmGui.CheckWindowLock();
            TiltEmGui.DrawGui();
        }

        public static void CelestialBodyUpdate(CelestialBody body)
        {
            if (!body.bodyName.Contains("Kerbin")) return;

            if (RotatingFrame)
            {
                Planetarium.Rotation = (Quaternion)Planetarium.Rotation * (Quaternion.Inverse(Planetarium.Rotation) * Quaternion.Euler(Tilt, 0, 0)) * (Quaternion)Planetarium.Rotation;
            }
            else
            {
                body.bodyTransform.transform.Rotate(new Vector3(Tilt, 0, 0), Space.World);
            }
        }

        public void EnableToolBar()
        {
            var buttonTexture = GameDatabase.Instance.GetTexture("TiltEm/Button/TiltEmButton", false);
            GameEvents.onGUIApplicationLauncherReady.Remove(EnableToolBar);
            ApplicationLauncher.Instance.AddModApplication(() => TiltEmGui.Display = true, () => TiltEmGui.Display = false,
                () => { }, () => { }, () => { }, () => { }, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
        }
    }
}
