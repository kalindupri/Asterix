using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Ironoak.Screens
{
    /// <summary>Full-screen fade used for scene transitions (runs on unscaled time).</summary>
    public class FadeOverlay : MonoBehaviour
    {
        Image _image;

        public static FadeOverlay Create(Transform parent)
        {
            var img = UIFactory.Solid(parent, new Color(0.055f, 0.04f, 0.07f, 1f));
            img.gameObject.name = "Fade";
            var fade = img.gameObject.AddComponent<FadeOverlay>();
            fade._image = img;
            img.transform.SetAsLastSibling();
            return fade;
        }

        public IEnumerator FadeTo(float alpha, float seconds)
        {
            transform.SetAsLastSibling();
            float start = _image.color.a;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                Set(Mathf.Lerp(start, alpha, t / seconds));
                yield return null;
            }
            Set(alpha);
        }

        void Set(float a)
        {
            var c = _image.color;
            c.a = a;
            _image.color = c;
            _image.enabled = a > 0.001f;
        }
    }
}
