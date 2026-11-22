using Ironoak.Art;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>Frame animation, facing, squash &amp; stretch and damage blink for the player sprite.</summary>
    public class PlayerVisual : MonoBehaviour
    {
        SpriteRenderer _sr;
        PlayerController _pc;
        PlayerHealth _hp;
        Sprite _idle, _run1, _run2, _jump;
        float _runTime;
        Vector3 _scale = Vector3.one;

        public void Init(PlayerController pc, PlayerHealth hp, SpriteRenderer sr)
        {
            _pc = pc;
            _hp = hp;
            _sr = sr;
            _idle = SpriteLibrary.Get("player_idle");
            _run1 = SpriteLibrary.Get("player_run1");
            _run2 = SpriteLibrary.Get("player_run2");
            _jump = SpriteLibrary.Get("player_jump");
            _sr.sprite = _idle;
            pc.Jumped += () => _scale = new Vector3(0.8f, 1.2f, 1f);
            pc.Landed += () => _scale = new Vector3(1.25f, 0.8f, 1f);
        }

        void LateUpdate()
        {
            if (_pc == null) return;
            Vector2 v = _pc.Velocity;
            Sprite frame;

            if (_hp.Dead)
            {
                frame = _jump;
                _sr.flipY = true;
            }
            else if (!_pc.Grounded)
            {
                frame = _jump;
            }
            else if (Mathf.Abs(v.x) > 0.3f)
            {
                _runTime += Time.deltaTime * Mathf.Abs(v.x) * 1.6f;
                int f = (int)_runTime % 4;
                frame = f == 0 ? _run1 : f == 2 ? _run2 : _idle;
            }
            else
            {
                _runTime = 0f;
                frame = _idle;
            }

            _sr.sprite = frame;
            _sr.flipX = _pc.Facing < 0;

            _scale = Vector3.Lerp(_scale, Vector3.one, Time.deltaTime * 12f);
            transform.localScale = _scale;

            _sr.enabled = _hp.Dead || !_hp.Invulnerable || ((int)(Time.time * 14f) % 2 == 0);
        }
    }
}
