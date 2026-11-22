using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Puzzles
{
    /// <summary>A hanging bronze bell. Hit it with the sling to send a signal.</summary>
    public class Bell : SignalSender, IHittable
    {
        [Tooltip("On: the bell stays 'rung' (latched). Off: a momentary pulse each hit (e.g. reset bells).")]
        public bool staysOn = true;
        public Transform swingPivot;

        float _swing, _swingVel;

        public void OnSlingHit(Vector2 point, Vector2 direction)
        {
            Sfx.Play(SfxId.Bell);
            _swingVel += 260f * Mathf.Sign(direction.x == 0f ? 1f : direction.x);
            Fx.Burst(point, new Color(1f, 0.85f, 0.4f), 6, 2.5f, 0.35f, 1f);
            if (staysOn) Emit(true);
            else Pulse();
        }

        void Update()
        {
            if (swingPivot == null) return;
            // damped spring swing
            _swingVel += -_swing * 60f * Time.deltaTime;
            _swingVel *= Mathf.Exp(-3f * Time.deltaTime);
            _swing += _swingVel * Time.deltaTime;
            swingPivot.localRotation = Quaternion.Euler(0f, 0f, -_swing);
        }
    }
}
