using Ironoak.Controls;
using Ironoak.Core;
using Ironoak.Gameplay;
using Ironoak.Puzzles;
using UnityEngine;

namespace Ironoak.Powers
{
    /// <summary>
    /// Heartwood Torc: press Power near a glowing sap knot to raise or lower its root.
    /// With a mouse you can also point at any knot on screen within reach.
    /// </summary>
    public class RootgrowAbility : MonoBehaviour
    {
        public float nearRange = 1.8f;
        public float pointerReach = 10f;

        PlayerController _pc;
        PlayerPowers _powers;
        SapKnot _focus;

        public event System.Action Cast;

        void Awake()
        {
            _pc = GetComponent<PlayerController>();
            _powers = GetComponent<PlayerPowers>();
        }

        void OnDisable() => SetFocus(null);

        void Update()
        {
            if (!_pc.InputEnabled || _powers == null || !_powers.Has(Power.Rootgrow) || Time.timeScale == 0f)
            {
                SetFocus(null);
                return;
            }

            Vector2 me = (Vector2)transform.position + Vector2.up * 0.4f;
            SapKnot pointed = null;
            if (GameInput.MouseActive && !GameInput.UsingGamepad && PixelDisplay.Instance != null)
            {
                Vector2 cursor = PixelDisplay.Instance.ScreenToWorld(GameInput.MouseScreen);
                pointed = SapKnot.FindNearest(cursor, 1.4f);
                if (pointed != null && Vector2.Distance(pointed.transform.position, me) > pointerReach) pointed = null;
            }
            var near = SapKnot.FindNearest(me, nearRange);
            SetFocus(pointed != null ? pointed : near);

            if (GameInput.PowerPressed && _focus != null)
            {
                _focus.Toggle();
                Cast?.Invoke();
            }
        }

        void SetFocus(SapKnot knot)
        {
            if (_focus == knot) return;
            if (_focus != null) _focus.SetHighlighted(false);
            _focus = knot;
            if (_focus != null) _focus.SetHighlighted(true);
        }
    }
}
