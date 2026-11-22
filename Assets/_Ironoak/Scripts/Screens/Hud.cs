using System.Collections.Generic;
using Ironoak.Art;
using Ironoak.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Ironoak.Screens
{
    public class Hud : MonoBehaviour
    {
        const int MaxHearts = 3;
        const float ToastTime = 2.4f;

        Image[] _hearts;
        Sprite _full, _empty;
        PixelText _coins, _time, _banner, _bannerSub, _toast;
        float _bannerTime, _toastTime;
        int _lastSecond = -1;
        readonly Queue<string> _toasts = new Queue<string>();

        public static Hud Create(Transform parent)
        {
            var root = UIFactory.Panel(parent, "Hud");
            var hud = root.gameObject.AddComponent<Hud>();
            hud.Build(root);
            return hud;
        }

        void Build(RectTransform root)
        {
            var topLeft = new Vector2(0f, 1f);
            _full = SpriteLibrary.Get("heart_full");
            _empty = SpriteLibrary.Get("heart_empty");
            _hearts = new Image[MaxHearts];
            bool painted = SpriteLibrary.IsPainted(_full);
            for (int i = 0; i < MaxHearts; i++)
            {
                _hearts[i] = UIFactory.Sprite(root, _full, new Vector2((painted ? 5f : 8f) + i * (painted ? 14f : 11f), painted ? -5f : -8f), topLeft);
                if (painted) _hearts[i].rectTransform.sizeDelta = new Vector2(14f, 14f);
            }

            var coinIcon = SpriteLibrary.Get("ui_coin");
            if (SpriteLibrary.IsPainted(coinIcon))
                UIFactory.Sprite(root, coinIcon, new Vector2(6f, -20f), topLeft).rectTransform.sizeDelta = new Vector2(11f, 11f);
            else
                UIFactory.Sprite(root, SpriteLibrary.Get("coin_0"), new Vector2(5f, -19f), topLeft);
            _coins = UIFactory.Text(root, "X 0", new Vector2(21f, -23f), topLeft, UITheme.Cream);
            _time = UIFactory.Text(root, "TIME 0:00", new Vector2(-8f, -8f), new Vector2(1f, 1f), UITheme.Cream);
            _banner = UIFactory.Text(root, "", new Vector2(0f, 50f), UIFactory.Middle, UITheme.GoldTop, UITheme.GoldBottom, 2);
            _bannerSub = UIFactory.Text(root, "", new Vector2(0f, 30f), UIFactory.Middle, UITheme.Cream);
            _toast = UIFactory.Text(root, "", new Vector2(0f, -10f), new Vector2(0.5f, 1f), UITheme.Cream);
            _toast.gameObject.SetActive(false);
        }

        public void ResetHud(int health, int coins, int totalCoins, string levelName)
        {
            SetHealth(health);
            SetCoins(coins, totalCoins);
            _lastSecond = -1;
            SetTime(0f);
            _toasts.Clear();
            _toastTime = 0f;
            _toast.gameObject.SetActive(false);
            ShowBanner(levelName, "", 2.5f);
        }

        public void ShowBanner(string title, string subtitle, float seconds)
        {
            _banner.Text = title;
            _bannerSub.Text = subtitle;
            _banner.gameObject.SetActive(true);
            _bannerSub.gameObject.SetActive(!string.IsNullOrEmpty(subtitle));
            _bannerTime = seconds;
        }

        public void Toast(string message) => _toasts.Enqueue(message);

        public void SetHealth(int health)
        {
            for (int i = 0; i < _hearts.Length; i++) _hearts[i].sprite = i < health ? _full : _empty;
        }

        public void SetCoins(int coins, int total) => _coins.Text = $"X {coins}/{total}";

        public void SetTime(float seconds)
        {
            int s = Mathf.FloorToInt(seconds);
            if (s == _lastSecond) return;
            _lastSecond = s;
            _time.Text = "TIME " + SaveData.FormatTime(seconds);
        }

        void Update()
        {
            float dt = Time.unscaledDeltaTime;
            if (_bannerTime > 0f)
            {
                _bannerTime -= dt;
                if (_bannerTime <= 0f)
                {
                    _banner.gameObject.SetActive(false);
                    _bannerSub.gameObject.SetActive(false);
                }
            }

            if (_toastTime > 0f)
            {
                _toastTime -= dt;
                if (_toastTime <= 0f) _toast.gameObject.SetActive(false);
            }
            else if (_toasts.Count > 0)
            {
                _toast.Text = _toasts.Dequeue();
                _toast.gameObject.SetActive(true);
                _toastTime = ToastTime;
            }
        }
    }
}
