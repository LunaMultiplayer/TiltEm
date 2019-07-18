using Harmony;
using System;
using UnityEngine;

// ReSharper disable All

namespace TiltEm.Harmony
{
    /// <summary>
    /// This is the main harmony patch. Here we tilt the planets or the planetarium depending if we are in inverse rotation or not.
    /// Being in inverse rotation means that the planet tilt is set to 0 and the planets MOVE AROUND the planet you're orbiting, like Ptolemy tought the universe worked
    /// This is a core part of KSP, you CANNOT land into a planet if the planet rotates! Therefore Squad came with this crazy solution of making the planet static and move everything around.
    /// Once you're avobe a threshold altitude (100K on Kerbin) or in the track station you switch to NON inverse rotation mode, and everything is normal, planets move around the sun, etc...
    /// </summary>
    [HarmonyPatch(typeof(CelestialBody))]
    [HarmonyPatch("CBUpdate")]
    internal class CelestialBody_CBUpdate
    {
        [HarmonyPrefix]
        private static bool PrefixCBUpdate(CelestialBody __instance)
        {
            //Only do our stuff if we have a tilt for the planet
            if (TiltEm.TryGetTilt(__instance.bodyName, out var tilt))
            {
                CBUpdate(__instance, tilt);
                return false;
            }

            return true;
        }

        private static void CBUpdate(CelestialBody body, Vector3d tilt)
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

                //Do not touch those values. They will just mess up with the orbital and frame calculations, specifically while in inverse rotation
                //Also this mod is just cosmetic so better don't mess up with the numbers...
                //body.angularVelocity = QuaternionD.Inverse(Quaternion.Euler(tilt)) * Vector3d.down * (Math.PI * 2 * body.rotPeriodRecip);
                //body.zUpAngularVelocity = (QuaternionD)Quaternion.Euler(tilt) * Vector3d.back * (Math.PI * 2 * body.rotPeriodRecip);
                body.angularVelocity = Vector3d.down * (Math.PI * 2 * body.rotPeriodRecip);
                body.zUpAngularVelocity = Vector3d.back * (Math.PI * 2 * body.rotPeriodRecip);

                body.rotationAngle = (body.initialRotation + 360 * body.rotPeriodRecip * Planetarium.GetUniversalTime()) % 360;
                body.angularV = body.angularVelocity.magnitude;
                if (body.inverseRotation)
                {
                    Planetarium.InverseRotAngle = (body.rotationAngle - body.directRotAngle) % 360;

                    //Apply tilt to the planetarium. In this mode, the planet is NOT tilted. What we do is tilt the planetarium.
                    ////Don't worry about tilting the planet to 0 degrees. This is done in the "onRotatingFrameTransition" event of TiltEm class 
                    Planetarium.CelestialFrame.PlanetaryFrame(tilt.z + Planetarium.InverseRotAngle, 90 + tilt.x, 0, ref Planetarium.Zup);

                    var quaternionD = QuaternionD.Inverse(Planetarium.Zup.Rotation);
                    Planetarium.Rotation = quaternionD.swizzle;
                }
                else
                {
                    body.directRotAngle = (body.rotationAngle - Planetarium.InverseRotAngle) % 360;

                    //Apply tilt to the planet. In this mode, the planetarium is NOT tilted. What we do is tilt the planet
                    //Don't worry about tilting the planetarium to 0 degrees. This is done in the "onRotatingFrameTransition" event of TiltEm class 
                    Planetarium.CelestialFrame.PlanetaryFrame(tilt.z, 90 + tilt.x, body.directRotAngle, ref body.BodyFrame);

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
