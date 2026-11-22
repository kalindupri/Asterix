using System.Collections.Generic;
using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Puzzles;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>
    /// Patrolling legionary. Turns at walls and ledges, hurts the player on contact,
    /// is stunned by sling stones and defeated by being stomped from above.
    /// Expects a solid BoxCollider2D (body) and a trigger BoxCollider2D (hitbox).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class RomanSoldier : MonoBehaviour, IHittable
    {
        public float speed = 1.6f;
        public float stompBounce = 12f;
        public float stunTime = 2.5f;

        Rigidbody2D _rb;
        Collider2D _body;
        SpriteRenderer _sr;
        Sprite _walk1, _walk2, _squash;
        int _dir = -1;
        float _animTime, _stun;
        bool _dead;
        ContactFilter2D _filter;
        static readonly List<RaycastHit2D> Hits = new List<RaycastHit2D>(4);

        public bool Stunned => _stun > 0f;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _sr = GetComponentInChildren<SpriteRenderer>();
            foreach (var c in GetComponents<Collider2D>())
            {
                if (c.isTrigger) continue;
                _body = c;
                if (c.sharedMaterial == null) c.sharedMaterial = PhysicsMaterials.NoFriction;
            }
            _filter = new ContactFilter2D { useTriggers = false };
            _walk1 = SpriteLibrary.Get("roman_walk1");
            _walk2 = SpriteLibrary.Get("roman_walk2");
            _squash = SpriteLibrary.Get("roman_squash");
        }

        /// <summary>Player and soldier never physically push each other; contact is handled by the trigger.</summary>
        public void IgnorePlayer(Collider2D playerCollider)
        {
            if (_body != null && playerCollider != null) Physics2D.IgnoreCollision(_body, playerCollider, true);
        }

        void FixedUpdate()
        {
            if (_dead) return;
            var v = _rb.linearVelocity;

            if (_stun > 0f)
            {
                _stun -= Time.fixedDeltaTime;
                v.x = 0f;
                _rb.linearVelocity = v;
                return;
            }

            Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.4f);
            Vector2 forward = new Vector2(_dir, 0f);
            if (HitsSolid(origin, forward, 0.45f) || !HitsSolid(origin + forward * 0.45f, Vector2.down, 0.6f))
                _dir = -_dir;

            v.x = _dir * speed;
            _rb.linearVelocity = v;

            if (transform.position.y < PlayerHealth.KillY - 1f) Destroy(gameObject);
        }

        void Update()
        {
            if (_dead || _sr == null) return;
            if (_stun > 0f)
            {
                _sr.sprite = _walk1;
                _sr.color = ((int)(Time.time * 8f) & 1) == 0 ? new Color(1f, 1f, 0.6f) : Color.white;
                return;
            }
            _sr.color = Color.white;
            _animTime += Time.deltaTime;
            _sr.sprite = ((int)(_animTime * 6f) & 1) == 0 ? _walk1 : _walk2;
            _sr.flipX = _dir < 0;
        }

        bool HitsSolid(Vector2 origin, Vector2 direction, float distance)
        {
            int n = Physics2D.Raycast(origin, direction, _filter, Hits, distance);
            for (int i = 0; i < n; i++)
            {
                var c = Hits[i].collider;
                if (c == _body) continue;
                if (c.GetComponent<SolidGround>() != null) return true;
                if (direction.y == 0f && c.GetComponent<RomanSoldier>() != null) return true;
            }
            return false;
        }

        public void OnSlingHit(Vector2 point, Vector2 direction)
        {
            if (_dead) return;
            _stun = stunTime;
            Sfx.Play(SfxId.Hit);
            Fx.Burst(transform.position + Vector3.up * 1.2f, new Color(1f, 0.95f, 0.5f), 6, 1.5f, 0.6f, 0f);
        }

        void OnTriggerStay2D(Collider2D other)
        {
            if (_dead || other.attachedRigidbody == null) return;
            var player = other.attachedRigidbody.GetComponent<PlayerController>();
            if (player == null || player.Health == null || player.Health.Dead) return;

            bool stomp = player.Velocity.y < 0.5f && other.bounds.min.y > transform.position.y + 0.45f;
            if (stomp)
            {
                Defeat();
                player.Bounce(stompBounce);
            }
            else if (_stun <= 0f)
            {
                player.Health.Damage(transform.position);
            }
        }

        void Defeat()
        {
            _dead = true;
            if (_sr != null)
            {
                _sr.sprite = _squash;
                _sr.color = Color.white;
            }
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Kinematic;
            foreach (var c in GetComponents<Collider2D>()) c.enabled = false;
            Sfx.Play(SfxId.Stomp);
            GameEvents.RaiseEnemyDefeated();
            Destroy(gameObject, 0.6f);
        }
    }
}
