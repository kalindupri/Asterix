using System.Collections.Generic;
using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>
    /// Patrolling legionary. Turns at walls and ledges, hurts the player on contact,
    /// and is defeated by being stomped from above.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class RomanSoldier : MonoBehaviour
    {
        public float speed = 1.6f;
        public float stompBounce = 12f;

        Rigidbody2D _rb;
        BoxCollider2D _body;
        SpriteRenderer _sr;
        Sprite _walk1, _walk2, _squash;
        int _dir = -1;
        float _animTime;
        bool _dead;
        ContactFilter2D _filter;
        static readonly List<RaycastHit2D> Hits = new List<RaycastHit2D>(4);

        public static RomanSoldier Spawn(Transform parent, Vector2 position)
        {
            var go = new GameObject("RomanSoldier");
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.gravityScale = 3f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            var body = go.AddComponent<BoxCollider2D>();
            body.size = new Vector2(0.6f, 0.88f);
            body.offset = new Vector2(0f, 0.44f);
            body.sharedMaterial = new PhysicsMaterial2D("EnemyNoFriction") { friction = 0f };

            var hitbox = go.AddComponent<BoxCollider2D>();
            hitbox.isTrigger = true;
            hitbox.size = new Vector2(0.75f, 0.95f);
            hitbox.offset = new Vector2(0f, 0.47f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 8;

            var soldier = go.AddComponent<RomanSoldier>();
            soldier._body = body;
            soldier._sr = sr;
            return soldier;
        }

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
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

            Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.4f);
            Vector2 forward = new Vector2(_dir, 0f);
            if (HitsSolid(origin, forward, 0.45f) || !HitsSolid(origin + forward * 0.45f, Vector2.down, 0.6f))
                _dir = -_dir;

            var v = _rb.linearVelocity;
            v.x = _dir * speed;
            _rb.linearVelocity = v;

            if (transform.position.y < PlayerHealth.KillY - 1f) Destroy(gameObject);
        }

        void Update()
        {
            if (_dead) return;
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

        void OnTriggerStay2D(Collider2D other)
        {
            if (_dead) return;
            var player = other.GetComponent<PlayerController>();
            if (player == null || player.Health.Dead) return;

            bool stomp = player.Velocity.y < 0.5f && other.bounds.min.y > transform.position.y + 0.45f;
            if (stomp)
            {
                Defeat();
                player.Bounce(stompBounce);
            }
            else
            {
                player.Health.Damage(transform.position);
            }
        }

        void Defeat()
        {
            _dead = true;
            _sr.sprite = _squash;
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Kinematic;
            foreach (var c in GetComponents<Collider2D>()) c.enabled = false;
            Sfx.Play(SfxId.Stomp);
            GameEvents.RaiseEnemyDefeated();
            Destroy(gameObject, 0.6f);
        }
    }
}
