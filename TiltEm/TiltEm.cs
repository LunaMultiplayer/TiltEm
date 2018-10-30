using Harmony;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TiltEm
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class TiltEm : MonoBehaviour
    {
#if DEBUG
        public static bool[] DebugSwitches = new bool[10];
#endif

        public static readonly Dictionary<string, Vector3d> TiltDictionary = new Dictionary<string, Vector3d>();
        public static HarmonyInstance HarmonyInstance = HarmonyInstance.Create("TiltEm");

        public void Awake()
        {
            DontDestroyOnLoad(this);
            Debug.Log("[TiltEm]: TiltEm started!");

            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
#if DEBUG
            GameEvents.onGUIApplicationLauncherReady.Add(EnableToolBar);
#endif
        }
        
#if DEBUG
        // ReSharper disable once InconsistentNaming
        public void OnGUI()
        {
            TiltEmGui.SetStyles();
            TiltEmGui.CheckWindowLock();
            TiltEmGui.DrawGui();
        }

        public void EnableToolBar()
        {
            var buttonTexture = GameDatabase.Instance.GetTexture("TiltEm/TiltEmButton", false);
            GameEvents.onGUIApplicationLauncherReady.Remove(EnableToolBar);

            ApplicationLauncher.Instance.AddModApplication(() => TiltEmGui.Display = true, () => TiltEmGui.Display = false,
                () => { }, () => { }, () => { }, () => { }, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
        }
#endif

        /// <summary>
        /// Adds the tilt of the body into the system
        /// </summary>
        public static void AddTiltData(CelestialBody body, Vector3d tilt)
        {
            if (body == null)
            {
                Debug.LogError("[TiltEm]: AddTiltData parameter 'body' cannot be null!");
                return;
            }
            if (TiltDictionary.ContainsKey(body.bodyName))
            {
                Debug.LogError($"[TiltEm]: AddTiltData body {body.bodyName} already exists!");
                return;
            }

            TiltDictionary.Add(body.bodyName, tilt);
        }

        /// <summary>
        /// Gets the tilt magnitude to display it in a UI for the given body
        /// </summary>
        public static string GetTiltForDisplay(string bodyName)
        {
            return !TiltDictionary.TryGetValue(bodyName, out var tilt) ? "0" : KSPUtil.LocalizeNumber(tilt.magnitude, "F2");
        }

        /// <summary>
        /// Applies tilt to the given CelestialBody. If we are in the rotating frame of the given body
        /// we will rotate the planetarium instead
        /// </summary>
        public static void ApplyTiltToCelestialBody(CelestialBody body)
        {
            if (!TiltDictionary.TryGetValue(body.bodyName, out var tilt)) return;

            if (body.inverseRotation)
            {
                //Basically we do the same as body.bodyTransform.transform.Rotate but with the planetarium
                //as we are rotating WITH the planet and in the same reference plane
                Planetarium.Rotation = TiltEmUtil.ApplyWorldRotation(Planetarium.Rotation, tilt);
            }
            else
            {
                body.rotation = TiltEmUtil.ApplyWorldRotation(body.rotation, tilt);
                body.bodyTransform.transform.Rotate(tilt, Space.World);

                //We must fix the bodyFrame vectors as otherwise landed vessels will not compute the axial tilt on track station and they will
                //rotate around the vertical axis as it didn't had tilt
                body.rotation.swizzle.FrameVectors(out body.BodyFrame.X, out body.BodyFrame.Y, out body.BodyFrame.Z);
            }
        }

        public static bool TryGetTilt(string bodyName, out Vector3d tilt)
        {
            return TiltDictionary.TryGetValue(bodyName, out tilt);
        }
    }
}
