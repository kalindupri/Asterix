using System.Collections.Generic;
using Ironoak.Art;
using Ironoak.Core;
using Ironoak.Puzzles;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>
    /// Set piece: a wild boar stampede sweeps in from the left once Brenna enters the trigger.
    /// The herd front advances at a fixed speed and tramples everything behind it; getting
    /// caught costs a heart and restarts the chase. It stops at <see cref="frontEndX"/>.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class StampedeChase : MonoBehaviour
    {
        public float frontStartX;
        public float frontEndX;
        public float speed = 5.4f;
        [Tooltip("Max distance the herd may fall behind the player (keeps the pressure on).")]
        public float maxGap = 13f;
        public Transform restartPoint;
        public SapKnot[] knotsToReset = new SapKnot[0];
        public int boarCount = 5;

        enum State { Waiting, Running, Resetting, Done }

        State _state = State.Waiting;
        float _frontX, _resetTimer, _anim, _rumbleTimer;
        PlayerController _player;
        readonly List<SpriteRenderer> _boars = new List<SpriteRenderer>();
        Sprite _run1, _run2;

        public bool Running => _state == State.Running;
        public bool Done => _state == State.Done;
        public float FrontX => _frontX;

        void Awake()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
            _run1 = SpriteLibrary.Get("boar_run1");
            _run2 = SpriteLibrary.Get("boar_run2");
            for (int i = 0; i < boarCount; i++)
            {
                var go = new GameObject("Boar");
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = _run1;
                sr.sortingOrder = 9 - (i % 2);
                sr.enabled = false;
                _boars.Add(sr);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (_state != State.Waiting || other.attachedRigidbody == null) return;
            var p = other.attachedRigidbody.GetComponent<PlayerController>();
            if (p == null) return;
            _player = p;
            Begin();
        }

        void Begin()
        {
            _state = State.Running;
            _frontX = frontStartX;
            Sfx.Play(SfxId.Rumble);
            GameEvents.RaiseMessage("STAMPEDE! RUN!");
            foreach (var b in _boars) b.enabled = true;
        }

        void Update()
        {
            if (_state == State.Waiting || _state == State.Done) return;
            float dt = Time.deltaTime;
            _anim += dt;

            if (_state == State.Resetting)
            {
                _resetTimer -= dt;
                if (_resetTimer <= 0f) Restart();
                PlaceBoars();
                return;
            }

            if (_player == null || _player.Health.Dead) return;

            _frontX += speed * dt;
            _frontX = Mathf.Max(_frontX, _player.transform.position.x - maxGap);
            if (_frontX >= frontEndX)
            {
                _frontX = frontEndX;
                Finish();
                return;
            }

            _rumbleTimer -= dt;
            if (_rumbleTimer <= 0f)
            {
                _rumbleTimer = 1.1f;
                Sfx.Play(SfxId.Rumble, 0.6f);
            }
            CameraRig.Instance?.Shake(0.035f, 0.1f);
            PlaceBoars();

            if (_player.transform.position.x < _frontX - 0.2f) Caught();
        }

        void PlaceBoars()
        {
            for (int i = 0; i < _boars.Count; i++)
            {
                float x = _frontX - 0.6f - i * 1.1f - (i % 2) * 0.5f;
                var sr = _boars[i];
                sr.transform.position = new Vector3(x, GroundY(x), 0f);
                sr.sprite = ((int)(_anim * 10f + i) & 1) == 0 ? _run1 : _run2;
                if (Random.value < 0.15f) Fx.Dust(new Vector2(x - 0.5f, GroundY(x)));
            }
        }

        static readonly List<RaycastHit2D> GroundHits = new List<RaycastHit2D>(4);

        static float GroundY(float x)
        {
            var filter = new ContactFilter2D { useTriggers = false };
            int n = Physics2D.Raycast(new Vector2(x, 40f), Vector2.down, filter, GroundHits, 60f);
            for (int i = 0; i < n; i++)
                if (GroundHits[i].collider.GetComponent<SolidGround>() != null) return GroundHits[i].point.y;
            return 0f;
        }

        void Caught()
        {
            _player.Health.Damage(new Vector2(_frontX - 2f, _player.transform.position.y));
            if (_player.Health.Dead) return;
            _state = State.Resetting;
            _resetTimer = 0.9f;
        }

        void Restart()
        {
            foreach (var k in knotsToReset) if (k != null) k.ResetKnot();
            if (restartPoint != null) _player.Teleport(restartPoint.position);
            _frontX = frontStartX;
            _state = State.Running;
            Sfx.Play(SfxId.Rumble);
        }

        void Finish()
        {
            _state = State.Done;
            Sfx.Play(SfxId.Break);
            CameraRig.Instance?.Shake(0.15f, 0.5f);
            foreach (var b in _boars)
            {
                Fx.Burst(b.transform.position + Vector3.up * 0.5f, new Color(0.5f, 0.35f, 0.25f), 6, 4f, 0.6f);
                b.enabled = false;
            }
            GameEvents.RaiseMessage("THE HERD CRASHES AWAY");
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(frontStartX, 0f), new Vector3(frontStartX, 12f));
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(frontEndX, 0f), new Vector3(frontEndX, 12f));
        }
    }
}
