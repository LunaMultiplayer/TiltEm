using Harmony;
using System.Text;
using UnityEngine;

#if DEBUG
namespace TiltEm
{
    internal class TiltEmGui
    {
        private static OrbitPhysicsManager ObtPhysMgr => Traverse.Create<OrbitPhysicsManager>().Field<OrbitPhysicsManager>("fetch").Value;
        
        public static bool Display
        {
            get => _display && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => _display = value;
        }
        private static bool _display;

        private static bool _initialized;

        private static Rect _windowRect;
        private static Rect _moveRect;
        private const float WindowHeight = 150;
        private const float WindowWidth = 480;
        private static GUILayoutOption[] _layoutOptions;
        private static bool _isWindowLocked;

        private static readonly StringBuilder Builder = new StringBuilder();

        public static void DrawGui()
        {
            if (!Display) return;

            _windowRect = FixWindowPos(GUILayout.Window(6984624, _windowRect, DrawContent, "Tilt", _layoutOptions));
        }

        public static void CheckWindowLock()
        {
            if (Display)
            {
                Vector2 mousePos = Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;

                var shouldLock = _windowRect.Contains(mousePos);

                if (shouldLock && !_isWindowLocked)
                {
                    InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "TiltItLock");
                    _isWindowLocked = true;
                }
                if (!shouldLock && _isWindowLocked)
                    RemoveWindowLock();
            }

            if (!Display && _isWindowLocked)
                RemoveWindowLock();
        }

        public static void SetStyles()
        {
            if (_initialized) return;

            _windowRect = new Rect(Screen.width - (WindowWidth + 50), Screen.height / 2f - WindowHeight / 2f, WindowWidth, WindowHeight);
            _moveRect = new Rect(0, 0, int.MaxValue, 20);

            _layoutOptions = new GUILayoutOption[4];
            _layoutOptions[0] = GUILayout.MinWidth(WindowWidth);
            _layoutOptions[1] = GUILayout.MaxWidth(WindowWidth);
            _layoutOptions[2] = GUILayout.MinHeight(WindowHeight);
            _layoutOptions[3] = GUILayout.MaxHeight(WindowHeight);

            _initialized = true;
        }

        private static void DrawContent(int windowId)
        {
            GUI.DragWindow(_moveRect);
            GUILayout.BeginVertical();

            DrawDebugAndActionButtons();
            GUILayout.Space(20);
            DrawRotatingFrameButtons();

            FillBuilderWithVesselData();

            GUILayout.Space(20);
            GUILayout.Label(Builder.ToString());

            GUILayout.Space(20);
            GUILayout.Label($"Planetarium Rot: {((Quaternion)Planetarium.Rotation).eulerAngles} - Frm: {((Quaternion)Planetarium.Zup.Rotation).eulerAngles}");
            GUILayout.Space(20);

            FillBuilderWithPlanetsData();

            GUILayout.Label(Builder.ToString());

            GUILayout.EndVertical();
        }

        private static void FillBuilderWithPlanetsData()
        {
            Builder.Length = 0;
            foreach (var body in FlightGlobals.Bodies)
            {
                Builder.AppendLine($"{body.bodyName}: T: {TiltEm.GetTiltForDisplay(body.bodyName)}° " +
                                   $"- Rot: {((Quaternion) body.rotation).eulerAngles} " +
                                   $"- Frm: {((Quaternion) body.BodyFrame.Rotation).eulerAngles}");
            }
        }

        private static void FillBuilderWithVesselData()
        {
            Builder.Length = 0;
            Builder.AppendLine($"Vessel obt mode: {(FlightGlobals.ActiveVessel != null ? FlightGlobals.ActiveVessel.orbitDriver.updateMode.ToString() : string.Empty)}");
            Builder.AppendLine($"Vessel obt transform rot: {(FlightGlobals.ActiveVessel != null ? FlightGlobals.ActiveVessel.orbitDriver.driverTransform.rotation.eulerAngles : Vector3.zero)}");
            Builder.AppendLine($"Vessel obt frm: {(FlightGlobals.ActiveVessel != null ? ((Quaternion) FlightGlobals.ActiveVessel.orbit.OrbitFrame.Rotation).eulerAngles : Vector3.zero)}");
            Builder.AppendLine($"Vessel rot: {(FlightGlobals.ActiveVessel != null ? FlightGlobals.ActiveVessel.vesselTransform.rotation.eulerAngles : Vector3.zero)}");
            Builder.AppendLine($"Vessel pos: {(FlightGlobals.ActiveVessel != null ? FlightGlobals.ActiveVessel.vesselTransform.position : Vector3.zero)}");
        }

        private static void DrawRotatingFrameButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Toggle rotating frame"))
            {
                ObtPhysMgr.ToggleRotatingFrame();
            }

            if (GUILayout.Button("Reset"))
            {
                ObtPhysMgr.degub = false;
            }

            GUILayout.EndHorizontal();
        }

        private static void DrawDebugAndActionButtons()
        {
            GUILayout.BeginHorizontal();
            for (var i = 0; i < TiltEm.DebugSwitches.Length; i++)
            {
                TiltEm.DebugSwitches[i] = GUILayout.Toggle(TiltEm.DebugSwitches[i], $"D{i}");
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            for (var i = 0; i < TiltEm.DebugSwitches.Length; i++)
            {
                if (GUILayout.Button($"A{i}"))
                    TiltEm.DebugActions[i].Invoke();
            }
            GUILayout.EndHorizontal();
        }

        private static void RemoveWindowLock()
        {
            if (!_isWindowLocked) return;

            _isWindowLocked = false;
            InputLockManager.RemoveControlLock("TiltItLock");
        }

        /// <summary>
        /// Call this method to prevent the window going offscreen
        /// </summary>
        private static Rect FixWindowPos(Rect inputRect)
        {
            //Let the user drag 3/4 of the window sideways off the screen
            var xMin = 0 - 3 / 4f * inputRect.width;
            var xMax = Screen.width - 1 / 4f * inputRect.width;

            //Don't let the title bar move above the top of the screen
            var yMin = 0;
            //Don't let the title bar move below the bottom of the screen
            float yMax = Screen.height - 20;

            if (inputRect.x < xMin)
                inputRect.x = xMin;
            if (inputRect.x > xMax)
                inputRect.x = xMax;
            if (inputRect.y < yMin)
                inputRect.y = yMin;
            if (inputRect.y > yMax)
                inputRect.y = yMax;

            return inputRect;
        }
    }
}
#endif