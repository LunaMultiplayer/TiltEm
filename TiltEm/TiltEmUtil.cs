using UnityEngine;

namespace TiltEm
{
    public class TiltEmUtil
    {
        public static bool PlanetariumTilted { get; set; }

        /// <summary>
        /// Does the same as Transform.Rotate but against a given quaternion and against WORLD space
        /// </summary>
        /// <param name="quaternion">Quaternion to apply the rotation to</param>
        /// <param name="tilt">Rotation to apply</param>
        /// <returns>Rotated Quaternion</returns>
        public static Quaternion ApplyWorldRotation(Quaternion quaternion, Vector3 tilt) => quaternion * (Quaternion.Inverse(quaternion) * Quaternion.Euler(tilt)) * quaternion;

        /// <summary>
        /// Does the same as Transform.Rotate but against a given quaternion and against LOCAL space
        /// </summary>
        /// <param name="quaternion">Quaternion to apply the rotation to</param>
        /// <param name="tilt">Rotation to apply</param>
        /// <returns>Rotated Quaternion</returns>
        public static Quaternion ApplyLocalRotation(Quaternion quaternion, Vector3 tilt) => quaternion * Quaternion.Euler(tilt);

        /// <summary>
        /// Removes the tilt from the given planet and sets it back as default
        /// </summary>
        public static void RestorePlanetTilt(CelestialBody body)
        {
            body.directRotAngle = (body.rotationAngle - Planetarium.InverseRotAngle) % 360;
            Planetarium.CelestialFrame.PlanetaryFrame(0, 90, body.directRotAngle, ref body.BodyFrame);
            body.rotation = body.BodyFrame.Rotation.swizzle;
            body.bodyTransform.rotation = body.rotation;
        }

        /// <summary>
        /// Removes the tilt from the planetarium and sets it back as default
        /// </summary>
        public static void RestorePlanetariumTilt()
        {
            Planetarium.CelestialFrame.PlanetaryFrame(0, 90, Planetarium.InverseRotAngle, ref Planetarium.Zup);
            var quaternionD = QuaternionD.Inverse(Planetarium.Zup.Rotation);
            Planetarium.Rotation = quaternionD.swizzle;

            PlanetariumTilted = false;
        }

        /// <summary>
        /// Tilts given planet frames
        /// </summary>
        public static void ApplyPlanetTilt(CelestialBody body, Vector3d tilt)
        {
            if (body.inverseRotation) return;

            ApplyTiltToFrame(ref body.BodyFrame, tilt);
        }

        /// <summary>
        /// Tilts the planetarium. Useful when you're in inverse rotation mode 
        /// </summary>
        public static void ApplyPlanetariumTilt(CelestialBody body, Vector3d tilt)
        {
            if (!body.inverseRotation) return;

            ApplyTiltToFrame(ref Planetarium.Zup, tilt);
            PlanetariumTilted = true;
        }

        /// <summary>
        /// Applies tilt to the given celestial frame
        /// </summary>
        public static void ApplyTiltToFrame(ref Planetarium.CelestialFrame frame, Vector3d tilt)
        {
            var rot = (QuaternionD)ApplyWorldRotation(frame.Rotation.swizzle, tilt);
            rot.swizzle.FrameVectors(out frame.X, out frame.Y, out frame.Z);
        }

        /// <summary>
        /// Applies tilt to the given celestial frame
        /// </summary>
        public static void RemoveTiltToFrame(ref Planetarium.CelestialFrame frame, Vector3d tilt)
        {
            var rot = (QuaternionD)ApplyWorldRotation(frame.Rotation.swizzle, -tilt);
            rot.swizzle.FrameVectors(out frame.X, out frame.Y, out frame.Z);
        }
    }
}
