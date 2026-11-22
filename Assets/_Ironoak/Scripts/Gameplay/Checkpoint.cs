using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>Torch checkpoint. Lights up and becomes the respawn point.</summary>
    public class Checkpoint : MonoBehaviour
    {
        SpriteRenderer _sr;
        Sprite _off, _on1, _on2;
        bool _active;
        float _t;

        public static Checkpoint Spawn(Transform parent, Vector2 position)
        {
            var go = new GameObject("Checkpoint");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.8f, 2f);
            col.offset = new Vector2(0f, 1f);
            var cp = go.AddComponent<Checkpoint>();
            cp._sr = sr;
            return cp;
        }

        void Start()
        {
            _off = SpriteLibrary.Get("torch_off");
            _on1 = SpriteLibrary.Get("torch_on1");
            _on2 = SpriteLibrary.Get("torch_on2");
            _sr.sprite = _off;
        }

        void Update()
        {
            if (!_active) return;
            _t += Time.deltaTime;
            _sr.sprite = ((int)(_t * 8f) & 1) == 0 ? _on1 : _on2;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_active) return;
            var player = other.GetComponent<PlayerController>();
            if (player == null) return;
            _active = true;
            player.Health.RespawnPoint = transform.position;
            Sfx.Play(SfxId.Checkpoint);
            GameEvents.RaiseCheckpointReached();
        }
    }
}
