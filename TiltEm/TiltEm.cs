using Harmony;
using KSP.UI.Screens;
using System.Reflection;
using UnityEngine;

namespace TiltEm
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class TiltEm : MonoBehaviour
    {
        private class CbParent : MonoBehaviour
        {

        }


        public static CelestialBody Kerbin => FlightGlobals.GetBodyByName("Kerbin");
        public static HarmonyInstance HarmonyInstance = HarmonyInstance.Create("TiltEm");

        public static Vector3 RotationToApplyBkp = Vector3.zero;
        public static Vector3 RotationToApply = Vector3.zero;
        public static bool ResetRequested;

        public static bool InertialFrameOfReference => FlightGlobals.currentMainBody != null && (!FlightGlobals.currentMainBody.rotates || !FlightGlobals.currentMainBody.inverseRotation);

        public static bool ApplyRotationOnlyOnce => HighLogic.LoadedScene == GameScenes.SPACECENTER ||
            HighLogic.LoadedScene == GameScenes.FLIGHT && !InertialFrameOfReference;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            GameEvents.onGUIApplicationLauncherReady.Add(EnableToolBar);
            GameEvents.onLevelWasLoaded.Add(LevelLoaded);
        }

        private void LevelLoaded(GameScenes data)
        {
            if (data == GameScenes.TRACKSTATION)
            {
                RotationToApply = RotationToApplyBkp;
            }
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

            if (RotationToApply != Vector3.zero)
            {
                body.bodyTransform.Rotate(RotationToApply, Space.World);

                if (ApplyRotationOnlyOnce)
                {
                    RotationToApplyBkp = RotationToApply;
                    RotationToApply = Vector3.zero;
                }
            }

            if (ResetRequested)
            {
                body.bodyTransform.Rotate(new Vector3(0, body.bodyTransform.rotation.eulerAngles.y, 0));

                RotationToApplyBkp = RotationToApply = Vector3.zero;
                ResetRequested = false;
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
