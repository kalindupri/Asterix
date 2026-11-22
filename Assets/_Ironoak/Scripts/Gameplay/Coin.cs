using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>Spinning Celtic gold coin.</summary>
    public class Coin : MonoBehaviour
    {
        Sprite[] _frames;
        bool _painted;
        SpriteRenderer _sr;
        Vector3 _base;
        float _t;
        bool _taken;

        public static Coin Spawn(Transform parent, Vector2 position)
        {
            var go = new GameObject("Coin");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;
            var coin = go.AddComponent<Coin>();
            coin._sr = sr;
            return coin;
        }

        void Awake() => _sr = GetComponent<SpriteRenderer>();

        void Start()
        {
            _frames = SpriteLibrary.GetFrames("coin_0", "coin_1", "coin_2", "coin_3");
            _painted = SpriteLibrary.IsPainted(_frames[0]);
            if (_painted) _sr.sprite = _frames[0];
            _base = transform.position;
            _t = _base.x * 0.37f; // de-sync neighbouring coins
        }

        void Update()
        {
            _t += Time.deltaTime;
            float bob;
            if (_painted)
            {
                // One painted coin, spun by squashing; the back face is a little darker.
                float c = Mathf.Cos(_t * 4f);
                transform.localScale = new Vector3(Mathf.Max(0.08f, Mathf.Abs(c)), 1f, 1f); // y stays 1 so the trigger radius is unchanged
                float shade = c >= 0f ? 1f : 0.78f;
                _sr.color = new Color(shade, shade, shade);
                bob = Mathf.Sin(_t * 3f) * 0.09f;
            }
            else
            {
                _sr.sprite = _frames[(int)(_t * 8f) % 4];
                bob = Mathf.Round(Mathf.Sin(_t * 3f) * 1.5f) / PixelArt.PPU;
            }
            transform.position = _base + new Vector3(0f, bob, 0f);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_taken || other.GetComponent<PlayerController>() == null) return;
            _taken = true;
            Sfx.Play(SfxId.Coin);
            GameEvents.RaiseCoinCollected();
            Destroy(gameObject);
        }
    }
}
