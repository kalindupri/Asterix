using Ironoak.Core;
using Ironoak.Gameplay;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Puzzles
{
    /// <summary>
    /// A heavy wooden crate. Push it by walking into it, lift it with roots, knock it sideways
    /// with the sling. Optionally hangs from a rope until released, or respawns on a signal.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Crate : SignalReceiver, IHittable
    {
        public enum SignalMode { None, ReleaseWhenPowered, RespawnOnSignal }

        public SignalMode signalMode = SignalMode.None;
        [Tooltip("Starts frozen in the air (hanging) until released.")]
        public bool startHanging;
        public float slingImpulse = 7f;

        Rigidbody2D _rb;
        Vector2 _start;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _start = transform.position;
            if (startHanging) _rb.bodyType = RigidbodyType2D.Kinematic;
            var col = GetComponent<Collider2D>();
            if (col != null && col.sharedMaterial == null) col.sharedMaterial = PhysicsMaterials.Crate;
        }

        protected override void OnPoweredChanged(bool powered)
        {
            if (powered && signalMode == SignalMode.ReleaseWhenPowered && _rb.bodyType != RigidbodyType2D.Dynamic)
            {
                _rb.bodyType = RigidbodyType2D.Dynamic;
                _rb.WakeUp();
            }
        }

        protected override void OnInputRisingEdge(SignalSender sender)
        {
            if (signalMode == SignalMode.RespawnOnSignal) Respawn();
        }

        public void OnSlingHit(Vector2 point, Vector2 direction)
        {
            if (_rb.bodyType != RigidbodyType2D.Dynamic) return;
            _rb.AddForce(new Vector2(Mathf.Sign(direction.x) * slingImpulse, 1.5f), ForceMode2D.Impulse);
            Sfx.Play(SfxId.Hit);
        }

        public void Respawn()
        {
            Fx.Dust(transform.position);
            _rb.bodyType = startHanging ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
            _rb.position = _start;
            transform.position = _start;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            Fx.Dust(_start);
        }

        void FixedUpdate()
        {
            if (transform.position.y < PlayerHealth.KillY) Respawn();
        }

        void OnCollisionEnter2D(Collision2D c)
        {
            if (c.relativeVelocity.magnitude > 6f)
            {
                Sfx.Play(SfxId.Thud, 0.6f);
                CameraRig.Instance?.Shake(0.04f, 0.12f);
            }
        }
    }
}
