using Ironoak.Art;
using UnityEngine;
using UnityEngine.UI;

namespace Ironoak.Core
{
    /// <summary>
    /// Owns the world camera and the game UI canvas, in one of two modes (see IronoakConfig):
    /// - PixelPerfect: world and UI render into a fixed 480x270 render texture, upscaled to the
    ///   screen by a whole-number factor with point filtering.
    /// - Native: the camera renders straight to the screen (for hand-painted art). The UI keeps a
    ///   480x270 reference layout scaled to the screen height.
    /// The world view is the same size in both modes (16.875 units tall).
    /// </summary>
    public class PixelDisplay : MonoBehaviour
    {
        public const int Width = 480;
        public const int Height = 270;

        public static PixelDisplay Instance { get; private set; }

        public Camera WorldCamera { get; private set; }
        public Canvas UICanvas { get; private set; }
        public bool Native { get; private set; }

        RenderTexture _rt;
        RawImage _view;
        Vector2Int _lastScreen;

        /// <summary>Converts a screen position (e.g. the mouse) to a world position.</summary>
        public Vector2 ScreenToWorld(Vector2 screen)
        {
            if (Native) return WorldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
            Vector2 size = _view.rectTransform.sizeDelta;
            float x0 = (Screen.width - size.x) * 0.5f, y0 = (Screen.height - size.y) * 0.5f;
            float u = (screen.x - x0) / size.x, v = (screen.y - y0) / size.y;
            return WorldCamera.ViewportToWorldPoint(new Vector3(u, v, 10f));
        }

        public static PixelDisplay Create(Transform parent)
        {
            var go = new GameObject("PixelDisplay");
            go.transform.SetParent(parent, false);
            var display = go.AddComponent<PixelDisplay>();
            Instance = display;
            display.Native = !IronoakConfig.PixelPerfect;
            display.Build();
            return display;
        }

        void Build()
        {
            var camGo = new GameObject("WorldCamera");
            camGo.transform.SetParent(transform, false);
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            WorldCamera = camGo.AddComponent<Camera>();
            WorldCamera.orthographic = true;
            WorldCamera.orthographicSize = Height / 2f / PixelArt.PPU;
            WorldCamera.clearFlags = CameraClearFlags.SolidColor;
            WorldCamera.backgroundColor = new Color32(0x4a, 0x90, 0xc8, 0xff);
            WorldCamera.nearClipPlane = 0.1f;
            WorldCamera.farClipPlane = 50f;

            var uiGo = new GameObject("GameUI", typeof(RectTransform));
            uiGo.transform.SetParent(transform, false);
            UICanvas = uiGo.AddComponent<Canvas>();
            UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
            UICanvas.worldCamera = WorldCamera;
            UICanvas.planeDistance = 1f;
            UICanvas.sortingOrder = 100;
            var scaler = uiGo.AddComponent<CanvasScaler>();

            if (Native)
            {
                WorldCamera.depth = 0;
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(Width, Height);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 1f;
                return;
            }

            _rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point,
                name = "IronoakPixelRT"
            };
            _rt.Create();
            WorldCamera.targetTexture = _rt;
            WorldCamera.depth = -10;
            UICanvas.pixelPerfect = true;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = 1f;

            // Display camera only clears the backbuffer (letterbox colour)
            var dcGo = new GameObject("DisplayCamera");
            dcGo.transform.SetParent(transform, false);
            var displayCam = dcGo.AddComponent<Camera>();
            displayCam.cullingMask = 0;
            displayCam.clearFlags = CameraClearFlags.SolidColor;
            displayCam.backgroundColor = new Color32(0x0e, 0x0a, 0x12, 0xff);
            displayCam.orthographic = true;
            displayCam.depth = 10;

            // Overlay canvas that shows the render texture on screen
            var ovGo = new GameObject("ScreenCanvas", typeof(RectTransform));
            ovGo.transform.SetParent(transform, false);
            var overlay = ovGo.AddComponent<Canvas>();
            overlay.renderMode = RenderMode.ScreenSpaceOverlay;
            overlay.sortingOrder = 0;

            var viewGo = new GameObject("PixelView", typeof(RectTransform));
            viewGo.transform.SetParent(ovGo.transform, false);
            _view = viewGo.AddComponent<RawImage>();
            _view.texture = _rt;
            _view.raycastTarget = false;
            var rt = _view.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;

            Fit();
        }

        void LateUpdate()
        {
            if (Native) return;
            if (Screen.width != _lastScreen.x || Screen.height != _lastScreen.y) Fit();
        }

        void Fit()
        {
            _lastScreen = new Vector2Int(Screen.width, Screen.height);
            float scale = Mathf.Min(Screen.width / (float)Width, Screen.height / (float)Height);
            // Prefer whole-number scaling (crispest) unless it wastes too much screen.
            if (scale >= 1f && Mathf.Floor(scale) / scale > 0.85f) scale = Mathf.Floor(scale);
            _view.rectTransform.sizeDelta = new Vector2(Width * scale, Height * scale);
        }

        void OnDestroy()
        {
            if (_rt != null)
            {
                _rt.Release();
                Destroy(_rt);
            }
        }
    }
}
