using UnityEngine;

namespace TiltEmCommon
{
    [KSPAddon(KSPAddon.Startup.TrackingStation, true)]
    public class TiltAxisRenderer : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(this);

            GameEvents.onPlanetariumTargetChanged.Add(OnPlanetariumTargetChange);
            GameEvents.onGameSceneSwitchRequested.Add(OnSceneRequested);
        }

        public void OnPlanetariumTargetChange(MapObject data)
        {
            if (data.celestialBody != null)
            {
                data.celestialBody.scaledBody.AddComponent<LineRenderer>().positionCount = 3;
            }
        }

        public void OnSceneRequested(GameEvents.FromToAction<GameScenes, GameScenes> data)
        {
            if (data.from == GameScenes.TRACKSTATION && data.to != GameScenes.TRACKSTATION && PlanetariumCamera.fetch.target.celestialBody)
            {
                Destroy(PlanetariumCamera.fetch.target.celestialBody.scaledBody.GetComponent<LineRenderer>());
            }
        }

        public void Update()
        {
            if (HighLogic.LoadedScene != GameScenes.TRACKSTATION || PlanetariumCamera.fetch.target.celestialBody == null) return;

            var body = PlanetariumCamera.fetch.target.celestialBody;
            var scale = (int)body.Radius;

            var lineRenderer = body.scaledBody.GetComponent<LineRenderer>();

            if (lineRenderer == null) return;

            lineRenderer.SetPosition(0, -1 * (body.scaledBody.transform.up * scale));
            lineRenderer.SetPosition(1, body.scaledBody.transform.position);
            lineRenderer.SetPosition(2, body.scaledBody.transform.up * scale);
        }
    }
}
