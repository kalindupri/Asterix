using System;
using System.Collections.Generic;
using Ironoak.Controls;
using Ironoak.Powers;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>
    /// Tight platformer movement for Brenna: acceleration curves, coyote time,
    /// jump buffering, variable jump height and faster falling.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Run")]
        public float maxSpeed = 6.5f;
        public float groundAccel = 70f;
        public float groundDecel = 80f;
        public float airAccel = 45f;
        public float airDecel = 25f;

        [Header("Jump")]
        public float jumpVelocity = 16f; // ~3.6 tiles: 3-tile steps are comfortable, 4-tile walls are not
        public float gravityScale = 3.6f;
        public float fallGravityMultiplier = 1.7f;
        public float jumpCutMultiplier = 0.45f;
        public float maxFallSpeed = 20f;
        public float coyoteTime = 0.1f;
        public float jumpBufferTime = 0.12f;

        public event Action Jumped;
        public event Action Landed;

        public bool InputEnabled { get; set; } = true;
        /// <summary>Set while aiming the sling: feet planted, no running or jumping.</summary>
        public bool AimLock { get; set; }
        public bool Grounded { get; private set; }
        public int Facing { get; private set; } = 1;
        public Vector2 Velocity => _rb.linearVelocity;
        public Rigidbody2D Body => _rb;
        public BoxCollider2D Collider => _col;
        public PlayerHealth Health { get; private set; }
        public PlayerPowers Powers { get; private set; }
        /// <summary>Last position where Brenna stood on static ground (used after falling in water).</summary>
        public Vector2 LastSafePosition { get; private set; }

        Rigidbody2D _rb;
        BoxCollider2D _col;
        float _coyote, _buffer, _controlLock, _moveInput, _safeTimer;
        bool _jumpHeld, _jumping;
        Collider2D _groundCollider;
        ContactFilter2D _groundFilter;
        readonly List<RaycastHit2D> _hits = new List<RaycastHit2D>(4);

        public static PlayerController Spawn(Transform parent, Vector2 position)
        {
            var go = new GameObject("Player");
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = position;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.55f, 0.85f);
            col.offset = new Vector2(0f, 0.43f);
            col.sharedMaterial = PhysicsMaterials.NoFriction;

            go.AddComponent<PlayerPowers>();
            var pc = go.AddComponent<PlayerController>();
            var health = go.AddComponent<PlayerHealth>();
            health.RespawnPoint = position;
            go.AddComponent<SlingAbility>();
            go.AddComponent<RootgrowAbility>();

            if (Core.IronoakConfig.Current.cutoutPlayer)
            {
                // Painted cut-out puppet, animated procedurally from the physics state.
                var rig = Characters.PuppetRig.Build(go.transform, Characters.PuppetDefinition.LoadOrDefault("Brenna"), 10);
                rig.gameObject.AddComponent<Characters.PuppetAnimator>().Init(rig, pc, health);
            }
            else
            {
                var visualGo = new GameObject("Visual");
                visualGo.transform.SetParent(go.transform, false);
                var sr = visualGo.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10;
                visualGo.AddComponent<PlayerVisual>().Init(pc, health, sr);
            }
            return pc;
        }

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
            _rb.gravityScale = gravityScale;
            _groundFilter = new ContactFilter2D { useTriggers = false };
            Health = GetComponent<PlayerHealth>();
            Powers = GetComponent<PlayerPowers>();
            LastSafePosition = transform.position;
        }

        void Start()
        {
            if (Health == null) Health = GetComponent<PlayerHealth>();
        }

        void Update()
        {
            if (!InputEnabled || AimLock)
            {
                _moveInput = 0f;
                _jumpHeld = false;
                _buffer = 0f;
                return;
            }
            if (GameInput.JumpPressed) _buffer = jumpBufferTime;
            else _buffer -= Time.deltaTime;
            _jumpHeld = GameInput.JumpHeld;
            _moveInput = GameInput.Horizontal;
        }

        public void FaceTowards(float x)
        {
            if (Mathf.Abs(x) > 0.01f) Facing = x > 0f ? 1 : -1;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            Vector2 v = _rb.linearVelocity;

            bool wasGrounded = Grounded;
            Grounded = v.y <= 0.01f + GroundVelocityY() && CheckGround();
            if (Grounded)
            {
                _coyote = coyoteTime;
                _jumping = false;
                if (!wasGrounded) Landed?.Invoke();
                TrackSafeGround(dt);
            }
            else
            {
                _coyote -= dt;
            }

            // Horizontal
            if (_controlLock > 0f)
            {
                _controlLock -= dt;
            }
            else
            {
                float target = _moveInput * maxSpeed;
                bool moving = Mathf.Abs(target) > 0.01f;
                float accel = Grounded ? (moving ? groundAccel : groundDecel) : (moving ? airAccel : airDecel);
                if (moving && Mathf.Abs(v.x) > 0.01f && Mathf.Sign(target) != Mathf.Sign(v.x)) accel *= 1.5f; // snappy turn
                v.x = Mathf.MoveTowards(v.x, target, accel * dt);
                if (moving && !AimLock) Facing = target > 0f ? 1 : -1;
            }

            // Jump (buffered + coyote)
            if (_buffer > 0f && _coyote > 0f)
            {
                v.y = jumpVelocity;
                _buffer = 0f;
                _coyote = 0f;
                _jumping = true;
                Grounded = false;
                Sfx.Play(SfxId.Jump);
                Jumped?.Invoke();
            }

            // Variable jump height
            if (_jumping && !_jumpHeld && v.y > 0f)
            {
                v.y *= jumpCutMultiplier;
                _jumping = false;
            }

            _rb.gravityScale = v.y < 0f ? gravityScale * fallGravityMultiplier : gravityScale;
            if (v.y < -maxFallSpeed) v.y = -maxFallSpeed;
            _rb.linearVelocity = v;
        }

        float GroundVelocityY()
        {
            // Standing on a rising root counts as grounded even though we're moving up.
            if (_groundCollider == null || _groundCollider.attachedRigidbody == null) return 0f;
            return Mathf.Max(0f, _groundCollider.attachedRigidbody.linearVelocity.y);
        }

        bool CheckGround()
        {
            int n = _col.Cast(Vector2.down, _groundFilter, _hits, 0.06f);
            for (int i = 0; i < n; i++)
            {
                var hit = _hits[i];
                if (hit.normal.y > 0.6f && hit.collider.GetComponent<SolidGround>() != null)
                {
                    _groundCollider = hit.collider;
                    return true;
                }
            }
            _groundCollider = null;
            return false;
        }

        void TrackSafeGround(float dt)
        {
            _safeTimer -= dt;
            if (_safeTimer > 0f || _groundCollider == null) return;
            var body = _groundCollider.attachedRigidbody;
            if (body != null && body.bodyType != RigidbodyType2D.Static) return; // not on roots/crates/bridges
            _safeTimer = 0.25f;
            LastSafePosition = _rb.position;
        }

        /// <summary>Upward bounce after stomping an enemy. Holding jump gives a higher bounce.</summary>
        public void Bounce(float velocity)
        {
            var v = _rb.linearVelocity;
            v.y = velocity;
            _rb.linearVelocity = v;
            _jumping = true;
            Grounded = false;
        }

        public void Knockback(float direction)
        {
            _rb.linearVelocity = new Vector2(direction * 6f, 8f);
            _controlLock = 0.25f;
            _jumping = false;
        }

        public void Teleport(Vector2 position)
        {
            _rb.position = position;
            transform.position = position;
            _rb.linearVelocity = Vector2.zero;
            _controlLock = 0f;
        }
    }
}
