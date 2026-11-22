using Ironoak.Art;
using UnityEngine;
using UnityEngine.UI;

namespace Ironoak.Screens
{
    /// <summary>uGUI Image that displays text rendered with the built-in PixelFont.</summary>
    [RequireComponent(typeof(Image))]
    public class PixelText : MonoBehaviour
    {
        string _text = "";
        Color _top = Color.white, _bottom = Color.white;
        int _scale = 1;
        bool _outline = true;
        Vector2 _basePivot = new Vector2(0.5f, 0.5f);
        Image _image;
        Sprite _sprite;

        public string Text
        {
            get => _text;
            set
            {
                value ??= "";
                if (value == _text) return;
                _text = value;
                Rebuild();
            }
        }

        public void Configure(string text, Color top, Color bottom, int scale, bool outline, Vector2 basePivot)
        {
            _text = text ?? "";
            _top = top;
            _bottom = bottom;
            _scale = Mathf.Max(1, scale);
            _outline = outline;
            _basePivot = basePivot;
            Rebuild();
        }

        public void SetColor(Color top, Color bottom)
        {
            if (top == _top && bottom == _bottom) return;
            _top = top;
            _bottom = bottom;
            Rebuild();
        }

        void Rebuild()
        {
            if (_image == null) _image = GetComponent<Image>();
            Release();
            _sprite = PixelFont.Render(_text, _top, _bottom, _outline, UITheme.Outline);
            _image.sprite = _sprite;
            int w = _sprite.texture.width, h = _sprite.texture.height;
            var rt = _image.rectTransform;
            rt.sizeDelta = new Vector2(w * _scale, h * _scale);
            // Keep centred text on whole pixels even when its size is odd.
            rt.pivot = new Vector2(PivotFor(_basePivot.x, w), PivotFor(_basePivot.y, h));
        }

        static float PivotFor(float p, int size) => Mathf.Approximately(p, 0.5f) ? Mathf.Floor(size / 2f) / size : p;

        void Release()
        {
            if (_sprite == null) return;
            Destroy(_sprite.texture);
            Destroy(_sprite);
            _sprite = null;
        }

        void OnDestroy() => Release();
    }
}
