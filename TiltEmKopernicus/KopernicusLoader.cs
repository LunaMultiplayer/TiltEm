using Kopernicus;
using UnityEngine;

namespace TiltEmKopernicus
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class KopernicusLoader : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);
            Debug.Log("[TiltEm]: TiltEmKopernicus started!");

            foreach (var body in FlightGlobals.Bodies)
            {
                if (body.Has("tiltx") || body.Has("tiltz"))
                {
                    TiltEm.TiltEm.AddTiltData(body, new Vector3d(body.Get("tiltx", 0d), 0, body.Get("tiltz", 0d)));
                }
            }
        }
    }
}
