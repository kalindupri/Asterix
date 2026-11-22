using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>
    /// A hidden chamber. Entering it counts the secret once and fades away its cover
    /// (the false rock/leaves drawn over the room).
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class SecretArea : MonoBehaviour
    {
        public string secretName = "SECRET";
        [Tooltip("The Druid's Trial room of the level (tracked separately from secrets).")]
        public bool isTrial;
        public SpriteRenderer[] covers = new SpriteRenderer[0];

        bool _found;
        float _fade = 1f;

        public bool Found => _found;

        void Reset() => GetComponent<BoxCollider2D>().isTrigger = true;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_found || other.attachedRigidbody == null || other.attachedRigidbody.GetComponent<PlayerController>() == null) return;
            _found = true;
            Sfx.Play(SfxId.Secret);
            GameEvents.RaiseSecretFound(this);
        }

        void Update()
        {
            if (!_found || _fade <= 0f) return;
            _fade = Mathf.Max(0f, _fade - Time.deltaTime * 2.5f);
            foreach (var c in covers)
            {
                if (c == null) continue;
                var col = c.color;
                col.a = _fade;
                c.color = col;
            }
        }

        void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null) return;
            Gizmos.color = isTrial ? new Color(0.6f, 0.3f, 1f, 0.25f) : new Color(1f, 0.85f, 0.2f, 0.25f);
            Gizmos.DrawCube(transform.position + (Vector3)box.offset, box.size);
        }
    }
}
