using UnityEngine;

namespace TiltEm
{
    public class TiltEmGui
    {
        public static bool Display
        {
            get => _display && HighLogic.LoadedScene >= GameScenes.SPACECENTER;
            set => _display = value;
        }
        private static bool _display;

        private static bool _initialized;

        private static Rect _windowRect;
        private static Rect _moveRect;
        private const float WindowHeight = 100;
        private const float WindowWidth = 300;
        private static GUILayoutOption[] _layoutOptions;
        private static bool _isWindowLocked;

        public static void DrawGui()
        {
            if (!Display) return;

            _windowRect = FixWindowPos(GUILayout.Window(6984624, _windowRect, DrawContent, "Tilt Kerbin", _layoutOptions));
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

            _windowRect = new Rect(Screen.width - (WindowWidth + 50), Screen.height / 2f - WindowHeight / 2f, WindowWidth,
                WindowHeight);
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

            GUILayout.Label($"Planetarium Rotation: {((Quaternion)Planetarium.Rotation).eulerAngles}");
            GUILayout.Label($"Kerbin Rotation: {TiltEm.Kerbin.bodyTransform.rotation.eulerAngles}");
            GUILayout.Label($"Current tilt: {TiltEm.Tilt}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Tilt + 10"))
                TiltEm.Tilt += 10;
            if (GUILayout.Button("Tilt - 10"))
                TiltEm.Tilt -= 10;
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Reset"))
                TiltEm.Tilt = 0;

            GUILayout.EndVertical();
        }

        private static void RemoveWindowLock()
        {
            if (_isWindowLocked)
            {
                _isWindowLocked = false;
                InputLockManager.RemoveControlLock("TiltItLock");
            }
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
