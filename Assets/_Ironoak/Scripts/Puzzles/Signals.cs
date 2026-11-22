using System.Collections.Generic;
using UnityEngine;

namespace Ironoak.Puzzles
{
    /// <summary>
    /// Anything that can switch puzzle logic on or off: bells, ropes, pressure plates,
    /// sap knots, trigger zones. Wire its <see cref="targets"/> in the Inspector.
    /// </summary>
    public abstract class SignalSender : MonoBehaviour
    {
        [Tooltip("Receivers driven by this sender (gates, bridges, pools, crates...)")]
        public List<SignalReceiver> targets = new List<SignalReceiver>();

        public bool IsOn { get; private set; }

        protected void Emit(bool on)
        {
            if (on == IsOn) return;
            IsOn = on;
            foreach (var t in targets)
                if (t != null) t.SetInput(this, on);
        }

        /// <summary>Momentary signal: on, then immediately off (receivers act on the rising edge).</summary>
        protected void Pulse()
        {
            Emit(true);
            Emit(false);
        }

        protected void ResetSignal()
        {
            if (IsOn) Emit(false);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.9f);
            foreach (var t in targets)
                if (t != null) Gizmos.DrawLine(transform.position, t.transform.position);
        }
    }

    /// <summary>
    /// Anything driven by senders. Becomes powered when at least
    /// <see cref="requiredInputs"/> senders are on; with <see cref="latch"/> it stays powered.
    /// </summary>
    public abstract class SignalReceiver : MonoBehaviour
    {
        [Min(1)] public int requiredInputs = 1;
        [Tooltip("Once powered, stay powered even if the inputs switch off.")]
        public bool latch;

        readonly HashSet<SignalSender> _active = new HashSet<SignalSender>();
        bool _powered;

        public bool Powered => _powered;

        public void SetInput(SignalSender sender, bool on)
        {
            bool wasActive = _active.Contains(sender);
            if (on) _active.Add(sender);
            else _active.Remove(sender);
            if (on && !wasActive) OnInputRisingEdge(sender);

            bool powered = _active.Count >= Mathf.Max(1, requiredInputs);
            if (_powered && latch) return;
            if (powered == _powered) return;
            _powered = powered;
            OnPoweredChanged(powered);
        }

        /// <summary>Called whenever any input switches on (used for momentary actions such as respawns).</summary>
        protected virtual void OnInputRisingEdge(SignalSender sender) { }

        protected abstract void OnPoweredChanged(bool powered);
    }

    /// <summary>Things that react to a sling stone.</summary>
    public interface IHittable
    {
        void OnSlingHit(Vector2 point, Vector2 direction);
    }
}
