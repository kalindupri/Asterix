using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    public enum CollectibleType { OghamStone, GoldenTorc, ShrineShard, DruidMark }

    /// <summary>Secret rewards: lore stones, golden torcs, shrine shards and the Druid's Mark.</summary>
    public class Collectible : MonoBehaviour
    {
        public CollectibleType type;
        public string displayName = "";

        SpriteRenderer _sr;
        Vector3 _base;
        float _t;
        bool _taken;

        public static string SpriteFor(CollectibleType t)
        {
            switch (t)
            {
                case CollectibleType.OghamStone: return "item_ogham";
                case CollectibleType.GoldenTorc: return "item_golden_torc";
                case CollectibleType.ShrineShard: return "item_shard";
                default: return "item_druid_mark";
            }
        }

        public static string TypeName(CollectibleType t)
        {
            switch (t)
            {
                case CollectibleType.OghamStone: return "OGHAM STONE";
                case CollectibleType.GoldenTorc: return "GOLDEN TORC";
                case CollectibleType.ShrineShard: return "SHRINE SHARD";
                default: return "DRUID'S MARK";
            }
        }

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null && _sr.sprite == null) _sr.sprite = SpriteLibrary.Get(SpriteFor(type));
        }

        void Start()
        {
            _base = transform.position;
            PickupGlow.Attach(_sr, GlowColor(type));
        }

        static Color GlowColor(CollectibleType t)
        {
            switch (t)
            {
                case CollectibleType.OghamStone: return new Color(0.7f, 0.85f, 1f);
                case CollectibleType.GoldenTorc: return new Color(1f, 0.85f, 0.45f);
                case CollectibleType.ShrineShard: return new Color(0.65f, 0.85f, 1f);
                default: return new Color(0.85f, 0.6f, 1f);
            }
        }

        void Update()
        {
            _t += Time.deltaTime;
            float bob = Mathf.Sin(_t * 2.5f) * 2f / PixelArt.PPU;
            if (IronoakConfig.PixelPerfect) bob = Mathf.Round(bob * PixelArt.PPU) / PixelArt.PPU;
            transform.position = _base + new Vector3(0f, bob, 0f);
            if ((int)(_t * 3f) % 4 == 0 && Random.value < 0.05f)
                Fx.Burst(transform.position + (Vector3)Random.insideUnitCircle * 0.4f, new Color(1f, 0.95f, 0.6f), 1, 0.5f, 0.5f, -0.5f);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_taken || other.attachedRigidbody == null || other.attachedRigidbody.GetComponent<PlayerController>() == null) return;
            _taken = true;
            Sfx.Play(SfxId.Collect);
            Fx.Burst(transform.position, new Color(1f, 0.9f, 0.4f), 14, 4f, 0.6f, 2f);
            GameEvents.RaiseCollectibleFound(this);
            Destroy(gameObject);
        }
    }
}
