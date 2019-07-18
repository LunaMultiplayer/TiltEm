using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
// ReSharper disable All

#if DEBUG
namespace TiltEm
{
    /// <summary>
    /// This class just render some lines on the planets while in tracking station. Only useful for debugging purposes
    /// </summary>
    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class TiltAxisRenderer : MonoBehaviour
    {
        private static VectorLine _lineUpDown;
        private static VectorLine _lineLeftRight;
        private static VectorLine _lineFwdBack;

        private static readonly List<Vector3> PointsUpDownAxis = new List<Vector3> { new Vector3(), new Vector3() };
        private static readonly List<Vector3> PointsLeftRightAxis = new List<Vector3> { new Vector3(), new Vector3() };
        private static readonly List<Vector3> PointsFwdBackAxis = new List<Vector3> { new Vector3(), new Vector3() };

        public void OnDisable()
        {
            if (_lineUpDown != null)
                VectorLine.Destroy(ref _lineUpDown);
            if (_lineLeftRight != null)
                VectorLine.Destroy(ref _lineLeftRight);
            if (_lineFwdBack != null)
                VectorLine.Destroy(ref _lineFwdBack);
        }

        public void Update()
        {
            if (_lineUpDown != null)
                VectorLine.Destroy(ref _lineUpDown);
            if (_lineLeftRight != null)
                VectorLine.Destroy(ref _lineLeftRight);
            if (_lineFwdBack != null)
                VectorLine.Destroy(ref _lineFwdBack);

            if (HighLogic.LoadedScene != GameScenes.TRACKSTATION || PlanetariumCamera.fetch.target.celestialBody == null) return;

            var body = PlanetariumCamera.fetch.target.celestialBody;

            var scale = (float)(body.Radius * 0.0025);

            PointsUpDownAxis[0] = body.scaledBody.transform.position + body.scaledBody.transform.up * scale; //top
            PointsUpDownAxis[1] = body.scaledBody.transform.position + (-1 * body.scaledBody.transform.up) * scale; //bottom

            _lineUpDown = new VectorLine("AxialTiltUpDown", PointsUpDownAxis, 2f, LineType.Continuous);
            _lineUpDown.rectTransform.gameObject.layer = 31;
            _lineUpDown.color = Color.blue;
            _lineUpDown.smoothColor = true;
            _lineUpDown.UpdateImmediate = true;

            _lineUpDown.active = true;
            _lineUpDown.Draw3D();

            PointsLeftRightAxis[0] = body.scaledBody.transform.position + body.scaledBody.transform.right * scale; //right
            PointsLeftRightAxis[1] = body.scaledBody.transform.position + (-1 * body.scaledBody.transform.right) * scale; //left

            _lineLeftRight = new VectorLine("AxialTiltLeftRight", PointsLeftRightAxis, 2f, LineType.Continuous);
            _lineLeftRight.rectTransform.gameObject.layer = 31;
            _lineLeftRight.color = Color.red;
            _lineLeftRight.smoothColor = true;
            _lineLeftRight.UpdateImmediate = true;

            _lineLeftRight.active = true;
            _lineLeftRight.Draw3D();

            PointsFwdBackAxis[0] = body.scaledBody.transform.position + body.scaledBody.transform.forward * scale; //forward
            PointsFwdBackAxis[1] = body.scaledBody.transform.position + (-1 * body.scaledBody.transform.forward) * scale; //back

            _lineFwdBack = new VectorLine("AxialTiltFwdBack", PointsFwdBackAxis, 2f, LineType.Continuous);
            _lineFwdBack.rectTransform.gameObject.layer = 31;
            _lineFwdBack.color = Color.green;
            _lineFwdBack.smoothColor = true;
            _lineFwdBack.UpdateImmediate = true;

            _lineFwdBack.active = true;
            _lineFwdBack.Draw3D();

        }
    }
}
#endif