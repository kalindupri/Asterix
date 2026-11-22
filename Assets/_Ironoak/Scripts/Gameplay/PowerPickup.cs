using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Powers;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>Unlocks one of Brenna's powers (the Heartwood Torc unlocks Rootgrow).</summary>
    public class PowerPickup : MonoBehaviour
    {
        public Power power = Power.Rootgrow;
        public string itemName = "HEARTWOOD TORC";

        Vector3 _base;
        float _t;
        bool _taken;

        void Start() => _base = transform.position;

        void Update()
        {
            _t += Time.deltaTime;
            transform.position = _base + new Vector3(0f, Mathf.Round(Mathf.Sin(_t * 2f) * 3f) / PixelArt.PPU, 0f);
            if (Random.value < 0.08f)
                Fx.Burst(transform.position + (Vector3)Random.insideUnitCircle * 0.5f, new Color(0.7f, 1f, 0.5f), 1, 0.6f, 0.6f, -0.6f);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_taken || other.attachedRigidbody == null) return;
            var powers = other.attachedRigidbody.GetComponent<PlayerPowers>();
            if (powers == null) return;
            _taken = true;
            powers.Unlock(power);
            Sfx.Play(SfxId.PowerUp);
            CameraRig.Instance?.Shake(0.05f, 0.4f);
            Fx.Burst(transform.position, new Color(0.7f, 1f, 0.5f), 24, 5f, 0.9f, 1f);
            GameEvents.RaisePowerUnlocked(power);
            Destroy(gameObject);
        }
    }
}
