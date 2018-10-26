using Kopernicus;
using TiltEmCommon;
using UnityEngine;

namespace TiltEmKopernicus
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class TiltEm : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);
            Debug.Log("[TiltEm]: TiltEmKopernicus started!");

            foreach (var body in FlightGlobals.Bodies)
            {
                if (body.Has("tiltx") || body.Has("tiltz"))
                {
                    TiltEmShared.AddTiltData(body, new Vector3d(body.Get("tiltx", 0d), 0, body.Get("tiltz", 0d)));
                }
            }
        }
    }
}
