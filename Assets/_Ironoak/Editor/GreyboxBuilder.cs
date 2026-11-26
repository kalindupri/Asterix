using System.Collections.Generic;
using System.IO;
using Ironoak.Art;
using Ironoak.Gameplay;
using Ironoak.Puzzles;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Ironoak.EditorTools
{
    /// <summary>
    /// Small API for laying out greybox levels in code, which produces a normal Unity scene
    /// with Tilemaps and prefab instances that designers then refine by hand.
    /// Coordinates are tiles. Fill ranges: x0..x1 inclusive, yBottom..yTop exclusive
    /// (so <c>yTop</c> is the walkable surface height).
    /// </summary>
    public class GreyboxBuilder
    {
        enum Mat : byte { None, Earth, Stone, Prop }

        public const string TileDir = "Assets/_Ironoak/Art/Tiles";

        readonly Dictionary<Vector2Int, Mat> _cells = new Dictionary<Vector2Int, Mat>();
        readonly Scene _scene;
        public readonly Transform Root;
        readonly Transform _props;

        public GreyboxBuilder(Scene scene, string rootName)
        {
            _scene = scene;
            var root = new GameObject(rootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            Root = root.transform;
            _props = new GameObject("Props").transform;
            _props.SetParent(Root, false);
        }

        // ---------- terrain ----------

        public void Earth(int x0, int x1, int yBottom, int yTop) => Fill(x0, x1, yBottom, yTop, Mat.Earth);
        public void Stone(int x0, int x1, int yBottom, int yTop) => Fill(x0, x1, yBottom, yTop, Mat.Stone);
        public void Clear(int x0, int x1, int yBottom, int yTop) => Fill(x0, x1, yBottom, yTop, Mat.None);
        /// <summary>Solid cells that the painted terrain leaves blank: a painted prop sprite draws them.</summary>
        public void Prop(int x0, int x1, int yBottom, int yTop) => Fill(x0, x1, yBottom, yTop, Mat.Prop);
        public void Ground(int x0, int x1, int top) => Earth(x0, x1, 0, top);

        void Fill(int x0, int x1, int y0, int y1, Mat m)
        {
            for (int x = x0; x <= x1; x++)
                for (int y = y0; y < y1; y++)
                {
                    var c = new Vector2Int(x, y);
                    if (m == Mat.None) _cells.Remove(c);
                    else _cells[c] = m;
                }
        }

        bool Solid(int x, int y) => _cells.ContainsKey(new Vector2Int(x, y));

        public void BuildTilemaps()
        {
            var grid = new GameObject("Grid").AddComponent<Grid>();
            grid.transform.SetParent(Root, false);
            var go = new GameObject("Ground");
            go.transform.SetParent(grid.transform, false);
            var tm = go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder = 0;

            var grass = TileAsset("tile_grass");
            var dirt = TileAsset("tile_dirt");
            var stone = TileAsset("tile_stone");
            var plank = TileAsset("tile_plank");
            foreach (var kv in _cells)
            {
                var p = kv.Key;
                TileBase t = kv.Value == Mat.Stone ? stone : kv.Value == Mat.Prop ? plank : Solid(p.x, p.y + 1) ? dirt : grass;
                tm.SetTile(new Vector3Int(p.x, p.y, 0), t);
            }

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            var tc = go.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
            var cc = go.AddComponent<CompositeCollider2D>();
            // Outlines (edge chains) instead of polygons: no "ghost" snags on tile seams when
            // sliding along walls, which otherwise steals jump height.
            cc.geometryType = CompositeCollider2D.GeometryType.Outlines;
            go.AddComponent<SolidGround>();
        }

        static Tile TileAsset(string spriteName)
        {
            Directory.CreateDirectory(TileDir);
            string path = $"{TileDir}/{spriteName}.asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                tile = ScriptableObject.CreateInstance<Tile>();
                AssetDatabase.CreateAsset(tile, path);
            }
            tile.sprite = SpriteLibrary.Get(spriteName);
            tile.colliderType = Tile.ColliderType.Grid;
            EditorUtility.SetDirty(tile);
            return tile;
        }

        // ---------- objects ----------

        public GameObject Place(string prefab, float x, float y, string name = null, Transform parent = null)
        {
            var asset = PrefabFactory.Load(prefab);
            if (asset == null) throw new System.Exception($"Missing prefab {prefab}. Run Generate Prefabs first.");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(asset, _scene);
            go.transform.SetParent(parent != null ? parent : _props, false);
            go.transform.position = new Vector3(x, y, 0f);
            if (!string.IsNullOrEmpty(name)) go.name = name;
            return go;
        }

        public T Place<T>(string prefab, float x, float y, string name = null) where T : Component =>
            Place(prefab, x, y, name).GetComponent<T>();

        public GameObject Empty(string name, float x, float y)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_props, false);
            go.transform.position = new Vector3(x, y, 0f);
            return go;
        }

        /// <summary>Records edits made to a prefab instance so they persist in the scene.</summary>
        public static void Commit(Component c)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(c)) PrefabUtility.RecordPrefabInstancePropertyModifications(c);
            EditorUtility.SetDirty(c);
        }

        public void Wire(SignalSender sender, params SignalReceiver[] receivers)
        {
            foreach (var r in receivers) sender.targets.Add(r);
            Commit(sender);
        }

        public void Coins(float x0, float y, int count, float step = 1f)
        {
            for (int i = 0; i < count; i++) Place("Coin", x0 + i * step, y);
        }

        public void CoinArc(float x0, float y, int count, float height)
        {
            for (int i = 0; i < count; i++)
            {
                float t = count == 1 ? 0.5f : i / (float)(count - 1);
                Place("Coin", x0 + i, y + Mathf.Sin(t * Mathf.PI) * height);
            }
        }

        /// <summary>A hidden room: trigger + a cover sprite that hides it until found.</summary>
        public SecretArea Secret(string name, int x0, int y0, int x1, int y1, bool trial, Color? coverTint = null, bool leaves = false)
        {
            float w = x1 - x0 + 1, h = y1 - y0;
            var go = Empty((trial ? "Trial " : "Secret ") + name, x0 + w * 0.5f, y0 + h * 0.5f);
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(w, h);
            var area = go.AddComponent<SecretArea>();
            area.secretName = name;
            area.isTrial = trial;
            if (coverTint.HasValue)
            {
                var cover = new GameObject("Cover");
                cover.transform.SetParent(go.transform, false);
                var sr = cover.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteLibrary.Get("secret_cover");
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(w, h);
                sr.color = coverTint.Value;
                var canopy = leaves ? SpriteLibrary.GetPaintedOrNull("Props", "secret_cover_canopy") : null;
                if (canopy != null && SpriteLibrary.PreferPainted)
                {
                    // The canopy only repeats sideways: scale one row up to the room's height.
                    float k = h / canopy.bounds.size.y;
                    sr.sprite = canopy;
                    sr.color = Color.white;
                    cover.transform.localScale = new Vector3(k, k, 1f);
                    sr.size = new Vector2(w / k + 0.4f, canopy.bounds.size.y);
                }
                else if (SpriteLibrary.IsPainted(sr.sprite)) sr.color = Color.white;
                sr.sortingOrder = 20;
                area.covers = new[] { sr };
            }
            return area;
        }

        public CameraZone Zone(string name, float x0, float y0, float x1, float y1)
        {
            var go = Empty("CameraZone " + name, (x0 + x1) * 0.5f, (y0 + y1) * 0.5f);
            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;
            box.size = new Vector2(x1 - x0, y1 - y0);
            return go.AddComponent<CameraZone>();
        }
    }
}
