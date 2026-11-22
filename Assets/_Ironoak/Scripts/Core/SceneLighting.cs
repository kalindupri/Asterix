using Ironoak.Art;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Ironoak.Core
{
    /// <summary>
    /// Per-level light and colour settings (set on each level's LevelRoot).
    /// </summary>
    [System.Serializable]
    public class LevelLook
    {
        [Tooltip("Parallax set to show: Resources/Parallax/<name>.asset, or a built-in preset (Forest, Pixel).")]
        public string parallaxSet = "Forest";
        [Header("Ambient light")]
        public Color ambientColor = new Color(1f, 0.96f, 0.88f);
        [Range(0f, 2f)] public float ambientIntensity = 0.92f;
        [Header("Sun shafts (top-left god-rays)")]
        public bool sunShafts = true;
        public Color sunColor = new Color(1f, 0.84f, 0.55f);
        [Range(0f, 2f)] public float sunIntensity = 0.5f;
        [Header("Post-processing")]
        [Range(0f, 3f)] public float bloom = 0.45f;
        [Range(0f, 1f)] public float vignette = 0.24f;
        [Range(-50f, 50f)] public float saturation = 8f;
        [Range(-50f, 50f)] public float contrast = 6f;
        public Color colorFilter = new Color(1f, 0.98f, 0.95f);

        public static LevelLook Default => new LevelLook();
    }

    /// <summary>
    /// Builds the URP 2D lighting rig at runtime: one global (ambient) light, an optional
    /// sun-shaft cookie light pinned to the view, and a global post-processing volume.
    /// Does nothing if the project isn't running on URP.
    /// </summary>
    [DefaultExecutionOrder(110)]
    public class SceneLighting : MonoBehaviour
    {
        public static SceneLighting Instance { get; private set; }
        public static bool Active => GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset;

        Camera _cam;
        Light2D _ambient, _sun;
        Volume _volume;
        Bloom _bloom;
        Vignette _vignette;
        ColorAdjustments _grade;
        LevelLook _look = LevelLook.Default;
        float _t;

        public static SceneLighting Create(Transform parent, Camera cam)
        {
            if (!Active) return null;
            var go = new GameObject("SceneLighting");
            go.transform.SetParent(parent, false);
            var lighting = go.AddComponent<SceneLighting>();
            lighting._cam = cam;
            lighting.Build();
            Instance = lighting;
            return lighting;
        }

        void Build()
        {
            var camData = _cam.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = true;
            camData.antialiasing = AntialiasingMode.None;

            var ambientGo = new GameObject("AmbientLight");
            ambientGo.transform.SetParent(transform, false);
            _ambient = ambientGo.AddComponent<Light2D>();
            _ambient.lightType = Light2D.LightType.Global;

            var shafts = SpriteLibrary.GetPaintedOrNull("Environment/Forest", "fx_light_shafts");
            if (shafts != null)
            {
                var sunGo = new GameObject("SunShafts");
                sunGo.transform.SetParent(transform, false);
                _sun = sunGo.AddComponent<Light2D>();
                _sun.lightType = Light2D.LightType.Sprite;
                _sun.lightCookieSprite = shafts;
            }

            var volGo = new GameObject("PostVolume");
            volGo.transform.SetParent(transform, false);
            _volume = volGo.AddComponent<Volume>();
            _volume.isGlobal = true;
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            profile.name = "IronoakPost";
            _bloom = profile.Add<Bloom>(true);
            _bloom.threshold.Override(0.95f);
            _bloom.scatter.Override(0.65f);
            _vignette = profile.Add<Vignette>(true);
            _vignette.smoothness.Override(0.45f);
            _grade = profile.Add<ColorAdjustments>(true);
            _volume.sharedProfile = profile;

            Apply(LevelLook.Default);
        }

        public void Apply(LevelLook look)
        {
            _look = look ?? LevelLook.Default;
            _ambient.color = _look.ambientColor;
            _ambient.intensity = _look.ambientIntensity;
            if (_sun != null)
            {
                _sun.enabled = _look.sunShafts;
                _sun.color = _look.sunColor;
            }
            _bloom.intensity.Override(_look.bloom);
            _vignette.intensity.Override(_look.vignette);
            _grade.saturation.Override(_look.saturation);
            _grade.contrast.Override(_look.contrast);
            _grade.colorFilter.Override(_look.colorFilter);
        }

        void LateUpdate()
        {
            if (_sun == null || !_sun.enabled || _cam == null) return;
            // Pinned to the top-left of the view, slightly oversized, gently breathing.
            float halfH = _cam.orthographicSize, halfW = halfH * _cam.aspect;
            float size = halfH * 2.2f;
            float spriteSize = _sun.lightCookieSprite.bounds.size.y;
            Vector3 cp = _cam.transform.position;
            _sun.transform.position = new Vector3(cp.x - halfW + size * 0.42f, cp.y + halfH - size * 0.4f, 0f);
            float s = size / spriteSize;
            _sun.transform.localScale = new Vector3(s, s, 1f);
            _t += Time.deltaTime;
            _sun.intensity = _look.sunIntensity * (0.88f + 0.12f * Mathf.Sin(_t * 0.7f));
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_volume != null && _volume.sharedProfile != null) Destroy(_volume.sharedProfile);
        }
    }
}
