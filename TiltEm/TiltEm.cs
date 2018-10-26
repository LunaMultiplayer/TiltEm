using KSP.UI.Screens;
using System;
using TiltEmCommon;
using UnityEngine;

namespace TiltEm
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class TiltEm : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);
            Debug.Log("[TiltEm]: TiltEm started!");

            FillTiltDictionary();
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

        private static void FillTiltDictionary()
        {
            try
            {
                foreach (var urlCfg in GameDatabase.Instance.GetConfigs("PlanetTilts"))
                {
                    foreach (var node in urlCfg.config.GetNodes())
                    { 
                        var body = FlightGlobals.GetBodyByName(node.name);
                        if (body != null)
                        {
                            var tiltX = float.Parse(node.GetValue("TiltX"));
                            var tiltZ = float.Parse(node.GetValue("TiltZ"));
                            var vector = new Vector3(tiltX, 0, tiltZ);

                            Debug.Log($"[TiltEm]: Celestialbody {body.bodyName} will have a tilt of {vector.magnitude}");
                            TiltEmShared.AddTiltData(body, vector);
                        }
                        else
                        {
                            Debug.LogError($"[TiltEm]: Celestialbody with name {node.name} does not exist!");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TiltEm]: Error while reading PlanetTilts config node. Details: {e}");
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
