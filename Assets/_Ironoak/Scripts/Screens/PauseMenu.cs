using Ironoak.Controls;
using Ironoak.Core;
using UnityEngine;

namespace Ironoak.Screens
{
    public class PauseMenu : MonoBehaviour
    {
        IronoakGame _game;
        int _openedFrame;

        public static PauseMenu Create(Transform parent, IronoakGame game)
        {
            var root = UIFactory.Panel(parent, "PauseMenu");
            var menu = root.gameObject.AddComponent<PauseMenu>();
            menu._game = game;
            UIFactory.Solid(root, UITheme.Overlay);
            UIFactory.Text(root, "PAUSED", new Vector2(0f, 40f), UIFactory.Middle, UITheme.GoldTop, UITheme.GoldBottom, 2);
            MenuList.Create(root, new Vector2(0f, -6f), 14,
                ("RESUME", () => game.Resume()),
                ("RESTART", () => game.StartGame()),
                ("QUIT TO TITLE", () => game.ShowTitle()));
            return menu;
        }

        void OnEnable() => _openedFrame = Time.frameCount;

        void Update()
        {
            if (Time.frameCount > _openedFrame && (GameInput.PausePressed || GameInput.BackPressed))
                _game.Resume();
        }
    }
}
