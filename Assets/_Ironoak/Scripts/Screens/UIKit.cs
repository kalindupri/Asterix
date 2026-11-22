using Ironoak.Art;
using UnityEngine;
using UnityEngine.UI;

namespace Ironoak.Screens
{
    /// <summary>Shared UI colours.</summary>
    public static class UITheme
    {
        public static readonly Color GoldTop = PixelArt.Hex("ffe27a");
        public static readonly Color GoldBottom = PixelArt.Hex("e0902a");
        public static readonly Color Cream = PixelArt.Hex("f3e6c8");
        public static readonly Color Dim = PixelArt.Hex("b9a88a");
        public static readonly Color Red = PixelArt.Hex("e04848");
        public static readonly Color Green = PixelArt.Hex("8fd36a");
        public static readonly Color Outline = PixelArt.Hex("1d1424");
        public static readonly Color Overlay = new Color(0.06f, 0.04f, 0.08f, 0.72f);
    }

    /// <summary>Helpers that build pixel-perfect uGUI elements in code.</summary>
    public static class UIFactory
    {
        public static readonly Vector2 Middle = new Vector2(0.5f, 0.5f);

        public static RectTransform Panel(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static Image Solid(Transform parent, Color color)
        {
            var rt = Panel(parent, "Solid");
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static PixelText Text(Transform parent, string text, Vector2 position, Vector2 anchor, Color top, Color? bottom = null, int scale = 1, bool outline = true)
        {
            var go = new GameObject("Text", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.anchoredPosition = position;
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            var pt = go.AddComponent<PixelText>();
            pt.Configure(text, top, bottom ?? top, scale, outline, anchor);
            return pt;
        }

        public static Image Sprite(Transform parent, Sprite sprite, Vector2 position, Vector2 anchor, int scale = 1)
        {
            var go = new GameObject(sprite != null ? sprite.name : "Sprite", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = position;
            var img = go.AddComponent<Image>();
            img.raycastTarget = false;
            img.sprite = sprite;
            if (sprite != null) rt.sizeDelta = sprite.rect.size * scale;
            return img;
        }
    }
}
