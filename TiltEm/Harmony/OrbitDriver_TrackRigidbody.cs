using System;
using System.Reflection;
using Harmony;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This harmony patch is intended to restore the planetarium or the planet tilt when changing the rotating frame
    /// </summary>
    [HarmonyPatch(typeof(OrbitDriver))]
    [HarmonyPatch("TrackRigidbody")]
    internal class OrbitDriver_TrackRigidbody
    {
        private static Vector3d Tilt;
        private static FieldInfo updateUT = typeof(OrbitDriver).GetField("updateUT", AccessTools.all);

        [HarmonyPrefix]
        private static bool PrefixTrackRigidbody(OrbitDriver __instance, CelestialBody refBody, double fdtOffset)
        {
            if (refBody.inverseRotation && TiltEm.TryGetTilt(__instance.vessel.mainBody.bodyName, out Tilt))
            {
                //TrackRigidbody(__instance, refBody, fdtOffset);
                //return false;
            }

            return true;
        }

        public static void TrackRigidbody(OrbitDriver drv, CelestialBody refBody, double fdtOffset)
        {
            updateUT.SetValue(drv, Planetarium.GetUniversalTime());
            if (drv.vessel != null)
            {
                var coMD = drv.vessel.CoMD - drv.referenceBody.position;
                drv.pos = coMD.xzy;
            }
            if (drv.vessel != null && drv.vessel.rootPart != null && drv.vessel.rootPart.rb != null && !drv.vessel.rootPart.rb.isKinematic)
            {
                updateUT.SetValue(drv, Planetarium.GetUniversalTime() + fdtOffset);
                drv.vel = drv.vessel.velocityD.xzy + drv.orbit.GetRotFrameVelAtPos(drv.referenceBody, drv.pos);
            }
            else if (drv.updateMode == OrbitDriver.UpdateMode.IDLE)
            {
                drv.vel = drv.orbit.GetRotFrameVel(drv.referenceBody);
            }
            if (refBody != drv.referenceBody)
            {
                if (drv.vessel != null)
                {
                    var vector3d = drv.vessel.CoMD - refBody.position;
                    drv.pos = vector3d.xzy;
                }
                var frameVel = drv;
                frameVel.vel = frameVel.vel + (drv.referenceBody.GetFrameVel() - refBody.GetFrameVel());
            }
            drv.lastTrackUT = (double)updateUT.GetValue(drv);

            UpdateFromStateVectors(drv.orbit, drv.pos, drv.vel, refBody, drv.lastTrackUT);

            drv.pos.Swizzle();
            drv.vel.Swizzle();
        }

        public static void UpdateFromStateVectors(Orbit obt, Vector3d pos, Vector3d vel, CelestialBody refBody, double UT)
        {
            obt.referenceBody = refBody;
            obt.h = Vector3d.Cross(pos, vel);
            if (obt.h.sqrMagnitude != 0)
            {
                obt.an = Vector3d.Cross(Vector3d.forward, obt.h);
                obt.OrbitFrame.Z = Planetarium.Zup.LocalToWorld(obt.h / obt.h.magnitude);
            }
            else
            {
                obt.inclination = Math.Acos(pos.z / pos.magnitude) * 57.2957795130823;
                obt.an = Vector3d.Cross(pos, Vector3d.forward);
            }
            if (obt.an.sqrMagnitude == 0)
            {
                obt.an = Vector3d.right;
            }
            obt.an = Planetarium.Zup.LocalToWorld(obt.an);
            obt.LAN = Math.Atan2(obt.an.y, obt.an.x) * 57.2957795130823;
            obt.LAN = (obt.LAN + 360) % 360;
            obt.eccVec = (Vector3d.Cross(vel, obt.h) / refBody.gravParameter) - (pos / pos.magnitude);
            obt.eccentricity = obt.eccVec.magnitude;
            obt.orbitalEnergy = vel.sqrMagnitude / 2 - refBody.gravParameter / pos.magnitude;
            obt.semiMajorAxis = (obt.eccentricity >= 1 ? -obt.semiLatusRectum / (obt.eccVec.sqrMagnitude - 1) : -refBody.gravParameter / (2 * obt.orbitalEnergy));
            if (obt.eccentricity != 0)
            {
                obt.OrbitFrame.X = Planetarium.Zup.LocalToWorld(obt.eccVec.normalized);
                obt.argumentOfPeriapsis = Orbit.SafeAcos(Vector3d.Dot(obt.an, obt.OrbitFrame.X) / obt.an.magnitude);
                if (obt.OrbitFrame.X.z < 0)
                {
                    obt.argumentOfPeriapsis = 6.28318530717959 - obt.argumentOfPeriapsis;
                }
            }
            else
            {
                obt.OrbitFrame.X = obt.an.normalized;
                obt.argumentOfPeriapsis = 0;
            }
            if (obt.h.sqrMagnitude != 0)
            {
                obt.OrbitFrame.Y = Vector3d.Cross(obt.OrbitFrame.Z, obt.OrbitFrame.X);
            }
            else
            {
                obt.OrbitFrame.Y = obt.an.normalized;
                obt.OrbitFrame.Z = Vector3d.Cross(obt.OrbitFrame.X, obt.OrbitFrame.Y);
            }

            //Tilt the orbit frame before using it
            TiltEmUtil.ApplyTiltToFrame(ref obt.OrbitFrame, -Tilt);

            obt.inclination = Math.Acos(obt.OrbitFrame.Z.z) * 57.2957795130823;
            obt.argumentOfPeriapsis *= 57.2957795130823;
            obt.meanMotion = obt.GetMeanMotion(obt.semiMajorAxis);
            var world = Planetarium.Zup.LocalToWorld(pos);
            var num = Vector3d.Dot(world, obt.OrbitFrame.X);
            var num1 = Vector3d.Dot(world, obt.OrbitFrame.Y);
            obt.trueAnomaly = Math.Atan2(num1, num);
            obt.eccentricAnomaly = obt.GetEccentricAnomaly(obt.trueAnomaly);
            obt.meanAnomaly = obt.GetMeanAnomaly(obt.eccentricAnomaly);
            obt.meanAnomalyAtEpoch = obt.meanAnomaly;
            obt.ObT = obt.meanAnomaly / obt.meanMotion;
            obt.ObTAtEpoch = obt.ObT;
            if (obt.eccentricity >= 1)
            {
                obt.period = double.PositiveInfinity;
                obt.orbitPercent = 0;
                obt.timeToPe = -obt.ObT;
                obt.timeToAp = double.PositiveInfinity;
            }
            else
            {
                obt.period = 6.28318530717959 / obt.meanMotion;
                obt.orbitPercent = obt.meanAnomaly / 6.28318530717959;
                obt.orbitPercent = (obt.orbitPercent + 1) % 1;
                obt.timeToPe = (obt.period - obt.ObT) % obt.period;
                obt.timeToAp = obt.timeToPe - obt.period / 2;
                if (obt.timeToAp < 0)
                {
                    obt.timeToAp += obt.period;
                }
            }
            obt.radius = pos.magnitude;
            obt.altitude = obt.radius - refBody.Radius;
            obt.epoch = UT;
            obt.pos = pos;
            obt.vel = vel;
            obt.debugPos = pos;
            obt.debugVel = vel;
            obt.debugH = obt.h;
            obt.debugAN = obt.an;
            obt.debugEccVec = obt.eccVec;
            obt.OrbitFrameX = obt.OrbitFrame.X;
            obt.OrbitFrameY = obt.OrbitFrame.Y;
            obt.OrbitFrameZ = obt.OrbitFrame.Z;
        }
    }
}
