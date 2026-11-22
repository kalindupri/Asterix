using System;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>Hearts, invulnerability frames, pit falls, respawn and death.</summary>
    public class PlayerHealth : MonoBehaviour
    {
        public const float KillY = -2.5f;

        public int maxHealth = 3;
        public float invulnerableTime = 1.2f;

        public event Action<int> HealthChanged;
        public event Action Died;

        public int Health { get; private set; }
        public bool Dead { get; private set; }
        public bool Invulnerable => _invulnerable > 0f;
        public Vector2 RespawnPoint { get; set; }

        PlayerController _pc;
        float _invulnerable;

        void Awake()
        {
            _pc = GetComponent<PlayerController>();
            Health = maxHealth;
        }

        void Update()
        {
            if (_invulnerable > 0f) _invulnerable -= Time.deltaTime;
            if (!Dead && transform.position.y < KillY) FellOut();
        }

        public void Damage(Vector2 sourcePosition)
        {
            if (Dead || _invulnerable > 0f) return;
            if (!LoseHeart()) return;

            _invulnerable = invulnerableTime;
            float dir = transform.position.x - sourcePosition.x;
            _pc.Knockback(Mathf.Abs(dir) < 0.05f ? -_pc.Facing : Mathf.Sign(dir));
        }

        /// <summary>Fell into deep water: back to the last safe spot, no damage.</summary>
        public void ReturnToSafeGround()
        {
            if (Dead) return;
            _pc.Teleport(_pc.LastSafePosition + Vector2.up * 0.05f);
            _invulnerable = Mathf.Max(_invulnerable, 0.6f);
        }

        void FellOut()
        {
            if (!LoseHeart()) return;
            _pc.Teleport(RespawnPoint);
            _invulnerable = invulnerableTime;
        }

        /// <summary>Removes a heart. Returns false if the player died.</summary>
        bool LoseHeart()
        {
            Health = Mathf.Max(0, Health - 1);
            HealthChanged?.Invoke(Health);
            Sfx.Play(SfxId.Hurt);
            if (Health > 0) return true;
            Die();
            return false;
        }

        void Die()
        {
            Dead = true;
            _pc.InputEnabled = false;
            _pc.Collider.enabled = false;
            _pc.Body.linearVelocity = new Vector2(0f, 12f);
            Sfx.Play(SfxId.Lose);
            Died?.Invoke();
        }
    }
}
