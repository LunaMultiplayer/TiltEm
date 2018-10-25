using Harmony;
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

            body.bodyTransform.transform.Rotate(new Vector3(tilt, 0, 0), Space.World);
        }

        public static void CelestialBodyUpdate(CelestialBody body)
        {
            if (!TiltDictionary.TryGetValue(body.flightGlobalsIndex, out var tilt)) return;

            if (body.inverseRotation)
            {
                //Basically we do the same as body.bodyTransform.transform.Rotate but with the planetarium
                //as we are rotating WITH the planet and in the same reference plane
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
                        var bodyIndex = int.Parse(value.name);
                        var tilt = float.Parse(value.value);
                        if (FlightGlobals.Bodies.Count < bodyIndex)
                        {
                            Debug.Log($"[TiltEm]: Celestialbody {FlightGlobals.Bodies[bodyIndex].bodyName} with index {bodyIndex} will have a tilt of {tilt}");
                            TiltDictionary.Add(bodyIndex, tilt);
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
