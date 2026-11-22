using System.Collections.Generic;
using Ironoak.Art;
using UnityEngine;

namespace Ironoak.Core
{
    /// <summary>
    /// Endless multi-layer parallax. Each layer is a 480 px wide seamless strip,
    /// laid out three times side by side and rearranged as the camera moves.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class ParallaxBackground : MonoBehaviour
    {
        class Layer
        {
            public Transform[] Copies;
            public float Factor;   // 0 = moves with world, 1 = fixed to camera
            public float Width;
            public float YOffset;  // world units above the bottom of the view
        }

        Camera _cam;
        Transform _sky;
        readonly List<Layer> _layers = new List<Layer>();

        public static ParallaxBackground Create(Transform parent, Camera cam)
        {
            var go = new GameObject("Parallax");
            go.transform.SetParent(parent, false);
            var p = go.AddComponent<ParallaxBackground>();
            p._cam = cam;
            p.Build();
            return p;
        }

        void Build()
        {
            var skyGo = new GameObject("bg_sky");
            skyGo.transform.SetParent(transform, false);
            var skySr = skyGo.AddComponent<SpriteRenderer>();
            skySr.sprite = SpriteLibrary.Get("bg_sky");
            skySr.sortingOrder = -100;
            _sky = skyGo.transform;

            AddLayer("bg_clouds", 0.92f, 10.2f, -90);
            AddLayer("bg_mountains", 0.85f, 1.0f, -80);
            AddLayer("bg_hills", 0.7f, 0.8f, -70);
            AddLayer("bg_forest", 0.45f, 0f, -60);
        }

        void AddLayer(string spriteName, float factor, float yOffset, int order)
        {
            var sprite = SpriteLibrary.Get(spriteName);
            var layer = new Layer
            {
                Copies = new Transform[3],
                Factor = factor,
                Width = sprite.bounds.size.x,
                YOffset = yOffset
            };
            for (int i = 0; i < 3; i++)
            {
                var go = new GameObject($"{spriteName}_{i}");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = order;
                layer.Copies[i] = go.transform;
            }
            _layers.Add(layer);
        }

        static float Snap(float v) => Mathf.Round(v * PixelArt.PPU) / PixelArt.PPU;

        void LateUpdate()
        {
            if (_cam == null) return;
            Vector3 cp = _cam.transform.position;
            float bottom = cp.y - _cam.orthographicSize;
            _sky.position = new Vector3(cp.x, cp.y, 0f);

            foreach (var layer in _layers)
            {
                float baseX = cp.x * layer.Factor;
                float k = Mathf.Floor((cp.x - baseX) / layer.Width);
                for (int i = 0; i < 3; i++)
                {
                    float x = baseX + (k + i - 1) * layer.Width;
                    layer.Copies[i].position = new Vector3(Snap(x), Snap(bottom + layer.YOffset), 0f);
                }
            }
        }
    }
}
