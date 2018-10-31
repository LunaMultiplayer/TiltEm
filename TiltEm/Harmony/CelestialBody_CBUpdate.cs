using Harmony;
using UnityEngine;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This harmony patch is intended to update the body rotation after KSP has done it's thing
    /// </summary>
    [HarmonyPatch(typeof(CelestialBody))]
    [HarmonyPatch("CBUpdate")]
    internal class CelestialBody_CBUpdate
    {
        private static Vector3d Tilt;

        [HarmonyPrefix]
        private static bool PreFixCBUpdate(CelestialBody __instance)
        {
            if (!TiltEm.TryGetTilt(__instance.bodyName, out Tilt)) return true;

            CBUpdate(__instance);
            return false;
        }

        private static void CBUpdate(CelestialBody body)
        {
            body.gMagnitudeAtCenter = body.GeeASL * 9.80665 * body.Radius * body.Radius;
            body.Mass = body.Radius * body.Radius * (body.GeeASL * 9.80665) / 6.67408E-11;
            body.gravParameter = body.Mass * 6.67408E-11;

            if (body.rotates && body.rotationPeriod != 0 && (!body.tidallyLocked || body.orbit != null && body.orbit.period != 0))
            {
                if (body.tidallyLocked)
                {
                    body.rotationPeriod = body.orbit.period;
                }
                body.rotPeriodRecip = 1 / body.rotationPeriod;
                body.angularVelocity = Vector3d.down * (6.28318530717959 * body.rotPeriodRecip);
                body.zUpAngularVelocity = Vector3d.back * (6.28318530717959 * body.rotPeriodRecip);
                body.rotationAngle = (body.initialRotation + 360 * body.rotPeriodRecip * Planetarium.GetUniversalTime()) % 360;
                body.angularV = body.angularVelocity.magnitude;
                if (body.inverseRotation)
                {
                    Planetarium.InverseRotAngle = (body.rotationAngle - body.directRotAngle) % 360;
                    Planetarium.CelestialFrame.PlanetaryFrame(0, 90, Planetarium.InverseRotAngle, ref Planetarium.Zup);
                    
                    //Apply tilt
                    var rot = (QuaternionD)TiltEmUtil.ApplyWorldRotation(Planetarium.Zup.Rotation.swizzle, Tilt);
                    rot.swizzle.FrameVectors(out Planetarium.Zup.X, out Planetarium.Zup.Y, out Planetarium.Zup.Z);

                    var quaternionD = QuaternionD.Inverse(Planetarium.Zup.Rotation);
                    Planetarium.Rotation = quaternionD.swizzle;
                }
                else
                {
                    body.directRotAngle = (body.rotationAngle - Planetarium.InverseRotAngle) % 360;
                    Planetarium.CelestialFrame.PlanetaryFrame(0, 90, body.directRotAngle, ref body.BodyFrame);

                    //Apply tilt
                    var rot = (QuaternionD)TiltEmUtil.ApplyWorldRotation(body.BodyFrame.Rotation.swizzle, Tilt);
                    rot.swizzle.FrameVectors(out body.BodyFrame.X, out body.BodyFrame.Y, out body.BodyFrame.Z);

                    body.rotation = body.BodyFrame.Rotation.swizzle;
                    body.bodyTransform.rotation = body.rotation;
                }
            }
            if (body.orbitDriver)
            {
                body.orbitDriver.UpdateOrbit(true);
            }

            var celestialBody = body;
            var sun = (Planetarium.fetch == null ? FlightGlobals.Bodies[0] : Planetarium.fetch.Sun);
            while (celestialBody.referenceBody != sun && celestialBody.referenceBody != null)
            {
                celestialBody = celestialBody.referenceBody;
            }

            if (celestialBody.orbit == null)
            {
                body.solarDayLength = 1;
            }
            else
            {
                var num = celestialBody.orbit.period - body.rotationPeriod;
                body.solarDayLength = num != 0 ? celestialBody.orbit.period * body.rotationPeriod / num : double.MaxValue;
            }


        }
    }
}
