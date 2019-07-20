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
        #region Fields

        public static TiltEm Singleton;

#if DEBUG
        public static bool[] DebugSwitches { get; } = new bool[10];
        public static Action[] DebugActions { get; } = new Action[10];
#endif
        
        /// <summary>
        /// Here we define the default tilts in case you don't use Kopernicus
        /// </summary>

        public static readonly Dictionary<string, Vector3d> TiltDictionary = new Dictionary<string, Vector3d>
        {
            ["Sun"] = new Vector3d(7.57, 0, 2.12),
            ["Kerbin"] = new Vector3d(20, 0, 5),
            ["Mun"] = new Vector3d(15.45, 0, 10.61),
            ["Minmus"] = new Vector3d(5.87, 0, 12.63),
            ["Moho"] = new Vector3d(15.14, 0, 30.25),
            ["Eve"] = new Vector3d(120.4, 0, 35.82),
            ["Duna"] = new Vector3d(5.93, 0, 3.81),
            ["Ike"] = new Vector3d(15.41, 0, 4.22),
            ["Jool"] = new Vector3d(0.54, 0, 1.16),
            ["Laythe"] = new Vector3d(10.63, 0, 13.45),
            ["Vall"] = new Vector3d(5.5, 0, 6.12),
            ["Bop"] = new Vector3d(7.1, 0, 9.4),
            ["Tylo"] = new Vector3d(17.3, 0, 6),
            ["Gilly"] = new Vector3d(15.7, 0, 9.69),
            ["Pol"] = new Vector3d(15.4, 0, 3.12),
            ["Dres"] = new Vector3d(8.64, 0, 11.48),
            ["Eeloo"] = new Vector3d(80.63, 0, 12.34),
        };

        #endregion

        #region Unity methods

        /// <summary>
        /// Called just when starting
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void Awake()
        {
            Singleton = this;
            DontDestroyOnLoad(this);
            Debug.Log("[TiltEm]: TiltEm started!");

            HarmonyInstance.Create("TiltEm").PatchAll(Assembly.GetExecutingAssembly());
            GameEvents.onGameSceneSwitchRequested.Add(SceneRequested);
            GameEvents.onVesselChange.Add(OnVesselChange);
            GameEvents.onRotatingFrameTransition.Add(RotatingFrameChanged);

#if DEBUG
            GameEvents.onGUIApplicationLauncherReady.Add(EnableToolBar);
            DefineDebugActions();
#endif
        }

#if DEBUG

        /// <summary>
        /// Called on every GUI frame
        /// </summary>
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Global
        public void OnGUI()
        {
            TiltEmGui.SetStyles();
            TiltEmGui.CheckWindowLock();
            TiltEmGui.DrawGui();
        }

#endif

        #endregion

        #region Game events

#if DEBUG

        /// <summary>
        /// Enables the toolbar button
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void EnableToolBar()
        {
            var buttonTexture = GameDatabase.Instance.GetTexture("TiltEm/TiltEmButton", false);
            GameEvents.onGUIApplicationLauncherReady.Remove(EnableToolBar);

            ApplicationLauncher.Instance.AddModApplication(() => TiltEmGui.Display = true, () => TiltEmGui.Display = false,
                () => { }, () => { }, () => { }, () => { }, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
        }

#endif

        /// <summary>
        /// When switching to inverse rotation (below 100K on Kerbin) we must restore the planet tilt to 0 as then the planetarium will be tilted in <see cref="Harmony.CelestialBody_CBUpdate"/>.
        /// When switching to NON inverse rotation (above 100K on Kerbin) we must restore the planetarium tilt and then the planet will be tilted in <see cref="Harmony.CelestialBody_CBUpdate"/>.
        ///
        /// Also we must adjust the orbits of the loaded vessels to match the new tilt
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void RotatingFrameChanged(GameEvents.HostTargetAction<CelestialBody, bool> data)
        {
            if (TryGetTilt(data.host.bodyName, out _))
            {
                //TODO: Here we adjust the loaded vessel orbits that are in physics mode. This creates some small errors in the orbit parameters so more investigation is needed
                foreach (var vessel in FlightGlobals.VesselsLoaded)
                {
                    if (vessel.orbitDriver.updateMode == OrbitDriver.UpdateMode.TRACK_Phys)
                    {
                        if (!data.target) //NOT rotating frame
                        {
                            vessel.orbitDriver.updateFromParameters();
                            vessel.SetPosition(vessel.orbit.getPositionAtUT(Planetarium.GetUniversalTime()), false);
                            vessel.SetWorldVelocity(vessel.orbit.GetVel() - Krakensbane.GetFrameVelocity());
                        }
                        else
                        {
                            vessel.orbitDriver.updateFromParameters();
                            vessel.SetWorldVelocity(vessel.orbit.GetVel() - vessel.orbit.referenceBody.getRFrmVel(vessel.vesselTransform.position) - Krakensbane.GetFrameVelocity());
                        }
                    }
                }
            }

            if (data.host && data.target)
            {
                TiltEmUtil.RestorePlanetTilt(data.host);
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
        // ReSharper disable once MemberCanBeMadeStatic.Local
        private void SceneRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            if (data.from < GameScenes.SPACECENTER || data.to < GameScenes.SPACECENTER) return;

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
        /// When loading a vessel that doesn't have a main body or that is not in inverse rotation we rotate the bodies.
        /// Otherwise if the vessel has a main body and is rotating we rotate the planetarium
        /// </summary>
        // ReSharper disable once MemberCanBeMadeStatic.Local
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
        
#endregion

#region Public accessors

        /// <summary>
        /// Adds the tilt of the body into the system.
        /// Feel free to call this method from another mod.
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
                TiltDictionary[body.bodyName] = tilt;
            }
            else
            {
                TiltDictionary.Add(body.bodyName, tilt);
            }
        }

        /// <summary>
        /// Gets the tilt magnitude to display it in a UI for a given body name
        /// </summary>
        public static string GetTiltForDisplay(string bodyName)
        {
            return !TiltDictionary.TryGetValue(bodyName, out var tilt) ? "0" : KSPUtil.LocalizeNumber(tilt.magnitude, "F2");
        }

        /// <summary>
        /// Returns the given tilt if found in the storage
        /// </summary>
        public static bool TryGetTilt(string bodyName, out Vector3d tilt)
        {
            return TiltDictionary.TryGetValue(bodyName, out tilt);
        }

#endregion

#region Private methods

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

#endif

#endregion

    }
}
