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

        private static bool _displayVesselData;
        private static bool _displayTilts;

        private static Rect _windowRect;
        private static Rect _moveRect;
        private const float WindowHeight = 150;
        private const float WindowWidth = 580;
        private static GUILayoutOption[] _layoutOptions;
        private static GUIStyle _horizontalLine;
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

            _horizontalLine = new GUIStyle
            {
                normal = { background = Texture2D.whiteTexture },
                margin = new RectOffset(0, 0, 4, 4),
                fixedHeight = 1
            };

            _initialized = true;
        }

        private static void DrawEditButtons(ref double value)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-1"))
                value -= 1;
            if (GUILayout.Button("-0.5"))
                value -= 0.5;
            if (GUILayout.Button("-0.1"))
                value -= 0.1;
            if (GUILayout.Button("-0.01"))
                value -= 0.01;
            if (GUILayout.Button("0"))
                value = 0;
            if (GUILayout.Button("+0.01"))
                value += 0.01;
            if (GUILayout.Button("+0.1"))
                value += 0.1;
            if (GUILayout.Button("+0.5"))
                value += 0.5;
            if (GUILayout.Button("+1"))
                value += 1;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"{value:F2}", GUILayout.Width(55.0f));
            value = GUILayout.HorizontalScrollbar((float)value, 0, 0, 360);
            GUILayout.EndHorizontal();
            DrawHorizontalLine(Color.white);
        }

        private static void DrawContent(int windowId)
        {
            GUI.DragWindow(_moveRect);
            GUILayout.BeginVertical();

            DrawDebugAndActionButtons();
            DrawHorizontalLine(Color.white);

            DrawRotatingFrameButtons();
            DrawHorizontalLine(Color.white);

            _displayVesselData = GUILayout.Toggle(_displayVesselData, "Display Vessel data");
            if (_displayVesselData)
            {
                GUILayout.Label(GetVesselData());
            }

            _displayTilts = GUILayout.Toggle(_displayTilts, "Display Tilts");
            if (_displayTilts)
            {
                GUILayout.Label(GetTilts());
            }

            GUILayout.EndVertical();
        }

        private static string GetTilts()
        {
            Builder.Length = 0;

            Builder.AppendLine($"Planetarium Rot: {((Quaternion)Planetarium.Rotation).eulerAngles} - Frm: {((Quaternion)Planetarium.Zup.Rotation).eulerAngles} " +
                               $"- Inverse rot°: {Planetarium.InverseRotAngle:F2}");
            Builder.AppendLine(string.Empty);
            for (var i = 0; i < FlightGlobals.Bodies.Count; i++)
            {
                var body = FlightGlobals.Bodies[i];

                if (i == FlightGlobals.Bodies.Count - 1)
                {
                    Builder.Append($"{body.bodyName}: T: {TiltEm.GetTiltForDisplay(body.bodyName)}° " +
                                       $"- Rot: {((Quaternion)body.transform.rotation).eulerAngles} " +
                                       $"- Frm: {((Quaternion)body.BodyFrame.Rotation).eulerAngles} " +
                                       $"- Direct rot°: {body.rotationAngle:F2}");
                }
                else
                {
                    Builder.AppendLine($"{body.bodyName}: T: {TiltEm.GetTiltForDisplay(body.bodyName)}° " +
                                       $"- Rot: {((Quaternion)body.transform.rotation).eulerAngles} " +
                                       $"- Frm: {((Quaternion)body.BodyFrame.Rotation).eulerAngles} " +
                                       $"- Direct rot°: {body.rotationAngle:F2}");
                }
            }

            return Builder.ToString();
        }

        private static string GetVesselData()
        {
            Builder.Length = 0;
            Builder.Append("Vessel obt mode: ");
            Builder.AppendLine(FlightGlobals.ActiveVessel != null ? FlightGlobals.ActiveVessel.orbitDriver.updateMode.ToString() : string.Empty);

            Builder.Append("Vessel obt transform rot: ");
            Builder.AppendLine((FlightGlobals.ActiveVessel != null ? FlightGlobals.ActiveVessel.orbitDriver.driverTransform.rotation.eulerAngles : Vector3.zero).ToString());

            Builder.Append("Vessel obt frm: ");
            Builder.AppendLine((FlightGlobals.ActiveVessel != null ? ((Quaternion)FlightGlobals.ActiveVessel.orbit.OrbitFrame.Rotation).eulerAngles : Vector3.zero).ToString());

            Builder.Append("Vessel rot: ");
            Builder.AppendLine((FlightGlobals.ActiveVessel != null ? FlightGlobals.ActiveVessel.vesselTransform.rotation.eulerAngles : Vector3.zero).ToString());

            Builder.Append("Vessel pos: ");
            Builder.Append((FlightGlobals.ActiveVessel != null ? FlightGlobals.ActiveVessel.vesselTransform.position : Vector3.zero).ToString());

            return Builder.ToString();
        }

        private static void DrawRotatingFrameButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Toggle rotating frame"))
            {
                if (OrbitPhysicsManager.DominantBody != null)
                {
                    Debug.Log(OrbitPhysicsManager.DominantBody.inverseRotation
                        ? $"Setting NORMAL rotation t:{Planetarium.GetUniversalTime()}"
                        : $"Setting INVERSE rotation t:{Planetarium.GetUniversalTime()}");
                }

                ObtPhysMgr.ToggleRotatingFrame();
            }

            GUILayout.Label($"Rotated planetarium: {(OrbitPhysicsManager.DominantBody != null ? OrbitPhysicsManager.DominantBody.inverseRotation.ToString() : "?")}");
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Reset"))
            {
                ObtPhysMgr.degub = false;
            }
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

        /// <summary>
        /// Draws an horizontal separator line
        /// </summary>
        private static void DrawHorizontalLine(Color color)
        {
            GUILayout.Space(10);

            var aux = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, _horizontalLine);
            GUI.color = aux;

            GUILayout.Space(10);
        }
    }
}
#endif