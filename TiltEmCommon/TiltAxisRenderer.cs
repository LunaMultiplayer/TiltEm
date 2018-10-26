using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

namespace TiltEmCommon
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, true)]
    public class TiltAxisRenderer : MonoBehaviour
    {
        private static CelestialBody _targetBody;
        private static VectorLine _line;

        private static readonly List<Vector3> Points = new List<Vector3> { new Vector3(), new Vector3() };

        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public void Update()
        {
            if (HighLogic.LoadedScene != GameScenes.TRACKSTATION || PlanetariumCamera.fetch.target.celestialBody == null) return;

            var body = PlanetariumCamera.fetch.target.celestialBody;

            if (_targetBody != body)
                VectorLine.Destroy(ref _line);

            _targetBody = body;

            var scale = (float)(body.Radius * 0.00025);

            Points[0] = body.scaledBody.transform.position + body.scaledBody.transform.up * scale; //top
            Points[1] = body.scaledBody.transform.position + (-1 * body.scaledBody.transform.up) * scale; //bottom

            _line = new VectorLine("AxialTilt", Points, 2f, LineType.Continuous);
            _line.rectTransform.gameObject.layer = 31;
            _line.color = Color.blue;
            _line.smoothColor = true;
            _line.UpdateImmediate = true;

            _line.active = true;
            _line.Draw3D();
        }
    }
}
