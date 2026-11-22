using System;
using System.Collections.Generic;
using Ironoak.Controls;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Screens
{
    /// <summary>Vertical keyboard/gamepad menu in retro style ("&gt; START &lt;").</summary>
    public class MenuList : MonoBehaviour
    {
        struct Item
        {
            public string Label;
            public Action Action;
            public PixelText Text;
        }

        readonly List<Item> _items = new List<Item>();
        int _index;
        int _readyFrame;
        bool _interactable = true;

        public static MenuList Create(Transform parent, Vector2 position, int spacing, params (string label, Action action)[] items)
        {
            var go = new GameObject("Menu", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = UIFactory.Middle;
            rt.anchoredPosition = position;
            rt.sizeDelta = Vector2.zero;

            var menu = go.AddComponent<MenuList>();
            for (int i = 0; i < items.Length; i++)
            {
                var text = UIFactory.Text(rt, items[i].label, new Vector2(0f, -i * spacing), UIFactory.Middle, UITheme.Dim);
                menu._items.Add(new Item { Label = items[i].label, Action = items[i].action, Text = text });
            }
            menu.Refresh();
            return menu;
        }

        public void SetInteractable(bool value)
        {
            _interactable = value;
            if (value) _readyFrame = Time.frameCount + 1;
        }

        void OnEnable()
        {
            _index = 0;
            _readyFrame = Time.frameCount + 1; // ignore the key press that opened this menu
            Refresh();
        }

        void Update()
        {
            if (!_interactable || _items.Count == 0 || Time.frameCount < _readyFrame) return;

            if (GameInput.MenuUp) Move(-1);
            else if (GameInput.MenuDown) Move(1);
            else if (GameInput.ConfirmPressed)
            {
                Sfx.Play(SfxId.Confirm);
                _items[_index].Action?.Invoke();
            }
        }

        void Move(int delta)
        {
            _index = (_index + delta + _items.Count) % _items.Count;
            Sfx.Play(SfxId.Select);
            Refresh();
        }

        void Refresh()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                bool selected = i == _index;
                item.Text.Text = selected ? "> " + item.Label + " <" : item.Label;
                item.Text.SetColor(selected ? UITheme.GoldTop : UITheme.Dim, selected ? UITheme.GoldBottom : UITheme.Dim);
            }
        }
    }
}
