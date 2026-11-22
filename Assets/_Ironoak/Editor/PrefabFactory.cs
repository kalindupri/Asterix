using System.IO;
using Ironoak.Art;
using Ironoak.Gameplay;
using Ironoak.Powers;
using Ironoak.Puzzles;
using UnityEditor;
using UnityEngine;

namespace Ironoak.EditorTools
{
    /// <summary>
    /// Generates the level-building prefab library in Assets/_Ironoak/Prefabs. Level designers
    /// drag these into level scenes; the greybox blueprint uses them too. Re-running regenerates
    /// the prefabs (scene instances keep their per-instance overrides).
    /// </summary>
    public static class PrefabFactory
    {
        public const string Dir = "Assets/_Ironoak/Prefabs";

        [MenuItem("Ironoak/Pipeline/2. Generate Prefabs", priority = 101)]
        public static void BuildAll()
        {
            Directory.CreateDirectory(Dir);
            Save("Coin", Coin());
            Save("Spikes", Spikes.Spawn(null, Vector2.zero).gameObject);
            Save("Checkpoint", Checkpoint());
            Save("Goal", Goal.Spawn(null, Vector2.zero).gameObject);
            Save("RomanSoldier", Soldier());
            Save("SapKnot", Knot());
            Save("Bell", BellPrefab());
            Save("Rope", new GameObject("Rope", typeof(Rope)));
            Save("Gate", GatePrefab());
            Save("Drawbridge", DrawbridgePrefab());
            Save("PressurePlate", Plate());
            Save("Crate", CratePrefab());
            Save("CrackedWall", Cracked());
            Save("WaterPool", Water());
            Save("Collectible_Ogham", Item(CollectibleType.OghamStone));
            Save("Collectible_GoldenTorc", Item(CollectibleType.GoldenTorc));
            Save("Collectible_ShrineShard", Item(CollectibleType.ShrineShard));
            Save("Collectible_DruidMark", Item(CollectibleType.DruidMark));
            Save("HeartwoodTorc", Torc());
            Save("Decor_BrokenCart", Decor("cart_broken", 1));
            AssetDatabase.SaveAssets();
            Debug.Log("[Ironoak] Prefabs generated in " + Dir);
        }

        public static GameObject Load(string name) => AssetDatabase.LoadAssetAtPath<GameObject>($"{Dir}/{name}.prefab");

        static void Save(string name, GameObject go)
        {
            go.name = name;
            PrefabUtility.SaveAsPrefabAsset(go, $"{Dir}/{name}.prefab");
            Object.DestroyImmediate(go);
        }

        static Sprite S(string name) => SpriteLibrary.Get(name);

        static SpriteRenderer Renderer(GameObject go, string sprite, int order)
        {
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = S(sprite);
            sr.sortingOrder = order;
            return sr;
        }

        static GameObject Coin()
        {
            var go = Gameplay.Coin.Spawn(null, Vector2.zero).gameObject;
            go.GetComponent<SpriteRenderer>().sprite = S("coin_0");
            return go;
        }

        static GameObject Checkpoint()
        {
            var go = Gameplay.Checkpoint.Spawn(null, Vector2.zero).gameObject;
            go.GetComponent<SpriteRenderer>().sprite = S("torch_off");
            return go;
        }

        static GameObject Soldier()
        {
            var go = new GameObject();
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            var body = go.AddComponent<BoxCollider2D>();
            body.size = new Vector2(0.6f, 0.88f);
            body.offset = new Vector2(0f, 0.44f);
            var hit = go.AddComponent<BoxCollider2D>();
            hit.isTrigger = true;
            hit.size = new Vector2(0.75f, 0.95f);
            hit.offset = new Vector2(0f, 0.47f);
            Renderer(go, "roman_walk1", 8);
            go.AddComponent<RomanSoldier>();
            return go;
        }

        static GameObject Knot()
        {
            var go = new GameObject();
            Renderer(go, "sap_knot", 2);
            go.AddComponent<SapKnot>();
            return go;
        }

        static GameObject BellPrefab()
        {
            var go = new GameObject();
            Renderer(go, "bell_post", 3);
            var pivot = new GameObject("Pivot");
            pivot.transform.SetParent(go.transform, false);
            pivot.transform.localPosition = new Vector3(0.22f, 2.62f, 0f);
            var bell = new GameObject("BellBody");
            bell.transform.SetParent(pivot.transform, false);
            Renderer(bell, "bell", 4);
            var col = bell.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
            col.offset = new Vector2(0f, -0.4f);
            go.AddComponent<Bell>().swingPivot = pivot.transform;
            return go;
        }

        static GameObject GatePrefab()
        {
            var go = new GameObject();
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<BoxCollider2D>();
            var sr = Renderer(go, "gate_bars", -1);
            sr.drawMode = SpriteDrawMode.Tiled;
            go.AddComponent<SolidGround>();
            go.AddComponent<Gate>().Configure();
            return go;
        }

        static GameObject DrawbridgePrefab()
        {
            var go = new GameObject();
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            go.AddComponent<Drawbridge>();
            return go;
        }

        static GameObject Plate()
        {
            var go = new GameObject();
            Renderer(go, "plate_up", 3);
            go.AddComponent<PressurePlate>();
            return go;
        }

        static GameObject CratePrefab()
        {
            var go = new GameObject();
            var rb = go.AddComponent<Rigidbody2D>();
            rb.mass = 2f;
            rb.gravityScale = 3f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.96f, 0.96f);
            box.offset = new Vector2(0f, 0.48f);
            Renderer(go, "crate", 6);
            go.AddComponent<SolidGround>();
            go.AddComponent<Crate>();
            return go;
        }

        static GameObject Cracked()
        {
            var go = new GameObject();
            go.AddComponent<BoxCollider2D>();
            var sr = Renderer(go, "wall_cracked", 1);
            sr.drawMode = SpriteDrawMode.Tiled;
            go.AddComponent<SolidGround>();
            go.AddComponent<CrackedWall>().Configure();
            return go;
        }

        static GameObject Water()
        {
            var go = new GameObject();
            var sr = Renderer(go, "water", 15);
            sr.drawMode = SpriteDrawMode.Tiled;
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<WaterPool>();
            return go;
        }

        static GameObject Item(CollectibleType type)
        {
            var go = new GameObject();
            Renderer(go, Collectible.SpriteFor(type), 7);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;
            go.AddComponent<Collectible>().type = type;
            return go;
        }

        static GameObject Torc()
        {
            var go = new GameObject();
            Renderer(go, "item_heartwood_torc", 7);
            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.55f;
            var p = go.AddComponent<PowerPickup>();
            p.power = Power.Rootgrow;
            return go;
        }

        static GameObject Decor(string sprite, int order)
        {
            var go = new GameObject();
            Renderer(go, sprite, order);
            return go;
        }
    }
}
