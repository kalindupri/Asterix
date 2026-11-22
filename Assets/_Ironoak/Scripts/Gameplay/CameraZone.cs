using Ironoak.Core;
using UnityEngine;

namespace Ironoak.Gameplay
{
    /// <summary>
    /// While Brenna is inside, the camera is confined to this box (frames a puzzle room) and, in
    /// the Native display mode, zooms to frame it.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class CameraZone : MonoBehaviour
    {
        [Tooltip("View height while inside, in tiles. 0 = zoom out just enough to show the whole zone; -1 = keep the default zoom.")]
        public float viewHeight;

        BoxCollider2D _box;

        Rect Zone
        {
            get
            {
                var b = _box.bounds;
                return Rect.MinMaxRect(b.min.x, b.min.y, b.max.x, b.max.y);
            }
        }

        void Awake()
        {
            _box = GetComponent<BoxCollider2D>();
            _box.isTrigger = true;
        }

        static bool IsPlayer(Collider2D c) => c.attachedRigidbody != null && c.attachedRigidbody.GetComponent<PlayerController>() != null;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (IsPlayer(other)) CameraRig.Instance?.SetZone(Zone, viewHeight);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (IsPlayer(other)) CameraRig.Instance?.ClearZone(Zone);
        }

        void OnDrawGizmos()
        {
            var box = GetComponent<BoxCollider2D>();
            if (box == null) return;
            Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.8f);
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        }
    }
}
