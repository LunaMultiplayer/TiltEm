using Harmony;
using KSP.UI.Screens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TiltEm
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class TiltEm : MonoBehaviour
    {
        public static HarmonyInstance HarmonyInstance = HarmonyInstance.Create("TiltEm");
        public static Dictionary<int, Vector3> TiltDictionary = new Dictionary<int, Vector3>();

        public void Awake()
        {
            DontDestroyOnLoad(this);
            Debug.Log("[TiltEm]: TiltEm started!");

            FillTiltDictionary();
            if (TiltDictionary.Any())
            {
                HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
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
#endif
        public static void CelestialBodyAwake(CelestialBody body)
        {
            if (!TiltDictionary.TryGetValue(body.flightGlobalsIndex, out var tilt)) return;

            body.bodyTransform.transform.Rotate(tilt, Space.World);
        }

        public static void CelestialBodyUpdate(CelestialBody body)
        {
            //Instead of a harmony patch, you can do it in the TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Precalc)
            //That would involve running trough all the celestial bodies with a loop tough...

            if (!TiltDictionary.TryGetValue(body.flightGlobalsIndex, out var tilt)) return;

            if (body.inverseRotation)
            {
                //Basically we do the same as body.bodyTransform.transform.Rotate but with the planetarium
                //as we are rotating WITH the planet and in the same reference plane
                Planetarium.Rotation = ApplySpaceRotation(Planetarium.Rotation, tilt);
            }
            else
            {
                body.rotation = ApplySpaceRotation(body.rotation, tilt);
                body.bodyTransform.transform.rotation = ApplySpaceRotation(body.bodyTransform.transform.rotation, tilt);
            }
        }

        private static Quaternion ApplySpaceRotation(Quaternion quat, Vector3 tilt) => quat * (Quaternion.Inverse(quat) * Quaternion.Euler(tilt)) * quat;

        private static void FillTiltDictionary()
        {
            try
            {
                var path = TiltEmUtil.CombinePaths(UrlDir.ApplicationRootPath, "GameData", "TiltEm", "PlanetTilt.cfg");
                if (File.Exists(path))
                {
                    var cfgNode = ConfigNode.Load(path);
                    foreach (var value in cfgNode.GetNodes()[0].values.Cast<ConfigNode.Value>())
                    {
                        var bodyIndex = int.Parse(value.name);
                        var tilt = float.Parse(value.value);
                        if (bodyIndex < FlightGlobals.Bodies.Count)
                        {
                            Debug.Log($"[TiltEm]: Celestialbody {FlightGlobals.Bodies[bodyIndex].bodyName} with index {bodyIndex} will have a tilt of {tilt}");
                            TiltDictionary.Add(bodyIndex, new Vector3(tilt, 0, 0));
                        }
                        else
                        {
                            Debug.LogError($"[TiltEm]: Celestialbody with index {bodyIndex} does not exist in FlightGlobals.Bodies");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[TiltEm]: No PlanetTilt.cfg found on path: {path}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TiltEm]: Error while reading PlanetTilt.cfg. Details: {e}");
            }
        }

#if DEBUG

        public void EnableToolBar()
        {
            var buttonTexture = GameDatabase.Instance.GetTexture("TiltEm/Button/TiltEmButton", false);
            GameEvents.onGUIApplicationLauncherReady.Remove(EnableToolBar);

            ApplicationLauncher.Instance.AddModApplication(() => TiltEmGui.Display = true, () => TiltEmGui.Display = false,
                () => { }, () => { }, () => { }, () => { }, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
        }
#endif

    }
}
