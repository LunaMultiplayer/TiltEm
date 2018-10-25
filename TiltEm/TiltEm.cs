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
        public static Dictionary<int, float> TiltDictionary = new Dictionary<int, float>();

        public static bool RotatingFrame => FlightGlobals.currentMainBody != null && FlightGlobals.currentMainBody.inverseRotation;

        public void Awake()
        {
            DontDestroyOnLoad(this);
            Debug.Log("[TiltEm]: TiltEm started!");

            FillTiltDictionary();
            if (TiltDictionary.Any())
            {
                HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
            GameEvents.onGUIApplicationLauncherReady.Add(EnableToolBar);
        }

        // ReSharper disable once InconsistentNaming
        public void OnGUI()
        {
            TiltEmGui.SetStyles();
            TiltEmGui.CheckWindowLock();
            TiltEmGui.DrawGui();
        }


        public static void CelestialBodyAwake(CelestialBody body)
        {
            if (!TiltDictionary.ContainsKey(body.flightGlobalsIndex) || body.orbit == null) return;

            body.bodyTransform.transform.Rotate(new Vector3(TiltDictionary[body.flightGlobalsIndex], 0, 0), Space.World);
        }

        public static void CelestialBodyUpdate(CelestialBody body)
        {
            if (!TiltDictionary.ContainsKey(body.flightGlobalsIndex) || body.orbit == null) return;

            var tilt = TiltDictionary[body.flightGlobalsIndex];

            if (body.inverseRotation)
            {
                Planetarium.Rotation = (Quaternion)Planetarium.Rotation * (Quaternion.Inverse(Planetarium.Rotation) * Quaternion.Euler(tilt, 0, 0)) * (Quaternion)Planetarium.Rotation;
            }
            else
            {
                body.bodyTransform.transform.Rotate(new Vector3(tilt, 0, 0), Space.World);
            }
        }
        
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
                        TiltDictionary.Add(int.Parse(value.name), float.Parse(value.value));
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

        public void EnableToolBar()
        {
            var buttonTexture = GameDatabase.Instance.GetTexture("TiltEm/Button/TiltEmButton", false);
            GameEvents.onGUIApplicationLauncherReady.Remove(EnableToolBar);
            ApplicationLauncher.Instance.AddModApplication(() => TiltEmGui.Display = true, () => TiltEmGui.Display = false,
                () => { }, () => { }, () => { }, () => { }, ApplicationLauncher.AppScenes.ALWAYS, buttonTexture);
        }
    }
}
