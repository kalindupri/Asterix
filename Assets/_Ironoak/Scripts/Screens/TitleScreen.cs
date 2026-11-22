using Ironoak.Controls;
using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Screens
{
    public class TitleScreen : MonoBehaviour
    {
        IronoakGame _game;
        MenuList _menu;
        RectTransform _logo, _controls;
        PixelText _best;
        int _controlsOpenedFrame;
        float _t;

        public static TitleScreen Create(Transform parent, IronoakGame game)
        {
            var root = UIFactory.Panel(parent, "TitleScreen");
            var screen = root.gameObject.AddComponent<TitleScreen>();
            screen._game = game;
            screen.Build(root);
            return screen;
        }

        void Build(RectTransform root)
        {
            var mid = UIFactory.Middle;
            _logo = UIFactory.Text(root, "IRONOAK", new Vector2(0f, 66f), mid, UITheme.GoldTop, UITheme.GoldBottom, 4).GetComponent<RectTransform>();
            UIFactory.Text(root, "THE LAST FREE VILLAGE", new Vector2(0f, 30f), mid, UITheme.Cream);

            _menu = MenuList.Create(root, new Vector2(0f, -10f), 14,
                ("START GAME", () => _game.StartGame()),
                ("CONTROLS", OpenControls),
                ("QUIT", () => _game.QuitGame()));

            _best = UIFactory.Text(root, "", new Vector2(0f, -78f), mid, UITheme.Dim);
            UIFactory.Text(root, "POC V0.1", new Vector2(-6f, 6f), new Vector2(1f, 0f), UITheme.Dim);

            _controls = UIFactory.Panel(root, "Controls");
            UIFactory.Solid(_controls, UITheme.Overlay);
            UIFactory.Text(_controls, "CONTROLS", new Vector2(0f, 70f), mid, UITheme.GoldTop, UITheme.GoldBottom, 2);
            string[] lines =
            {
                "MOVE     ARROWS / A D",
                "JUMP     SPACE / W / Z",
                "PAUSE    ESC / P",
                "GAMEPAD  STICK, A, START",
                "",
                "STOMP LEGIONARIES FROM ABOVE",
                "HOLD JUMP TO JUMP HIGHER",
            };
            int len = 0;
            foreach (var l in lines) len = Mathf.Max(len, l.Length);
            for (int i = 0; i < 4; i++) lines[i] = lines[i].PadRight(len);
            UIFactory.Text(_controls, string.Join("\n", lines), new Vector2(0f, 4f), mid, UITheme.Cream);
            UIFactory.Text(_controls, "PRESS ESC TO RETURN", new Vector2(0f, -72f), mid, UITheme.Dim);
            _controls.gameObject.SetActive(false);

            RefreshBest();
        }

        void OnEnable()
        {
            if (_menu == null) return;
            _controls.gameObject.SetActive(false);
            _menu.SetInteractable(true);
            RefreshBest();
        }

        void RefreshBest()
        {
            _best.Text = SaveData.HasRecord
                ? $"BEST: {SaveData.BestCoins} COINS IN {SaveData.FormatTime(SaveData.BestTime)}"
                : "";
        }

        void OpenControls()
        {
            _controls.gameObject.SetActive(true);
            _controlsOpenedFrame = Time.frameCount;
            _menu.SetInteractable(false);
        }

        void Update()
        {
            _t += Time.unscaledDeltaTime;
            _logo.anchoredPosition = new Vector2(0f, 66f + Mathf.Round(Mathf.Sin(_t * 2f) * 2f));

            if (_controls.gameObject.activeSelf && Time.frameCount > _controlsOpenedFrame &&
                (GameInput.BackPressed || GameInput.ConfirmPressed))
            {
                _controls.gameObject.SetActive(false);
                _menu.SetInteractable(true);
                Sfx.Play(SfxId.Select);
            }
        }
    }
}
