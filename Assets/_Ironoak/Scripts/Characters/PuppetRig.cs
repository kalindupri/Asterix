using System.Collections.Generic;
using Ironoak.Art;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ironoak.Characters
{
    /// <summary>
    /// A built cut-out puppet: a hierarchy of bone transforms (one per part) with the painted
    /// images attached so each joint sits exactly on its bone. Animate by rotating bones.
    /// </summary>
    public class PuppetRig : MonoBehaviour
    {
        readonly Dictionary<string, Transform> _bones = new Dictionary<string, Transform>();
        readonly Dictionary<string, Vector3> _rest = new Dictionary<string, Vector3>();
        readonly List<SpriteRenderer> _renderers = new List<SpriteRenderer>();
        readonly List<Color> _tints = new List<Color>();

        public IReadOnlyList<SpriteRenderer> Renderers => _renderers;

        public static PuppetRig Build(Transform parent, PuppetDefinition def, int sortingOrder)
        {
            var root = new GameObject("Puppet");
            root.transform.SetParent(parent, false);
            var group = root.AddComponent<SortingGroup>();
            group.sortingOrder = sortingOrder;
            var rig = root.AddComponent<PuppetRig>();

            foreach (var part in def.parts)
            {
                var bone = new GameObject(part.name).transform;
                var parentBone = string.IsNullOrEmpty(part.parent) ? root.transform : rig._bones[part.parent];
                bone.SetParent(parentBone, false);
                bone.localPosition = part.offset;
                rig._bones[part.name] = bone;
                rig._rest[part.name] = part.offset;

                var sprite = SpriteLibrary.GetPainted(def.spriteFolder, part.sprite);
                if (sprite == null) continue;
                var art = new GameObject("art").transform;
                art.SetParent(bone, false);
                var sr = art.gameObject.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = part.order;
                sr.color = part.tint;

                // Scale the image to its on-screen height, then shift it so the joint lands on the bone.
                Bounds b = sprite.bounds;
                float k = part.height / Mathf.Max(0.0001f, b.size.y);
                Vector2 joint = (Vector2)b.min + Vector2.Scale(part.pivot, b.size);
                art.localScale = new Vector3(k, k, 1f);
                art.localPosition = -(Vector3)(joint * k);

                rig._renderers.Add(sr);
                rig._tints.Add(part.tint);
            }
            return rig;
        }

        public Transform Bone(string name) => _bones.TryGetValue(name, out var t) ? t : null;
        public Vector3 RestPosition(string name) => _rest.TryGetValue(name, out var p) ? p : Vector3.zero;

        public void SetVisible(bool visible)
        {
            foreach (var r in _renderers) r.enabled = visible;
        }

        /// <summary>Flashes the whole puppet towards a colour (0 = normal tints).</summary>
        public void Flash(Color color, float amount)
        {
            for (int i = 0; i < _renderers.Count; i++)
                _renderers[i].color = Color.Lerp(_tints[i], color, amount);
        }
    }
}
