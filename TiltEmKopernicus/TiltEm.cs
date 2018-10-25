using Harmony;
using Kopernicus;
using System.Reflection;
using UnityEngine;

namespace TiltEmKopernicus
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class TiltEm : MonoBehaviour
    {
        public static HarmonyInstance HarmonyInstance = HarmonyInstance.Create("TiltEm");

        public void Awake()
        {
            DontDestroyOnLoad(this);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void CelestialBodyAwake(CelestialBody body)
        {
            if (!body.Has("tilt")) return;

            body.bodyTransform.transform.Rotate(new Vector3(body.Get("tilt", 0f), 0, 0), Space.World);
        }

        public static void CelestialBodyUpdate(CelestialBody body)
        {
            //Instead of a harmony patch, you can do it in the TimingManager.FixedUpdateAdd(TimingManager.TimingStage.Precalc)
            //That would involve running trough all the celestial bodies with a loop tough...

            if (!body.Has("tilt")) return;

            var tilt = new Vector3(body.Get("tilt", 0f), 0, 0);

            if (body.inverseRotation)
            {
                //Basically we do the same as body.bodyTransform.transform.Rotate but with the planetarium
                //as we are rotating WITH the planet and in the same reference plane
                Planetarium.Rotation = ApplySpaceRotation(Planetarium.Rotation, tilt);
            }
            else
            {
                body.rotation = ApplySpaceRotation(body.rotation, tilt);
                body.bodyTransform.transform.rotation = body.rotation;

                //We must fix the bodyFrame vectors as otherwise landed vessels will not take the axial tilt on track station
                body.rotation.swizzle.FrameVectors(out body.BodyFrame.X, out body.BodyFrame.Y, out body.BodyFrame.Z);
            }
        }

        private static Quaternion ApplySpaceRotation(Quaternion quat, Vector3 tilt) => quat * (Quaternion.Inverse(quat) * Quaternion.Euler(tilt)) * quat;
    }
}
