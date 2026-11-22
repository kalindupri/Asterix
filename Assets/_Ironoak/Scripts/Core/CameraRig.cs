using Ironoak.Art;
using UnityEngine;

namespace Ironoak.Core
{
    /// <summary>
    /// Follows the player with look-ahead and smoothing, stays inside the level bounds,
    /// and snaps to the pixel grid. Can also slowly auto-pan for the title screen.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class CameraRig : MonoBehaviour
    {
        public float smoothTime = 0.12f;
        public float lookAheadDistance = 2.5f;
        public float verticalOffset = 1.5f;

        Camera _cam;
        Rigidbody2D _target;
        Rect _bounds;
        Vector2 _pos, _vel;
        float _lookAhead;
        bool _autoPan;
        float _panSpeed, _panDir = 1f;

        void Awake() => _cam = GetComponent<Camera>();

        float HalfH => _cam.orthographicSize;
        float HalfW => _cam.orthographicSize * PixelDisplay.Width / PixelDisplay.Height;

        public void Follow(Rigidbody2D target, Rect bounds)
        {
            _target = target;
            _bounds = bounds;
            _autoPan = false;
            _lookAhead = 0f;
            _vel = Vector2.zero;
            _pos = Clamp(target.position + Vector2.up * verticalOffset);
            Apply();
        }

        public void AutoPan(Rect bounds, float speed)
        {
            _target = null;
            _bounds = bounds;
            _autoPan = true;
            _panSpeed = speed;
            _panDir = 1f;
            _pos = Clamp(new Vector2(bounds.xMin, bounds.yMin));
            Apply();
        }

        void LateUpdate()
        {
            if (_cam == null) return;
            float dt = Time.deltaTime;

            if (_target != null)
            {
                float desiredLook = Mathf.Clamp(_target.linearVelocity.x * 0.4f, -lookAheadDistance, lookAheadDistance);
                _lookAhead = Mathf.Lerp(_lookAhead, desiredLook, dt * 3f);
                var desired = new Vector2(_target.position.x + _lookAhead, _target.position.y + verticalOffset);
                _pos = Vector2.SmoothDamp(_pos, Clamp(desired), ref _vel, smoothTime, Mathf.Infinity, dt);
            }
            else if (_autoPan)
            {
                _pos.x += _panSpeed * _panDir * Time.unscaledDeltaTime;
                float min = _bounds.xMin + HalfW, max = _bounds.xMax - HalfW;
                if (_pos.x >= max) { _pos.x = max; _panDir = -1f; }
                if (_pos.x <= min) { _pos.x = min; _panDir = 1f; }
                _pos = Clamp(_pos);
            }

            Apply();
        }

        Vector2 Clamp(Vector2 p)
        {
            float minX = _bounds.xMin + HalfW, maxX = _bounds.xMax - HalfW;
            p.x = minX > maxX ? _bounds.center.x : Mathf.Clamp(p.x, minX, maxX);
            float minY = _bounds.yMin + HalfH, maxY = Mathf.Max(minY, _bounds.yMax - HalfH);
            p.y = Mathf.Clamp(p.y, minY, maxY);
            return p;
        }

        void Apply()
        {
            float ppu = PixelArt.PPU;
            transform.position = new Vector3(Mathf.Round(_pos.x * ppu) / ppu, Mathf.Round(_pos.y * ppu) / ppu, -10f);
        }
    }
}
