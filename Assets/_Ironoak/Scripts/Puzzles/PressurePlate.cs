using System.Collections.Generic;
using Ironoak.Art;
using Ironoak.Gameplay;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Puzzles
{
    /// <summary>A stone plate that sends a signal while a crate or Brenna stands on it.</summary>
    public class PressurePlate : SignalSender
    {
        public bool acceptPlayer = true;
        public bool acceptCrates = true;

        static readonly List<Collider2D> Hits = new List<Collider2D>(8);
        SpriteRenderer _sr;
        Sprite _up, _down;
        ContactFilter2D _filter;

        public bool Pressed => IsOn;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _up = SpriteLibrary.Get("plate_up");
            _down = SpriteLibrary.Get("plate_down");
            _filter = new ContactFilter2D { useTriggers = false };
        }

        void FixedUpdate()
        {
            Vector2 center = (Vector2)transform.position + new Vector2(0f, 0.2f);
            int n = Physics2D.OverlapBox(center, new Vector2(0.8f, 0.3f), 0f, _filter, Hits);
            bool pressed = false;
            for (int i = 0; i < n && !pressed; i++)
            {
                var rb = Hits[i].attachedRigidbody;
                if (rb == null) continue;
                if (acceptCrates && rb.GetComponent<Crate>() != null) pressed = true;
                else if (acceptPlayer && rb.GetComponent<PlayerController>() != null) pressed = true;
            }
            if (pressed != IsOn)
            {
                Sfx.Play(SfxId.Hit, 0.7f);
                if (_sr != null) _sr.sprite = pressed ? _down : _up;
                Emit(pressed);
            }
        }
    }
}
