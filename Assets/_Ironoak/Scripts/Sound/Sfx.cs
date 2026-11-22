using System.Collections.Generic;
using UnityEngine;

namespace Ironoak.Sound
{
    public enum SfxId
    {
        Jump, Coin, Stomp, Hurt, Select, Confirm, Checkpoint, Win, Lose,
        Sling, Hit, Break, Bell, RootGrow, RootRetract, Gate, Thud, Splash, Secret, Collect, PowerUp, Rumble
    }

    /// <summary>
    /// Placeholder chiptune sound effects synthesised at startup, so the POC has audio
    /// feedback with zero asset dependencies. Swap for authored clips in M4.
    /// </summary>
    public class Sfx : MonoBehaviour
    {
        enum Wave { Square, Triangle, Saw, Noise }

        const int Rate = 44100;
        static Sfx _instance;

        AudioSource _source;
        readonly Dictionary<SfxId, AudioClip> _clips = new Dictionary<SfxId, AudioClip>();

        public static void Play(SfxId id, float volume = 1f)
        {
            if (_instance != null && _instance._clips.TryGetValue(id, out var clip))
                _instance._source.PlayOneShot(clip, volume);
        }

        void Awake()
        {
            _instance = this;
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.volume = 0.35f;

            _clips[SfxId.Jump] = Sweep("sfx_jump", 0.14f, 280f, 640f, Wave.Square, 0.35f);
            _clips[SfxId.Coin] = Notes("sfx_coin", new[] { 988f, 1319f }, 0.06f, Wave.Square, 0.3f);
            _clips[SfxId.Stomp] = Sweep("sfx_stomp", 0.12f, 420f, 110f, Wave.Square, 0.4f);
            _clips[SfxId.Hurt] = Sweep("sfx_hurt", 0.28f, 320f, 70f, Wave.Saw, 0.4f);
            _clips[SfxId.Select] = Notes("sfx_select", new[] { 660f }, 0.035f, Wave.Square, 0.25f);
            _clips[SfxId.Confirm] = Notes("sfx_confirm", new[] { 523f, 784f }, 0.06f, Wave.Square, 0.3f);
            _clips[SfxId.Checkpoint] = Notes("sfx_checkpoint", new[] { 523f, 659f, 784f }, 0.08f, Wave.Triangle, 0.45f);
            _clips[SfxId.Win] = Notes("sfx_win", new[] { 523f, 659f, 784f, 1047f, 784f, 1047f }, 0.11f, Wave.Square, 0.3f);
            _clips[SfxId.Lose] = Notes("sfx_lose", new[] { 392f, 330f, 262f, 196f }, 0.16f, Wave.Triangle, 0.45f);

            _clips[SfxId.Sling] = Sweep("sfx_sling", 0.12f, 900f, 300f, Wave.Noise, 0.25f);
            _clips[SfxId.Hit] = Sweep("sfx_hit", 0.05f, 600f, 300f, Wave.Square, 0.3f);
            _clips[SfxId.Break] = Sweep("sfx_break", 0.3f, 400f, 80f, Wave.Noise, 0.45f);
            _clips[SfxId.Bell] = Notes("sfx_bell", new[] { 1319f, 1568f }, 0.22f, Wave.Triangle, 0.45f);
            _clips[SfxId.RootGrow] = Sweep("sfx_rootgrow", 0.35f, 90f, 260f, Wave.Saw, 0.3f);
            _clips[SfxId.RootRetract] = Sweep("sfx_rootretract", 0.3f, 240f, 80f, Wave.Saw, 0.25f);
            _clips[SfxId.Gate] = Sweep("sfx_gate", 0.5f, 140f, 90f, Wave.Square, 0.25f);
            _clips[SfxId.Thud] = Sweep("sfx_thud", 0.18f, 160f, 40f, Wave.Triangle, 0.6f);
            _clips[SfxId.Splash] = Sweep("sfx_splash", 0.35f, 700f, 200f, Wave.Noise, 0.3f);
            _clips[SfxId.Secret] = Notes("sfx_secret", new[] { 523f, 659f, 784f, 1047f, 1319f }, 0.07f, Wave.Triangle, 0.45f);
            _clips[SfxId.Collect] = Notes("sfx_collect", new[] { 784f, 988f, 1175f, 1568f }, 0.07f, Wave.Square, 0.3f);
            _clips[SfxId.PowerUp] = Notes("sfx_powerup", new[] { 392f, 523f, 659f, 784f, 1047f, 1319f, 1568f }, 0.09f, Wave.Triangle, 0.5f);
            _clips[SfxId.Rumble] = Sweep("sfx_rumble", 1.2f, 70f, 40f, Wave.Noise, 0.35f);
        }

        void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        static float Osc(Wave w, double phase, System.Random rng)
        {
            float p = (float)(phase - System.Math.Floor(phase));
            switch (w)
            {
                case Wave.Square: return p < 0.5f ? 1f : -1f;
                case Wave.Triangle: return 4f * Mathf.Abs(p - 0.5f) - 1f;
                case Wave.Saw: return 2f * p - 1f;
                default: return (float)(rng.NextDouble() * 2.0 - 1.0);
            }
        }

        static AudioClip Sweep(string name, float duration, float f0, float f1, Wave wave, float volume)
        {
            int n = Mathf.CeilToInt(duration * Rate);
            var data = new float[n];
            var rng = new System.Random(1);
            double phase = 0;
            float held = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / n;
                float f = Mathf.Lerp(f0, f1, t);
                phase += f / Rate;
                float s;
                if (wave == Wave.Noise)
                {
                    // sample-and-hold noise: the hold rate follows the sweep, giving pitch to the noise
                    if (i % Mathf.Max(1, (int)(Rate / (f * 4f))) == 0) held = Osc(Wave.Noise, phase, rng);
                    s = held;
                }
                else s = Osc(wave, phase, rng);
                data[i] = s * (1f - t) * volume;
            }
            var clip = AudioClip.Create(name, n, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Notes(string name, float[] freqs, float noteDuration, Wave wave, float volume)
        {
            int per = Mathf.CeilToInt(noteDuration * Rate);
            var data = new float[per * freqs.Length];
            var rng = new System.Random(1);
            for (int k = 0; k < freqs.Length; k++)
            {
                double phase = 0;
                for (int i = 0; i < per; i++)
                {
                    float t = (float)i / per;
                    phase += freqs[k] / Rate;
                    data[k * per + i] = Osc(wave, phase, rng) * (1f - 0.7f * t) * volume;
                }
            }
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
