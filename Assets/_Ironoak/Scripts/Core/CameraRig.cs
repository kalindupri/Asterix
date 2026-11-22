using Ironoak.Art;
using UnityEngine;

namespace Ironoak.Core
{
    /// <summary>
    /// Follows the player with look-ahead and smoothing, stays inside the level bounds
    /// (or the active camera zone), zooms per zone (Native mode), shakes on impacts, and snaps
    /// to the pixel grid in PixelPerfect mode.
    /// Can also slowly auto-pan for the title screen.
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class CameraRig : MonoBehaviour
    {
        public static CameraRig Instance { get; private set; }

        public float smoothTime = 0.12f;
        public float lookAheadDistance = 2.5f;
        public float verticalOffset = 1.5f;
        public float zoomTime = 0.6f;

        Camera _cam;
        Rigidbody2D _target;
        Rect _bounds;
        Rect? _zone;
        float _zoneHalf, _zoomVel;
        Vector2 _pos, _vel;
        float _lookAhead;
        bool _autoPan;
        float _panSpeed, _panDir = 1f;
        float _shakeTime, _shakeAmount;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            Instance = this;
        }

        float HalfH => _cam.orthographicSize;
        float BaseHalf => IronoakConfig.DefaultHalfHeight;
        float HalfW => _cam.orthographicSize * _cam.aspect;

        public void Follow(Rigidbody2D target, Rect bounds)
        {
            _target = target;
            _bounds = bounds;
            _zone = null;
            _autoPan = false;
            _cam.orthographicSize = BaseHalf;
            _zoomVel = 0f;
            _lookAhead = 0f;
            _vel = Vector2.zero;
            _pos = Clamp(target.position + Vector2.up * verticalOffset);
            Apply(Vector2.zero);
        }

        public void AutoPan(Rect bounds, float speed)
        {
            _target = null;
            _bounds = bounds;
            _zone = null;
            _cam.orthographicSize = BaseHalf;
            _autoPan = true;
            _panSpeed = speed;
            _panDir = 1f;
            _pos = Clamp(new Vector2(bounds.xMin, bounds.yMin));
            Apply(Vector2.zero);
        }

        /// <summary>
        /// Frames a puzzle room: the camera is confined to this rect while it is set. In Native mode
        /// it also zooms: <paramref name="viewHeight"/> &gt; 0 sets the view height, 0 zooms out just
        /// enough to show the whole zone, and a negative value keeps the default zoom.
        /// </summary>
        public void SetZone(Rect zone, float viewHeight = 0f)
        {
            _zone = zone;
            float baseHalf = BaseHalf;
            if (IronoakConfig.PixelPerfect || viewHeight < 0f) _zoneHalf = baseHalf;
            else if (viewHeight > 0f) _zoneHalf = viewHeight * 0.5f;
            else _zoneHalf = Mathf.Max(zone.height * 0.5f, zone.width * 0.5f / Mathf.Max(0.1f, _cam.aspect));
            float maxHalf = IronoakConfig.Current.maxZoneViewHeight * 0.5f;
            _zoneHalf = Mathf.Clamp(_zoneHalf, baseHalf, Mathf.Max(baseHalf, maxHalf));
        }

        public void ClearZone(Rect zone)
        {
            if (_zone.HasValue && _zone.Value == zone) _zone = null;
        }

        public void Shake(float amount, float duration)
        {
            _shakeAmount = Mathf.Max(_shakeAmount, amount);
            _shakeTime = Mathf.Max(_shakeTime, duration);
        }

        void LateUpdate()
        {
            if (_cam == null) return;
            float dt = Time.deltaTime;

            float targetHalf = _zone.HasValue ? _zoneHalf : BaseHalf;
            if (!Mathf.Approximately(_cam.orthographicSize, targetHalf))
            {
                float half = Mathf.SmoothDamp(_cam.orthographicSize, targetHalf, ref _zoomVel, zoomTime, Mathf.Infinity, Mathf.Max(dt, 0.0001f));
                if (Mathf.Abs(half - targetHalf) < 0.002f) { half = targetHalf; _zoomVel = 0f; }
                _cam.orthographicSize = half;
            }

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

            Vector2 shake = Vector2.zero;
            if (_shakeTime > 0f)
            {
                _shakeTime -= Time.unscaledDeltaTime;
                shake = Random.insideUnitCircle * _shakeAmount;
                if (_shakeTime <= 0f) _shakeAmount = 0f;
            }
            Apply(shake);
        }

        Vector2 Clamp(Vector2 p)
        {
            Rect r = _bounds;
            if (_zone.HasValue)
            {
                // Zone is confined to the level so the camera never shows outside it.
                var z = _zone.Value;
                float xMin = Mathf.Max(z.xMin, _bounds.xMin), yMin = Mathf.Max(z.yMin, _bounds.yMin);
                float xMax = Mathf.Min(z.xMax, _bounds.xMax), yMax = Mathf.Min(z.yMax, _bounds.yMax);
                r = Rect.MinMaxRect(xMin, yMin, Mathf.Max(xMin, xMax), Mathf.Max(yMin, yMax));
            }
            float minX = r.xMin + HalfW, maxX = r.xMax - HalfW;
            p.x = minX > maxX ? r.center.x : Mathf.Clamp(p.x, minX, maxX);
            float minY = r.yMin + HalfH, maxY = r.yMax - HalfH;
            p.y = minY > maxY ? Mathf.Max(_bounds.yMin + HalfH, r.center.y) : Mathf.Clamp(p.y, minY, maxY);
            return p;
        }

        void Apply(Vector2 offset)
        {
            Vector2 p = _pos + offset;
            if (IronoakConfig.PixelPerfect)
            {
                float ppu = PixelArt.PPU;
                p = new Vector2(Mathf.Round(p.x * ppu) / ppu, Mathf.Round(p.y * ppu) / ppu);
            }
            transform.position = new Vector3(p.x, p.y, -10f);
        }
    }
}
