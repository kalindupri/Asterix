using Ironoak.Core;
using UnityEngine;

namespace Ironoak.Screens
{
    public class ResultScreen : MonoBehaviour
    {
        PixelText _title, _stats;
        MenuList _winMenu, _loseMenu;

        public static ResultScreen Create(Transform parent, IronoakGame game)
        {
            var root = UIFactory.Panel(parent, "ResultScreen");
            var screen = root.gameObject.AddComponent<ResultScreen>();
            var mid = UIFactory.Middle;
            UIFactory.Solid(root, UITheme.Overlay);
            screen._title = UIFactory.Text(root, "", new Vector2(0f, 62f), mid, UITheme.GoldTop, UITheme.GoldBottom, 2);
            screen._stats = UIFactory.Text(root, "", new Vector2(0f, 12f), mid, UITheme.Cream);
            screen._winMenu = MenuList.Create(root, new Vector2(0f, -46f), 14,
                ("PLAY AGAIN", () => game.StartGame()),
                ("TITLE SCREEN", () => game.ShowTitle()));
            screen._loseMenu = MenuList.Create(root, new Vector2(0f, -46f), 14,
                ("TRY AGAIN", () => game.StartGame()),
                ("TITLE SCREEN", () => game.ShowTitle()));
            return screen;
        }

        public void Show(bool win, string stats)
        {
            gameObject.SetActive(true);
            _title.Text = win ? "VILLAGE REACHED!" : "DEFEATED";
            _title.SetColor(win ? UITheme.GoldTop : UITheme.Red, win ? UITheme.GoldBottom : UITheme.Red);
            // Pad every line to the same width so the centred columns line up (monospace font).
            var lines = stats.Split('\n');
            int width = 0;
            foreach (var l in lines) width = Mathf.Max(width, l.Length);
            for (int i = 0; i < lines.Length; i++) lines[i] = lines[i].PadRight(width);
            _stats.Text = string.Join("\n", lines);
            _winMenu.gameObject.SetActive(win);
            _loseMenu.gameObject.SetActive(!win);
        }
    }
}
