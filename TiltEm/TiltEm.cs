using Harmony;
using KSP.UI.Screens;
using System;
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
        public static Action[] DebugActions = new Action[10];
#endif

        public static TiltEm Singleton;
        public static readonly Dictionary<string, Vector3d> TiltDictionary = new Dictionary<string, Vector3d>();
        public static HarmonyInstance HarmonyInstance = HarmonyInstance.Create("TiltEm");

        public void Awake()
        {
            Singleton = this;
            DontDestroyOnLoad(this);
            Debug.Log("[TiltEm]: TiltEm started!");

            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onLevelWasLoadedGUIReady.Add(LevelWasLoaded);

#if DEBUG
            GameEvents.onGUIApplicationLauncherReady.Add(EnableToolBar);
            DefineDebugActions();
#endif
        }

#if DEBUG

        /// <summary>
        /// Define actions that you want to be executed when pressing the A0-A9 buttons
        /// </summary>
        public void DefineDebugActions()
        {
            DebugActions[0] = () => { };
            DebugActions[1] = () => { };
            DebugActions[2] = () => { };
            DebugActions[3] = () => { };
            DebugActions[4] = () => { };
            DebugActions[5] = () => { };
            DebugActions[6] = () => { };
            DebugActions[7] = () => { };
            DebugActions[8] = () => { };
            DebugActions[9] = () => { };
        }

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

        public void Update()
        {
            if (FlightGlobals.fetch && FlightGlobals.ActiveVessel && (!FlightGlobals.ActiveVessel.mainBody || FlightGlobals.ActiveVessel.mainBody && !FlightGlobals.ActiveVessel.mainBody.inverseRotation))
            {
                TiltEmUtil.RestorePlanetariumTilt();
            }
        }

        #region Game events

        /// <summary>
        /// When loading a vessel that doesn't have a main body or that is not in inverse rotation we rotate the bodies.
        /// Otherwise if the vessel has a main body and is rotating we rotate the planetarium
        /// </summary>
        private void OnVesselChange(Vessel vessel)
        {
            if (vessel.mainBody && vessel.mainBody.inverseRotation)
            {
                TiltEmUtil.RestorePlanetTilt(vessel.mainBody);
            }
            else
            {
                TiltEmUtil.RestorePlanetariumTilt();
            }
        }

        /// <summary>
        /// When loading a scene that doesn't have a main body we rotate the bodies.
        /// Otherwise if the scene has a main body and we are rotating, we rotate the planetarium instead
        /// </summary>
        private void LevelWasLoaded(GameScenes data)
        {
            if (data < GameScenes.SPACECENTER) return;

            if (FlightGlobals.currentMainBody && FlightGlobals.currentMainBody.inverseRotation)
            {
                TiltEmUtil.RestorePlanetTilt(FlightGlobals.currentMainBody);
            }
            else
            {
                TiltEmUtil.RestorePlanetariumTilt();
            }
        }
        
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

        #endregion

        /// <summary>
        /// Gets the tilt magnitude to display it in a UI for the given body
        /// </summary>
        public static string GetTiltForDisplay(string bodyName)
        {
            return !TiltDictionary.TryGetValue(bodyName, out var tilt) ? "0" : KSPUtil.LocalizeNumber(tilt.magnitude, "F2");
        }

        /// <summary>
        /// Returns the given tilt if found
        /// </summary>
        public static bool TryGetTilt(string bodyName, out Vector3d tilt)
        {
            return TiltDictionary.TryGetValue(bodyName, out tilt);
        }
    }
}
