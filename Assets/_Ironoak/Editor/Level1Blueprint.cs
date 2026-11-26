using System.IO;
using Ironoak.Gameplay;
using Ironoak.Puzzles;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Ironoak.EditorTools
{
    /// <summary>
    /// Greybox of Level 1 "The Oak Road" (docs/design/LEVELS.md, puzzles P1-01..P1-07, P1-T).
    /// Generates Assets/_Ironoak/Scenes/Levels/L1_OakRoad.unity. Once designers start hand-editing
    /// that scene, stop regenerating it (the menu asks before overwriting).
    ///
    /// Tile units. Ground surface is y = 3 unless noted. Jump height is about 3.4 tiles.
    /// </summary>
    public static class Level1Blueprint
    {
        public const string ScenePath = "Assets/_Ironoak/Scenes/Levels/L1_OakRoad.unity";
        static readonly Color Rock = Color.white;
        static readonly Color Leaves = new Color(0.55f, 0.85f, 0.5f);

        [MenuItem("Ironoak/Pipeline/3. Build Level 1 Greybox", priority = 102)]
        static void BuildFromMenu()
        {
            if (File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Rebuild Level 1?", "This regenerates L1_OakRoad.unity and discards manual edits to it.", "Rebuild", "Cancel"))
                return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Build();
        }

        public static void Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var b = new GreyboxBuilder(scene, "L1_OakRoad");

            var root = b.Root.gameObject.AddComponent<LevelRoot>();
            root.displayName = "THE OAK ROAD";
            root.bounds = new Rect(0f, 0f, 232f, 24f);
            root.spawnPoint = b.Empty("PlayerSpawn", 3.5f, 3f).transform;

            // ---------------- 0:00 Village road ----------------
            b.Ground(0, 32, 3);
            b.CoinArc(6, 4.5f, 3, 0.6f);
            b.Coins(12, 4f, 3);

            // ---------------- P1-01 Sling lesson: bell opens the gate ----------------
            var bell = b.Place<Bell>("Bell", 20.5f, 3f, "P1-01 Bell");
            var gate1 = b.Place<Gate>("Gate", 24.5f, 3f, "P1-01 Gate");
            gate1.height = 4; gate1.latch = true; GreyboxBuilder.Commit(gate1);
            b.Stone(24, 24, 7, 17); // wall above the gate: no jumping over
            b.Wire(bell, gate1);
            b.Coins(26, 4f, 3);

            // ---------------- P1-02 Rope bridge over the stream ----------------
            b.Earth(33, 38, 0, 1);
            var stream = b.Place<WaterPool>("WaterPool", 36f, 1f, "P1-02 Stream");
            stream.width = 6f; stream.depth = 1.7f; GreyboxBuilder.Commit(stream); // surface a little below the banks
            b.Ground(39, 48, 3);
            // Hinged on the near bank, held upright by a guy-rope staked behind the player: in plain view.
            var bridge = b.Place<Drawbridge>("Drawbridge", 33f, 3f, "P1-02 Drawbridge");
            bridge.length = 6; bridge.spanLeft = false; GreyboxBuilder.Commit(bridge);
            var rope = b.Place<Rope>("Rope", 33.2f, 8.8f, "P1-02 Rope");
            rope.toEnd = new Vector2(-4.7f, -5.08f); GreyboxBuilder.Commit(rope); // ends at the knot on its stake
            b.Wire(rope, bridge);
            b.Coins(34, 4.2f, 3);

            // ---------------- Patrol + Secret 1A (cracked wall) ----------------
            b.Place("RomanSoldier", 45.5f, 3f, "Legionary 1");
            b.Coins(41, 4f, 2);
            b.Earth(49, 50, 0, 5);           // steps
            b.Ground(51, 52, 3);             // gap in front of the cliff face
            b.Earth(53, 66, 0, 7);           // cliff
            b.Clear(54, 58, 3, 5);           // hidden chamber
            b.Clear(53, 53, 3, 5);           // the cracked stones replace the cliff face
            var cracked = b.Place<CrackedWall>("CrackedWall", 53.5f, 3f, "Secret 1A Cracked Wall");
            cracked.height = 2; GreyboxBuilder.Commit(cracked);
            b.Secret("THE SMITHS' OATH", 54, 3, 58, 5, false, Rock);
            var ogham = b.Place<Collectible>("Collectible_Ogham", 56.5f, 3.6f, "Ogham 1A");
            ogham.displayName = "THE SMITHS' OATH"; GreyboxBuilder.Commit(ogham);

            // ---------------- 3:30 Tavo's broken cart: the Heartwood Torc ----------------
            b.Place("Checkpoint", 56.5f, 7f, "Checkpoint Cliff");
            b.Coins(58, 8f, 3);
            b.Place("Decor_BrokenCart", 61.5f, 7f, "Tavo's Cart");
            b.Place("HeartwoodTorc", 64.5f, 7.7f, "Heartwood Torc");

            // ---------------- P1-03 Rootgrow lesson: raise a root to reach the ledge ----------------
            b.Ground(67, 75, 3);
            b.Place("SapKnot", 74.5f, 3f, "P1-03 Knot");                // max 3 -> top 6, ledge 8
            b.Earth(76, 82, 0, 8);
            b.Coins(77, 9f, 3);

            // Secret 1B: a tall root climbs into the hidden canopy nest
            var canopyKnot = b.Place<SapKnot>("SapKnot", 80.5f, 8f, "Secret 1B Knot");
            canopyKnot.maxHeight = 5; GreyboxBuilder.Commit(canopyKnot);   // top 13, nest floor 16
            b.Place("Coin", 80.5f, 14.5f); b.Place("Coin", 80.5f, 16f);    // breadcrumb upward
            b.Prop(81, 86, 15, 16);          // woven nest floor (drawn by the nest platform)
            b.Place("Decor_NestPlatform", 84f, 15.06f, "Canopy Nest Platform");
            b.Secret("CANOPY NEST", 80, 15, 87, 20, false, Leaves, leaves: true);
            var torc = b.Place<Collectible>("Collectible_GoldenTorc", 84.5f, 16.6f, "Golden Torc 1B");
            torc.displayName = "CANOPY NEST"; GreyboxBuilder.Commit(torc);

            // ---------------- P1-04 Test: crate on a root is a taller step ----------------
            b.Ground(83, 93, 3);
            b.Place("Crate", 89.5f, 3f, "P1-04 Crate");
            b.Place("SapKnot", 93.5f, 3f, "P1-04 Knot");                // root 6 alone < wall 10; with crate 7 -> ok
            b.Earth(94, 99, 0, 10);
            b.Coins(95, 11f, 3);

            // ---------------- P1-05 Twist: dam the stream, drain the pool, open the tunnel ----------------
            b.Ground(100, 103, 4);
            b.Place("Checkpoint", 100.5f, 4f, "Checkpoint Dam");
            var damKnot = b.Place<SapKnot>("SapKnot", 101.5f, 4f, "P1-05 Dam Knot");
            damKnot.maxHeight = 2; GreyboxBuilder.Commit(damKnot);
            var brook = b.Place<WaterPool>("WaterPool", 102f, 4f, "P1-05 Brook");
            brook.width = 4f; brook.depth = 0.35f; brook.hazard = false;
            // Silt and reeds piled against the root dam appear as the water drains.
            brook.revealOnDrain = b.Place("Decor_DamDebris", 102.9f, 4f, "P1-05 Dam Debris").GetComponent<SpriteRenderer>();
            GreyboxBuilder.Commit(brook);
            b.Earth(104, 109, 0, 1);         // pool bed
            var pool = b.Place<WaterPool>("WaterPool", 107f, 1f, "P1-05 Pool");
            pool.width = 6f; pool.depth = 2.4f; GreyboxBuilder.Commit(pool);
            b.Wire(damKnot, pool, brook);
            b.Earth(110, 115, 0, 12);        // rock wall...
            b.Clear(110, 115, 1, 3);         // ...with a tunnel at the pool bed
            b.Coins(111, 1.5f, 3);
            b.Earth(116, 116, 0, 2);         // step out of the tunnel

            // Secret 1C: a chamber under the dam, reachable only from the dry pool bed
            b.Clear(102, 103, 1, 3);
            b.Secret("UNDER THE DAM", 102, 1, 103, 3, false, Rock);
            b.Place("Collectible_ShrineShard", 102.5f, 1.6f, "Shard 1C");

            // ---------------- P1-06 Combo: root lifts the crate, sling knocks it onto the plate ----------------
            b.Ground(117, 122, 3);
            b.Place("Checkpoint", 118.5f, 3f, "Checkpoint Tunnel");
            b.Coins(119, 4f, 2);
            b.Place("SapKnot", 122.5f, 3f, "P1-06 Knot");               // lifts the crate to the ledge (6)
            b.Place("Crate", 122.5f, 3f, "P1-06 Crate");
            b.Earth(123, 126, 0, 6);         // ledge
            b.Earth(127, 127, 0, 7);         // stopper
            var plate6 = b.Place<PressurePlate>("PressurePlate", 126.5f, 6f, "P1-06 Plate");
            b.Ground(128, 232, 3);
            var gate6 = b.Place<Gate>("Gate", 130.5f, 3f, "P1-06 Gate");
            gate6.height = 4; GreyboxBuilder.Commit(gate6); // held open only while the plate is pressed
            b.Stone(130, 130, 7, 17);
            b.Wire(plate6, gate6);

            // ---------------- P1-T Druid's Trial (optional room above the path) ----------------
            var trialKnot = b.Place<SapKnot>("SapKnot", 133.5f, 3f, "P1-T Entrance Knot");
            trialKnot.maxHeight = 5; GreyboxBuilder.Commit(trialKnot);
            b.Stone(134, 153, 5, 9);         // trial floor (top 9); the path runs underneath
            b.Stone(134, 153, 17, 18);       // ceiling
            b.Stone(153, 153, 9, 17);        // right wall
            b.Stone(140, 141, 9, 11);        // shelf S1 (11)
            b.Stone(143, 144, 9, 13);        // shelf S2 (13)
            b.Stone(145, 145, 9, 15);        // stopper / divider (15)
            b.Stone(151, 151, 13, 17);       // wall above the reward gate
            var resetBell = b.Place<Bell>("Bell", 135.5f, 9f, "P1-T Reset Bell");
            resetBell.staysOn = false; GreyboxBuilder.Commit(resetBell);
            var trialCrate = b.Place<Crate>("Crate", 137.5f, 9f, "P1-T Crate");
            trialCrate.signalMode = Crate.SignalMode.RespawnOnSignal; GreyboxBuilder.Commit(trialCrate);
            b.Wire(resetBell, trialCrate);
            var k1 = b.Place<SapKnot>("SapKnot", 139.5f, 9f, "P1-T Knot 1");
            k1.maxHeight = 2; GreyboxBuilder.Commit(k1);
            var k2 = b.Place<SapKnot>("SapKnot", 142.5f, 9f, "P1-T Knot 2");
            k2.maxHeight = 4; GreyboxBuilder.Commit(k2);
            var plateA = b.Place<PressurePlate>("PressurePlate", 144.5f, 13f, "P1-T Plate A");
            var plateB = b.Place<PressurePlate>("PressurePlate", 148.5f, 9f, "P1-T Plate B");
            var trialGate = b.Place<Gate>("Gate", 151.5f, 9f, "P1-T Gate");
            trialGate.height = 4; trialGate.requiredInputs = 2; trialGate.latch = true; GreyboxBuilder.Commit(trialGate);
            b.Wire(plateA, trialGate);
            b.Wire(plateB, trialGate);
            b.Secret("THE ROOTED STONES", 134, 9, 152, 17, true);
            b.Place("Collectible_DruidMark", 152.5f, 9.6f, "Druid's Mark");
            b.Zone("Trial", 131f, 5f, 156f, 19f);
            b.Place("RomanSoldier", 147.5f, 3f, "Legionary 2");
            b.Coins(137, 3.6f, 4, 2f);

            // ---------------- P1-07 Set piece: boar stampede ----------------
            b.Place("Checkpoint", 158.5f, 3f, "Checkpoint Chase");
            var chaseGo = b.Empty("P1-07 Stampede", 162.5f, 8f);
            var trigger = chaseGo.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = new Vector2(1f, 10f);
            var chase = chaseGo.AddComponent<StampedeChase>();
            chase.frontStartX = 150f;
            chase.frontEndX = 213f;
            chase.restartPoint = b.Empty("P1-07 Restart", 160.5f, 3f).transform;
            var c1 = b.Place<SapKnot>("SapKnot", 168.5f, 3f, "P1-07 Knot 1");
            var c2 = b.Place<SapKnot>("SapKnot", 182.5f, 3f, "P1-07 Knot 2");
            var c3 = b.Place<SapKnot>("SapKnot", 195.5f, 3f, "P1-07 Knot 3");
            chase.knotsToReset = new[] { c1, c2, c3 };
            b.Earth(170, 172, 0, 8);
            b.Earth(184, 187, 0, 9);
            b.Earth(197, 199, 0, 8);
            b.Prop(214, 215, 3, 6);          // fallen oak: the herd crashes into it
            var oak = b.Place("Decor_FallenOak", 214.9f, 3f, "Fallen Oak");
            oak.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            b.Coins(170.5f, 9f, 3);
            b.Coins(184.5f, 10f, 4);
            b.Coins(197.5f, 9f, 3);

            // ---------------- 9:30 Friendly banner ----------------
            b.CoinArc(218, 4f, 5, 1.5f);
            b.Place("Goal", 225.5f, 3f, "Goal Banner");

            b.BuildTilemaps();
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[Ironoak] Level 1 greybox written to " + ScenePath);
        }
    }
}
