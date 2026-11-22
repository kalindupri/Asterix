using System.Collections.Generic;
using Ironoak.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Ironoak.Art
{
    /// <summary>
    /// Paints a solid Tilemap with hand-painted terrain: a world-space tiled earth fill, grassy
    /// top edges, cliff faces, dangling-root undersides and rounded corners. The Tilemap stays
    /// the (invisible) collision layer, so level design and physics are unchanged.
    ///
    /// Art (Resources/Art/Painted/Environment/&lt;set&gt;/): terrain_earth_fill (512², tiles both
    /// ways), _top (512×192, walkable line 64 px from the top), _side (192×512, face on the left),
    /// _bottom (512×128), _corner_tl/_tr (192²). 128 px = 1 tile.
    /// </summary>
    public class PaintedTerrain : MonoBehaviour
    {
        // Strip geometry in tiles (from the art spec).
        const float FillTile = 4f;          // fill and strip textures repeat every 512 px
        const float TopAbove = 0.5f;        // grass above the walkable line
        const float TopBelow = 1.0f;        // soil lip below it
        const float SideOut = 0.18f;        // cliff face overhangs the collider slightly
        const float SideWidth = 1.5f;
        const float BottomAbove = 0.38f, BottomBelow = 0.62f;
        const float CornerSize = 1.5f;

        static readonly Color32 StoneTint = new Color32(158, 158, 170, 255);

        class Quads
        {
            public readonly List<Vector3> V = new List<Vector3>();
            public readonly List<Vector2> UV = new List<Vector2>();
            public readonly List<Color32> C = new List<Color32>();
            public readonly List<int> T = new List<int>();

            public void Add(float x0, float y0, float x1, float y1, float u0, float v0, float u1, float v1, Color32 c, float z)
            {
                int i = V.Count;
                V.Add(new Vector3(x0, y0, z)); V.Add(new Vector3(x0, y1, z)); V.Add(new Vector3(x1, y1, z)); V.Add(new Vector3(x1, y0, z));
                UV.Add(new Vector2(u0, v0)); UV.Add(new Vector2(u0, v1)); UV.Add(new Vector2(u1, v1)); UV.Add(new Vector2(u1, v0));
                for (int k = 0; k < 4; k++) C.Add(c);
                T.Add(i); T.Add(i + 1); T.Add(i + 2); T.Add(i); T.Add(i + 2); T.Add(i + 3);
            }

            /// <summary>
            /// A quad split on a grid (xs/ys are normalised split positions) with per-vertex alpha,
            /// used to fade strip edges into the fill instead of ending in a hard seam.
            /// </summary>
            public void AddGrid(float x0, float y0, float x1, float y1, float u0, float v0, float u1, float v1, Color32 c, float z,
                                float[] xs, float[] ys, System.Func<float, float, float> alpha)
            {
                int i0 = V.Count, nx = xs.Length, ny = ys.Length;
                for (int j = 0; j < ny; j++)
                    for (int i = 0; i < nx; i++)
                    {
                        float xn = xs[i], yn = ys[j];
                        V.Add(new Vector3(Mathf.Lerp(x0, x1, xn), Mathf.Lerp(y0, y1, yn), z));
                        UV.Add(new Vector2(Mathf.Lerp(u0, u1, xn), Mathf.Lerp(v0, v1, yn)));
                        var col = c;
                        col.a = (byte)Mathf.RoundToInt(c.a * Mathf.Clamp01(alpha(xn, yn)));
                        C.Add(col);
                    }
                for (int j = 0; j < ny - 1; j++)
                    for (int i = 0; i < nx - 1; i++)
                    {
                        int a = i0 + j * nx + i, bq = a + nx;
                        T.Add(a); T.Add(bq); T.Add(bq + 1); T.Add(a); T.Add(bq + 1); T.Add(a + 1);
                    }
            }
        }

        const float EdgeFade = 0.3f; // fraction of a corner faded on its inner edges
        static float Ramp(float t) => Mathf.Clamp01(t);

        public static bool Available(string set = "Forest") =>
            !IronoakConfig.PixelPerfect && Resources.Load<Texture2D>(Dir(set) + "terrain_earth_fill") != null;

        static string Dir(string set) => $"Art/Painted/Environment/{set}/";

        /// <summary>Paints every collision tilemap in the scene root's hierarchy. Returns how many were painted.</summary>
        public static int ApplyAll(GameObject root, string set = "Forest")
        {
            if (!Available(set)) return 0;
            int n = 0;
            foreach (var tm in root.GetComponentsInChildren<Tilemap>(true))
            {
                if (tm.GetComponent<TilemapCollider2D>() == null || tm.GetComponent<PaintedTerrain>() != null) continue;
                tm.gameObject.AddComponent<PaintedTerrain>().Build(tm, set);
                n++;
            }
            return n;
        }

        void Build(Tilemap tm, string set)
        {
            string dir = Dir(set);
            var fillTex = Resources.Load<Texture2D>(dir + "terrain_earth_fill");
            var topTex = Resources.Load<Texture2D>(dir + "terrain_earth_top");
            var sideTex = Resources.Load<Texture2D>(dir + "terrain_earth_side");
            var bottomTex = Resources.Load<Texture2D>(dir + "terrain_earth_bottom");
            var tlTex = Resources.Load<Texture2D>(dir + "terrain_earth_corner_tl");
            var trTex = Resources.Load<Texture2D>(dir + "terrain_earth_corner_tr");

            // Collect solid cells and which are stone.
            tm.CompressBounds();
            var solid = new HashSet<Vector2Int>();
            var stone = new HashSet<Vector2Int>();
            var b = tm.cellBounds;
            foreach (var p in b.allPositionsWithin)
            {
                var tile = tm.GetTile(p);
                if (tile == null) continue;
                var c = new Vector2Int(p.x, p.y);
                solid.Add(c);
                if (tile.name.Contains("stone")) stone.Add(c);
            }
            if (solid.Count == 0) return;
            int minY = b.yMin;
            bool S(int x, int y) => solid.Contains(new Vector2Int(x, y));
            Color32 Tint(int x, int y) => stone.Contains(new Vector2Int(x, y)) ? StoneTint : (Color32)Color.white;

            var fill = new Quads();
            var side = new Quads();
            var bottom = new Quads();
            var top = new Quads();
            var tl = new Quads();
            var tr = new Quads();

            for (int y = b.yMin; y < b.yMax; y++)
            {
                // Fill: horizontal runs of the same material.
                for (int x = b.xMin; x < b.xMax; x++)
                {
                    if (!S(x, y)) continue;
                    var tint = Tint(x, y);
                    int x1 = x;
                    while (S(x1 + 1, y) && Tint(x1 + 1, y).Equals(tint)) x1++;
                    fill.Add(x, y, x1 + 1, y + 1, x / FillTile, y / FillTile, (x1 + 1) / FillTile, (y + 1) / FillTile, tint, 0.05f);
                    x = x1;
                }

                // Top edges: runs of solid cells with open sky above.
                for (int x = b.xMin; x < b.xMax; x++)
                {
                    if (!S(x, y) || S(x, y + 1)) continue;
                    var tint = Tint(x, y);
                    int x1 = x;
                    while (S(x1 + 1, y) && !S(x1 + 1, y + 1) && Tint(x1 + 1, y).Equals(tint)) x1++;
                    float s = y + 1;
                    top.Add(x, s - TopBelow, x1 + 1, s + TopAbove, x / FillTile, 0f, (x1 + 1) / FillTile, 1f, tint, 0.02f);
                    // Corners never reach past the middle of a narrow ledge; the texture is cropped, not squashed.
                    float cw = SideOut + Mathf.Min(CornerSize - SideOut, (x1 - x + 1) * 0.5f);
                    float cu = cw / CornerSize;
                    var ys = new[] { 0f, EdgeFade, 1f };
                    if (!S(x - 1, y))
                        tl.AddGrid(x - SideOut, s - TopBelow, x - SideOut + cw, s + TopAbove, 0f, 0f, cu, 1f, tint, 0.01f,
                            new[] { 0f, 1f - EdgeFade, 1f }, ys, (xn, yn) => Ramp((1f - xn) / EdgeFade) * Ramp(yn / EdgeFade));
                    if (!S(x1 + 1, y))
                        tr.AddGrid(x1 + 1 + SideOut - cw, s - TopBelow, x1 + 1 + SideOut, s + TopAbove, 1f - cu, 0f, 1f, 1f, tint, 0.01f,
                            new[] { 0f, EdgeFade, 1f }, ys, (xn, yn) => Ramp(xn / EdgeFade) * Ramp(yn / EdgeFade));
                    x = x1;
                }

                // Undersides: solid cells with open space below (not the bottom of the world).
                if (y > minY)
                    for (int x = b.xMin; x < b.xMax; x++)
                    {
                        if (!S(x, y) || S(x, y - 1)) continue;
                        var tint = Tint(x, y);
                        int x1 = x;
                        while (S(x1 + 1, y) && !S(x1 + 1, y - 1) && Tint(x1 + 1, y).Equals(tint)) x1++;
                        bottom.Add(x, y - BottomBelow, x1 + 1, y + BottomAbove, x / FillTile, 0f, (x1 + 1) / FillTile, 1f, tint, 0.03f);
                        x = x1;
                    }
            }

            // Cliff faces: vertical runs with open space to the left / right.
            for (int x = b.xMin; x < b.xMax; x++)
            {
                for (int face = -1; face <= 1; face += 2)
                {
                    // How far the face may reach inward: half the solid depth behind it, so faces on
                    // both sides of a thin pillar meet in the middle instead of overhanging.
                    float Inner(int yy)
                    {
                        int depth = 0;
                        while (depth < 3 && S(x - face * depth, yy)) depth++;
                        return Mathf.Min(SideWidth - SideOut, depth * 0.5f);
                    }

                    for (int y = b.yMin; y < b.yMax; y++)
                    {
                        if (!S(x, y) || S(x + face, y)) continue;
                        var tint = Tint(x, y);
                        float inner = Inner(y);
                        int y1 = y;
                        while (S(x, y1 + 1) && !S(x + face, y1 + 1) && Tint(x, y1 + 1).Equals(tint) && Mathf.Approximately(Inner(y1 + 1), inner)) y1++;
                        float v0 = y / FillTile, v1 = (y1 + 1) / FillTile;
                        float w = SideOut + inner, uw = w / SideWidth;
                        float f = Mathf.Min(0.45f, w * 0.4f) / w; // inner fade, normalised
                        var ys = new[] { 0f, 1f };
                        if (face < 0)
                            side.AddGrid(x - SideOut, y, x - SideOut + w, y1 + 1, 0f, v0, uw, v1, tint, 0.04f,
                                new[] { 0f, 1f - f, 1f }, ys, (xn, yn) => Ramp((1f - xn) / f));
                        else // mirrored: the face is on the right
                            side.AddGrid(x + 1 + SideOut - w, y, x + 1 + SideOut, y1 + 1, uw, v0, 0f, v1, tint, 0.04f,
                                new[] { 0f, f, 1f }, ys, (xn, yn) => Ramp(xn / f));
                        y = y1;
                    }
                }
            }

            var baseMat = DefaultSpriteMaterial();
            Make("Fill", fill, fillTex, baseMat);
            Make("Sides", side, sideTex, baseMat);
            Make("Undersides", bottom, bottomTex, baseMat);
            Make("Tops", top, topTex, baseMat);
            Make("CornersL", tl, tlTex, baseMat);
            Make("CornersR", tr, trTex, baseMat);

            var tr2 = tm.GetComponent<TilemapRenderer>();
            if (tr2 != null) tr2.enabled = false;
        }

        /// <summary>The render pipeline's default sprite material (lit under URP 2D).</summary>
        Material DefaultSpriteMaterial()
        {
            var probe = new GameObject("probe").AddComponent<SpriteRenderer>();
            var mat = probe.sharedMaterial;
            Destroy(probe.gameObject);
            return mat;
        }

        void Make(string name, Quads q, Texture2D tex, Material baseMat)
        {
            if (q.V.Count == 0 || tex == null) return;
            var mesh = new Mesh { name = "PaintedTerrain_" + name };
            if (q.V.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.SetVertices(q.V);
            mesh.SetUVs(0, q.UV);
            mesh.SetColors(q.C);
            mesh.SetTriangles(q.T, 0);
            mesh.RecalculateBounds();

            var go = new GameObject("Painted" + name);
            go.transform.SetParent(transform, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var mr = go.AddComponent<MeshRenderer>();
            var mat = new Material(baseMat) { name = "Terrain_" + name, mainTexture = tex };
            mr.sharedMaterial = mat;
            mr.sortingOrder = 0; // same slot as the old tiles: roots and gates (-1) hide behind, props (1+) in front
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
        }
    }
}
