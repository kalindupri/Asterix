using System.Collections.Generic;
using Ironoak.Art;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Ironoak.Gameplay
{
    /// <summary>Runtime result of building a level.</summary>
    public sealed class Level
    {
        public GameObject Root;
        public string Name = "LEVEL";
        public Rect Bounds;
        public Vector2 Spawn;
        public int TotalCoins;
        public PlayerController Player;
        public readonly List<RomanSoldier> Enemies = new List<RomanSoldier>();
    }

    /// <summary>
    /// Builds a playable level from a plain-text map (Resources/Levels/*.txt).
    /// Legend:  # ground   = stone   - one-way plank   o coin   ^ spikes
    ///          E legionary   P player start   C checkpoint   G goal   . air
    /// Lines starting with ';' are comments, '@name X' sets the level title.
    /// </summary>
    public static class LevelBuilder
    {
        public static Level Build(string source, Transform parent, bool spawnPlayer)
        {
            var level = new Level();
            var rows = new List<string>();
            foreach (var raw in source.Split('\n'))
            {
                string line = raw.TrimEnd('\r');
                if (line.StartsWith(";")) continue;
                if (line.StartsWith("@name ")) { level.Name = line.Substring(6).Trim(); continue; }
                if (line.StartsWith("@")) continue;
                rows.Add(line);
            }
            while (rows.Count > 0 && string.IsNullOrWhiteSpace(rows[rows.Count - 1])) rows.RemoveAt(rows.Count - 1);
            while (rows.Count > 0 && string.IsNullOrWhiteSpace(rows[0])) rows.RemoveAt(0);

            int height = rows.Count, width = 0;
            foreach (var r in rows) width = Mathf.Max(width, r.Length);

            char At(int row, int col) =>
                row < 0 || row >= height || col < 0 || col >= rows[row].Length ? '.' : rows[row][col];

            level.Root = new GameObject("Level");
            level.Root.transform.SetParent(parent, false);
            var root = level.Root.transform;

            var grid = new GameObject("Grid").AddComponent<Grid>();
            grid.transform.SetParent(root, false);
            var solid = CreateTilemap(grid.transform, "Solid", 0);
            var oneWay = CreateTilemap(grid.transform, "OneWay", 1);

            var tGrass = MakeTile("tile_grass");
            var tDirt = MakeTile("tile_dirt");
            var tStone = MakeTile("tile_stone");
            var tPlank = MakeTile("tile_plank");

            var enemySpawns = new List<Vector2>();
            level.Spawn = new Vector2(1.5f, height);

            for (int row = 0; row < height; row++)
            {
                int y = height - 1 - row;
                for (int col = 0; col < width; col++)
                {
                    char ch = At(row, col);
                    var cell = new Vector3Int(col, y, 0);
                    var feet = new Vector2(col + 0.5f, y);
                    switch (ch)
                    {
                        case '#':
                            char above = At(row - 1, col);
                            solid.SetTile(cell, above == '#' || above == '=' ? tDirt : tGrass);
                            break;
                        case '=': solid.SetTile(cell, tStone); break;
                        case '-': oneWay.SetTile(cell, tPlank); break;
                        case 'o': Coin.Spawn(root, new Vector2(col + 0.5f, y + 0.5f)); level.TotalCoins++; break;
                        case '^': Spikes.Spawn(root, feet); break;
                        case 'E': enemySpawns.Add(feet); break;
                        case 'P': level.Spawn = feet; break;
                        case 'C': Checkpoint.Spawn(root, feet); break;
                        case 'G': Goal.Spawn(root, feet); break;
                    }
                }
            }

            AddCollider(solid, false);
            AddCollider(oneWay, true);
            AddWall(root, -0.5f, height);
            AddWall(root, width + 0.5f, height);

            level.Bounds = new Rect(0f, 0f, width, height);

            foreach (var p in enemySpawns) level.Enemies.Add(RomanSoldier.Spawn(root, p));

            if (spawnPlayer)
            {
                level.Player = PlayerController.Spawn(root, level.Spawn);
                foreach (var e in level.Enemies) e.IgnorePlayer(level.Player.Collider);
            }
            return level;
        }

        static Tilemap CreateTilemap(Transform grid, string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid, false);
            var tm = go.AddComponent<Tilemap>();
            go.AddComponent<TilemapRenderer>().sortingOrder = order;
            return tm;
        }

        static void AddCollider(Tilemap tm, bool oneWay)
        {
            var go = tm.gameObject;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            var tc = go.AddComponent<TilemapCollider2D>();
            tc.compositeOperation = Collider2D.CompositeOperation.Merge;
            var cc = go.AddComponent<CompositeCollider2D>();
            cc.geometryType = CompositeCollider2D.GeometryType.Polygons;
            if (oneWay)
            {
                cc.usedByEffector = true;
                var effector = go.AddComponent<PlatformEffector2D>();
                effector.useOneWay = true;
                effector.surfaceArc = 160f;
                effector.useSideFriction = false;
            }
            go.AddComponent<SolidGround>();
        }

        static void AddWall(Transform root, float x, int height)
        {
            var go = new GameObject("Wall");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(x, height * 0.5f + 10f, 0f);
            var bc = go.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(1f, height + 40f);
            go.AddComponent<SolidGround>();
        }

        static Tile MakeTile(string spriteName)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = SpriteLibrary.Get(spriteName);
            tile.colliderType = Tile.ColliderType.Grid;
            tile.name = spriteName;
            return tile;
        }
    }
}
