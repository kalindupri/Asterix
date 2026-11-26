using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Ironoak.Gameplay
{
    /// <summary>Torch checkpoint. Lights up and becomes the respawn point.</summary>
    public class Checkpoint : MonoBehaviour
    {
        SpriteRenderer _sr, _flameSr;
        Sprite _off, _on1, _on2;
        Sprite[] _flames;
        Light2D _light;
        bool _active;
        float _t;

        public static Checkpoint Spawn(Transform parent, Vector2 position)
        {
            var go = new GameObject("Checkpoint");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 2f);
            col.offset = new Vector2(0f, 1f);
            var cp = go.AddComponent<Checkpoint>();
            cp._sr = sr;
            return cp;
        }

        void Awake() => _sr = GetComponent<SpriteRenderer>();

        void Start()
        {
            _off = SpriteLibrary.Get("torch_off");
            _on1 = SpriteLibrary.Get("torch_on1");
            _on2 = SpriteLibrary.Get("torch_on2");
            _sr.sprite = _off;

            if (SpriteLibrary.IsPainted(_off))
            {
                var f = new Sprite[4];
                for (int i = 0; i < 4; i++) f[i] = SpriteLibrary.GetPaintedOrNull("Props", "fx_flame_" + (i + 1));
                if (f[0] != null && f[3] != null)
                {
                    _flames = f;
                    _flameSr = new GameObject("Flame").AddComponent<SpriteRenderer>();
                    _flameSr.transform.SetParent(transform, false);
                    _flameSr.transform.localPosition = new Vector3(0f, FlameBase, 0f);
                    _flameSr.transform.localScale = Vector3.one * 0.72f;
                    _flameSr.sortingOrder = _sr.sortingOrder + 1;
                    _flameSr.enabled = false;
                }
            }
        }

        const float FlameBase = 1.84f; // inside the painted brazier bowl

        void Ignite()
        {
            if (_flameSr != null) _flameSr.enabled = true;
            if (!SceneLighting.Active || _light != null) return;
            var go = new GameObject("FireLight");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, FlameBase + 0.3f, 0f);
            _light = go.AddComponent<Light2D>();
            _light.lightType = Light2D.LightType.Point;
            _light.color = new Color(1f, 0.62f, 0.3f);
            _light.pointLightInnerRadius = 0.3f;
            _light.pointLightOuterRadius = 3.6f;
            _light.falloffIntensity = 0.6f;
            _light.intensity = 1f;
        }

        void Update()
        {
            if (!_active) return;
            _t += Time.deltaTime;
            if (_flameSr != null)
            {
                _sr.sprite = _on1;
                _flameSr.sprite = _flames[(int)(_t * 12f) % _flames.Length] ?? _flames[0];
            }
            else _sr.sprite = ((int)(_t * 8f) & 1) == 0 ? _on1 : _on2;
            if (_light != null) _light.intensity = 0.95f + 0.12f * Mathf.Sin(_t * 17f) + 0.08f * Mathf.Sin(_t * 7.3f);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_active) return;
            var player = other.GetComponent<PlayerController>();
            if (player == null) return;
            _active = true;
            Ignite();
            player.Health.RespawnPoint = transform.position;
            Sfx.Play(SfxId.Checkpoint);
            GameEvents.RaiseCheckpointReached();
        }
    }
}
