using Ironoak.Art;
using UnityEngine;
using UnityEngine.UI;

namespace Ironoak.Core
{
    /// <summary>
    /// Pixel-perfect presentation. The world camera and the game UI render into a fixed
    /// 480x270 render texture, which is then upscaled to the screen (by a whole-number
    /// factor where possible) with point filtering.
    /// </summary>
    public class PixelDisplay : MonoBehaviour
    {
        public const int Width = 480;
        public const int Height = 270;

        public Camera WorldCamera { get; private set; }
        public Canvas UICanvas { get; private set; }

        RenderTexture _rt;
        RawImage _view;
        Vector2Int _lastScreen;

        public static PixelDisplay Create(Transform parent)
        {
            var go = new GameObject("PixelDisplay");
            go.transform.SetParent(parent, false);
            var display = go.AddComponent<PixelDisplay>();
            display.Build();
            return display;
        }

        void Build()
        {
            _rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32)
            {
                filterMode = FilterMode.Point,
                name = "IronoakPixelRT"
            };
            _rt.Create();

            // World camera -> render texture
            var camGo = new GameObject("WorldCamera");
            camGo.transform.SetParent(transform, false);
            camGo.transform.position = new Vector3(0f, 0f, -10f);
            WorldCamera = camGo.AddComponent<Camera>();
            WorldCamera.orthographic = true;
            WorldCamera.orthographicSize = Height / 2f / PixelArt.PPU;
            WorldCamera.targetTexture = _rt;
            WorldCamera.clearFlags = CameraClearFlags.SolidColor;
            WorldCamera.backgroundColor = new Color32(0x4a, 0x90, 0xc8, 0xff);
            WorldCamera.nearClipPlane = 0.1f;
            WorldCamera.farClipPlane = 50f;
            WorldCamera.depth = -10;

            // Game UI, drawn by the world camera at native resolution
            var uiGo = new GameObject("PixelUI", typeof(RectTransform));
            uiGo.transform.SetParent(transform, false);
            UICanvas = uiGo.AddComponent<Canvas>();
            UICanvas.renderMode = RenderMode.ScreenSpaceCamera;
            UICanvas.worldCamera = WorldCamera;
            UICanvas.planeDistance = 1f;
            UICanvas.sortingOrder = 100;
            UICanvas.pixelPerfect = true;
            var scaler = uiGo.AddComponent<CanvasScaler>();
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
