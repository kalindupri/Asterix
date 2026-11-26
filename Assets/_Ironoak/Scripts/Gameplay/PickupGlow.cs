using Ironoak.Art;
using Ironoak.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Ironoak.Gameplay
{
    /// <summary>
    /// Soft sparkle halo behind a painted pickup (Batch 2 fx_pickup_glow), breathing gently,
    /// plus a small coloured light when 2D lighting is on.
    /// </summary>
    public class PickupGlow : MonoBehaviour
    {
        SpriteRenderer _sr;
        Light2D _light;
        Color _tint;
        float _scale, _t;

        /// <summary>Adds a halo to a painted pickup; does nothing for pixel art or if the art is missing.</summary>
        public static PickupGlow Attach(SpriteRenderer pickup, Color tint, float scale = 1f)
        {
            if (pickup == null || !SpriteLibrary.IsPainted(pickup.sprite)) return null;
            var sprite = SpriteLibrary.GetPaintedOrNull("Props", "fx_pickup_glow");
            if (sprite == null) return null;
            var go = new GameObject("Glow");
            go.transform.SetParent(pickup.transform, false);
            var g = go.AddComponent<PickupGlow>();
            g._sr = go.AddComponent<SpriteRenderer>();
            g._sr.sprite = sprite;
            g._sr.sortingOrder = pickup.sortingOrder - 1;
            g._tint = tint;
            g._scale = scale;
            g._t = Random.value * 6f;
            if (SceneLighting.Active)
            {
                g._light = go.AddComponent<Light2D>();
                g._light.lightType = Light2D.LightType.Point;
                g._light.color = tint;
                g._light.pointLightInnerRadius = 0.15f;
                g._light.pointLightOuterRadius = 1.8f * scale;
                g._light.falloffIntensity = 0.7f;
            }
            return g;
        }

        void Update()
        {
            _t += Time.deltaTime;
            float p = 0.5f + 0.5f * Mathf.Sin(_t * 2.4f);
            var c = _tint;
            c.a = 0.55f + 0.35f * p;
            _sr.color = c;
            transform.localScale = Vector3.one * _scale * (0.92f + 0.12f * p);
            if (_light != null) _light.intensity = 0.45f + 0.25f * p;
        }
    }
}
