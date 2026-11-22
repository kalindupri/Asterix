using Ironoak.Art;
using Ironoak.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Ironoak.Screens
{
    public class Hud : MonoBehaviour
    {
        const int MaxHearts = 3;

        Image[] _hearts;
        Sprite _full, _empty;
        PixelText _coins, _time, _banner;
        float _bannerTime;
        int _lastSecond = -1;

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
            for (int i = 0; i < MaxHearts; i++)
                _hearts[i] = UIFactory.Sprite(root, _full, new Vector2(8f + i * 11f, -8f), topLeft);

            UIFactory.Sprite(root, SpriteLibrary.Get("coin_0"), new Vector2(5f, -19f), topLeft);
            _coins = UIFactory.Text(root, "X 0", new Vector2(21f, -23f), topLeft, UITheme.Cream);
            _time = UIFactory.Text(root, "TIME 0:00", new Vector2(-8f, -8f), new Vector2(1f, 1f), UITheme.Cream);
            _banner = UIFactory.Text(root, "", new Vector2(0f, 50f), UIFactory.Middle, UITheme.GoldTop, UITheme.GoldBottom, 2);
        }

        public void ResetHud(int health, int coins, int totalCoins, string levelName)
        {
            SetHealth(health);
            SetCoins(coins, totalCoins);
            _lastSecond = -1;
            SetTime(0f);
            _banner.Text = levelName;
            _banner.gameObject.SetActive(true);
            _bannerTime = 2.5f;
        }

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
            if (_bannerTime <= 0f) return;
            _bannerTime -= Time.deltaTime;
            if (_bannerTime <= 0f) _banner.gameObject.SetActive(false);
        }
    }
}
