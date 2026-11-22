using System.Collections;
using System.IO;
using System.Text;
using Ironoak.Controls;
using Ironoak.Gameplay;
using Ironoak.Powers;
using Ironoak.Puzzles;
using UnityEngine;

namespace Ironoak.Core
{
    /// <summary>
    /// Automated playtest of Level 1. Inactive unless the player is launched with
    /// "-ironoak-playtest &lt;folder&gt;". It drives the real input and physics through every puzzle,
    /// logs PASS/FAIL lines ("[PLAYTEST]") plus a summary, saves screenshots, then quits.
    /// </summary>
    public class DevPlaytest : MonoBehaviour
    {
        string _dir;
        int _pass, _fail;
        readonly StringBuilder _report = new StringBuilder();
        IronoakGame _game;
        PlayerController _p;
        readonly System.Collections.Generic.List<float> _frameMs = new System.Collections.Generic.List<float>(20000);
        int _skipFrames;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] != "-ironoak-playtest") continue;
                var go = new GameObject("DevPlaytest");
                DontDestroyOnLoad(go);
                go.AddComponent<DevPlaytest>()._dir = args[i + 1];
                return;
            }
        }

        // ---------- helpers ----------

        static T Find<T>(string name) where T : Component
        {
            var go = GameObject.Find(name);
            return go != null ? go.GetComponent<T>() : null;
        }

        void Check(string name, bool ok, string detail = "")
        {
            if (ok) _pass++; else _fail++;
            string line = $"[PLAYTEST] {(ok ? "PASS" : "FAIL")} {name} {detail}";
            _report.AppendLine(line);
            Debug.Log(line);
        }

        IEnumerator Wait(float s) { yield return new WaitForSeconds(s); }

        /// <summary>Samples frame times during play (screenshot frames excluded) for the PERF line.</summary>
        void Update()
        {
            if (_skipFrames > 0) { _skipFrames--; return; }
            if (_game != null && _game.State == GameState.Playing) _frameMs.Add(Time.unscaledDeltaTime * 1000f);
        }

        IEnumerator Shot(string name)
        {
            _skipFrames = 4;
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(_dir, name + ".png"));
            yield return null;
        }

        void Put(float x, float y) => _p.Teleport(new Vector2(x, y));

        IEnumerator Hold(float h, bool jump, float seconds)
        {
            GameInput.TestHorizontal = h;
            GameInput.TestJumpHeld = jump;
            yield return new WaitForSeconds(seconds);
            GameInput.TestHorizontal = 0f;
            GameInput.TestJumpHeld = false;
            yield return new WaitForSeconds(0.05f);
        }

        float _apex;

        IEnumerator RunJump(float h, float seconds)
        {
            GameInput.TestHorizontal = h;
            GameInput.TestJumpHeld = true;
            _apex = float.MinValue;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                _apex = Mathf.Max(_apex, _p.transform.position.y);
                yield return null;
            }
            GameInput.TestHorizontal = 0f;
            GameInput.TestJumpHeld = false;
            yield return new WaitForSeconds(0.3f);
        }

        IEnumerator PressPower()
        {
            GameInput.TestPressPower();
            yield return new WaitForSeconds(0.9f);
        }

        void Stone(Vector2 from, Vector2 to)
        {
            Vector2 dir = (to - from).normalized;
            SlingStone.Spawn(from, dir * 16f, 0f, _p.Collider);
        }

        /// <summary>Close-up shots of the player puppet in its main poses, for visual review.</summary>
        IEnumerator PuppetShowcase()
        {
            var cam = PixelDisplay.Instance.WorldCamera;
            var rig = cam.GetComponent<CameraRig>();
            float size = cam.orthographicSize;
            rig.enabled = false;
            void Frame() => cam.transform.position = new Vector3(_p.transform.position.x, _p.transform.position.y + 0.7f, -10f);

            cam.orthographicSize = 1.6f;
            Put(10f, 3f);
            yield return Wait(0.8f);
            Frame(); yield return Shot("puppet_1_idle");

            GameInput.TestHorizontal = 1f;
            yield return Wait(0.45f);
            Frame(); yield return Shot("puppet_2_run_a");
            yield return Wait(0.13f);
            Frame(); yield return Shot("puppet_3_run_b");
            GameInput.TestHorizontal = 0f;
            yield return Wait(0.6f);

            GameInput.TestJumpHeld = true;
            yield return Wait(0.12f);
            Frame(); yield return Shot("puppet_4_jump");
            yield return Wait(0.45f);
            Frame(); yield return Shot("puppet_5_fall");
            GameInput.TestJumpHeld = false;
            yield return Wait(0.8f);

            GameInput.TestFireHeld = true;
            yield return Wait(0.5f);
            Frame(); yield return Shot("puppet_6_aim");
            GameInput.TestFireHeld = false;
            yield return Wait(0.1f);
            Frame(); yield return Shot("puppet_7_throw");
            yield return Wait(0.5f);

            cam.orthographicSize = size;
            rig.enabled = true;
            rig.Follow(_p.Body, _game.Level.bounds);
        }

        // ---------- the run ----------

        IEnumerator Start()
        {
            Directory.CreateDirectory(_dir);
            GameInput.TestMode = true;
            yield return Wait(2.5f);
            yield return Shot("01_title");

            _game = IronoakGame.Instance;
            _game.StartGame();
            float t = 0f;
            while (_game.State != GameState.Playing && t < 15f) { t += Time.unscaledDeltaTime; yield return null; }
            Check("Level loads", _game.State == GameState.Playing && _game.Player != null);
            if (_game.Player == null) { Finish(); yield break; }
            _p = _game.Player;
            yield return Wait(1f);
            yield return Shot("02_level_start");
            yield return PuppetShowcase();

            // P1-01 bell -> gate
            var bell = Find<Bell>("P1-01 Bell");
            var gate1 = Find<Gate>("P1-01 Gate");
            Put(17.5f, 3f);
            yield return Wait(0.3f);
            Stone(new Vector2(18f, 4.2f), (Vector2)bell.transform.position + new Vector2(0.22f, 2.2f));
            yield return Wait(1.3f);
            Check("P1-01 bell opens gate", gate1.IsOpen);
            yield return Shot("03_p1-01_gate");

            // P1-02 rope -> bridge, then walk across
            var rope = Find<Rope>("P1-02 Rope");
            var bridge = Find<Drawbridge>("P1-02 Drawbridge");
            Put(26f, 3f);
            yield return Wait(0.3f);
            Stone(new Vector2(26.5f, 4f), (Vector2)rope.transform.position + rope.toEnd * 0.5f);
            yield return Wait(1.4f);
            Check("P1-02 rope drops bridge", bridge.IsDown);
            Put(31f, 3f);
            yield return Hold(1f, false, 1.3f);
            Check("P1-02 bridge is walkable", _p.transform.position.x > 38.5f && _p.transform.position.y > 2.8f, $"pos={_p.transform.position}");
            yield return Shot("04_p1-02_bridge");

            // Secret 1A
            var cracked = Find<CrackedWall>("Secret 1A Cracked Wall");
            Put(51.5f, 3f);
            yield return Wait(0.3f);
            Stone(new Vector2(52f, 3.8f), new Vector2(53.5f, 3.8f));
            yield return Wait(0.6f);
            Check("Secret 1A wall breaks", cracked == null || !cracked.gameObject.activeSelf);
            yield return Hold(1f, false, 1.2f);
            Check("Secret 1A found", _game.SecretsFound >= 1, $"secrets={_game.SecretsFound}");
            yield return Shot("05_secret_1a");

            // Torc
            Put(63.5f, 7f);
            yield return Hold(1f, false, 0.6f);
            Check("Heartwood Torc unlocks Rootgrow", _p.Powers.Has(Power.Rootgrow));
            yield return Wait(0.5f);
            yield return Shot("06_torc");

            // P1-03 root lesson
            var k3 = Find<SapKnot>("P1-03 Knot");
            Put(73.4f, 3f);
            yield return Wait(0.3f);
            yield return PressPower();
            Check("P1-03 knot raises", k3.Raised);
            Put(74.5f, 6.1f);
            yield return Wait(0.3f);
            yield return RunJump(1f, 0.7f);
            Check("P1-03 ledge reachable", _p.transform.position.y > 7.9f && _p.transform.position.x > 76f, $"pos={_p.transform.position} apex={_apex:F2}");
            yield return Shot("07_p1-03");

            // Secret 1B canopy
            var k1b = Find<SapKnot>("Secret 1B Knot");
            int before = _game.SecretsFound;
            Put(79.6f, 8f);
            yield return Wait(0.3f);
            yield return PressPower();
            Check("Secret 1B knot raises", k1b.Raised);
            Put(80.5f, 13.1f);
            yield return Wait(0.3f);
            yield return RunJump(1f, 0.8f);
            yield return Hold(1f, false, 0.6f);
            Check("Secret 1B canopy reachable", _game.SecretsFound > before, $"pos={_p.transform.position}");
            yield return Shot("08_secret_1b");

            // P1-04 crate + root
            var crate4 = Find<Crate>("P1-04 Crate");
            Put(87.5f, 3f);
            yield return Wait(0.3f);
            yield return Hold(1f, false, 1.6f);
            Check("P1-04 crate pushed onto knot", Mathf.Abs(crate4.transform.position.x - 93.5f) < 0.45f, $"crate={crate4.transform.position}");
            Put(93.5f, 4.1f);
            yield return Wait(0.3f);
            yield return PressPower();
            Check("P1-04 root lifts crate", crate4.transform.position.y > 5.8f, $"crate={crate4.transform.position}");
            Put(93.5f, crate4.transform.position.y + 1.05f);
            yield return Wait(0.4f);
            float startY = _p.transform.position.y;
            yield return RunJump(0f, 0.8f);
            Debug.Log($"[PLAYTEST] DIAG crate vertical jump: start={startY:F2} apex={_apex:F2} grounded={_p.Grounded}");
            Put(88f, 3.05f);
            yield return Wait(0.4f);
            yield return RunJump(0f, 0.8f);
            Debug.Log($"[PLAYTEST] DIAG flat ground jump: apex={_apex:F2}");
            Put(93.5f, crate4.transform.position.y + 1.05f);
            yield return Wait(0.4f);
            yield return RunJump(1f, 0.8f);
            Check("P1-04 wall top reachable", _p.transform.position.y > 9.9f && _p.transform.position.x > 94f, $"pos={_p.transform.position} apex={_apex:F2}");
            yield return Shot("09_p1-04");

            // P1-05 dam, drain, tunnel, secret 1C
            var pool = Find<WaterPool>("P1-05 Pool");
            Put(100.6f, 4f);
            yield return Wait(0.3f);
            yield return PressPower();
            yield return Wait(2.6f);
            Check("P1-05 dam drains pool", pool == null || !pool.gameObject.activeSelf || pool.Drained);
            yield return Shot("10_p1-05_drained");
            before = _game.SecretsFound;
            Put(105.5f, 1.1f);
            yield return Hold(-1f, false, 1f);
            Check("Secret 1C found", _game.SecretsFound > before);
            yield return Hold(1f, false, 2.2f);
            yield return RunJump(1f, 0.6f);
            yield return RunJump(1f, 0.6f);
            Check("P1-05 tunnel passable", _p.transform.position.x > 116.5f, $"pos={_p.transform.position}");

            // P1-06 root + sling -> plate -> gate
            var crate6 = Find<Crate>("P1-06 Crate");
            var gate6 = Find<Gate>("P1-06 Gate");
            Put(121.2f, 3f);
            yield return Wait(0.3f);
            yield return PressPower();
            Check("P1-06 root lifts crate to ledge height", crate6.transform.position.y > 5.8f, $"crate={crate6.transform.position}");
            Put(119.5f, 3f);
            for (int i = 0; i < 3 && !gate6.Powered; i++)
            {
                Vector2 c = (Vector2)crate6.transform.position + new Vector2(0f, 0.5f);
                Stone(c + new Vector2(-1.2f, 0f), c);
                yield return Wait(1.4f);
            }
            Check("P1-06 crate on plate opens gate", gate6.Powered, $"crate={crate6.transform.position}");
            yield return Wait(0.6f);
            yield return Shot("11_p1-06");

            // P1-T Druid's Trial
            var trialKnot = Find<SapKnot>("P1-T Entrance Knot");
            Put(132.4f, 3f);
            yield return Wait(0.3f);
            yield return PressPower();
            Check("P1-T entrance knot raises", trialKnot.Raised);
            Put(134.6f, 9.05f);
            yield return Wait(0.5f);
            Check("P1-T room discovered", _game.TrialFound);
            var tc = Find<Crate>("P1-T Crate");
            var tk1 = Find<SapKnot>("P1-T Knot 1");
            var tk2 = Find<SapKnot>("P1-T Knot 2");
            yield return Hold(1f, false, 1.4f);
            Check("P1-T crate onto knot 1", Mathf.Abs(tc.transform.position.x - 139.5f) < 0.45f, $"crate={tc.transform.position}");
            Put(138f, 9.05f);
            yield return Wait(0.2f);
            yield return PressPower();
            Check("P1-T knot 1 raises crate", tk1.Raised && tc.transform.position.y > 10.8f, $"crate={tc.transform.position}");
            Stone(new Vector2(138.2f, 11.5f), new Vector2(139.5f, 11.5f));
            yield return Wait(1.6f);
            Check("P1-T crate drops onto knot 2", Mathf.Abs(tc.transform.position.x - 142.5f) < 0.45f && tc.transform.position.y < 9.5f, $"crate={tc.transform.position}");
            Put(142.5f, tc.transform.position.y + 1.05f);
            yield return Wait(0.3f);
            yield return PressPower();
            Check("P1-T knot 2 raises crate", tk2.Raised && tc.transform.position.y > 12.8f, $"crate={tc.transform.position}");
            Put(140.5f, 11.05f);
            yield return Wait(0.4f);
            var plateA = Find<PressurePlate>("P1-T Plate A");
            for (int i = 0; i < 3 && !plateA.Pressed; i++)
            {
                Vector2 c = (Vector2)tc.transform.position + new Vector2(0f, 0.5f);
                Stone(c + new Vector2(-1.2f, 0f), c);
                yield return Wait(1.4f);
            }
            Check("P1-T crate reaches plate A", plateA.Pressed, $"crate={tc.transform.position}");
            Put(148.5f, 9.05f);
            yield return Wait(1.2f);
            var trialGate = Find<Gate>("P1-T Gate");
            Check("P1-T both plates open reward gate", trialGate.Powered);
            yield return Shot("12_trial");
            yield return Hold(1f, false, 1.6f);
            Check("P1-T Druid's Mark collected", GameObject.Find("Druid's Mark") == null);

            // P1-07 stampede
            var chase = Find<StampedeChase>("P1-07 Stampede");
            var health = _p.GetComponent<PlayerHealth>();
            Put(161.5f, 3f);
            yield return Hold(1f, false, 0.25f);
            Check("P1-07 stampede starts", chase.Running);
            yield return Shot("13_stampede");
            int hp = health.Health;
            yield return Wait(3.2f);
            Check("P1-07 caught costs a heart and restarts", health.Health == hp - 1 && chase.Running && _p.transform.position.x < 162f, $"hp={health.Health} pos={_p.transform.position}");
            Put(181.3f, 3f);
            yield return Wait(0.1f);
            yield return PressPower();
            Put(182.5f, 6.1f);
            yield return Wait(0.2f);
            yield return RunJump(1f, 0.7f);
            Check("P1-07 wall 2 climbable with root", _p.transform.position.y > 8.9f && _p.transform.position.x > 184f, $"pos={_p.transform.position}");
            Put(217f, 3f);
            yield return Wait(2.5f);
            Check("P1-07 stampede ends at the fallen oak", chase.Done && !health.Dead);

            // Goal
            Put(223.5f, 3f);
            yield return Hold(1f, false, 1f);
            Check("Goal completes level", _game.State == GameState.LevelComplete);
            yield return Wait(1.8f);
            yield return Shot("14_result");
            Finish();
        }

        void Finish()
        {
            if (_frameMs.Count > 0)
            {
                var sorted = new System.Collections.Generic.List<float>(_frameMs);
                sorted.Sort();
                float avg = 0f;
                foreach (var f in sorted) avg += f;
                avg /= sorted.Count;
                float p95 = sorted[(int)(sorted.Count * 0.95f)], p99 = sorted[(int)(sorted.Count * 0.99f)];
                string perf = $"[PLAYTEST] PERF frames={sorted.Count} avg={avg:F2}ms ({1000f / avg:F0} fps) p95={p95:F2}ms p99={p99:F2}ms worst={sorted[sorted.Count - 1]:F1}ms " +
                              $"res={Screen.width}x{Screen.height} gpu={SystemInfo.graphicsDeviceName}";
                _report.AppendLine(perf);
                Debug.Log(perf);
            }
            string summary = $"[PLAYTEST] RESULT {_pass}/{_pass + _fail} passed";
            _report.AppendLine(summary);
            Debug.Log(summary);
            File.WriteAllText(Path.Combine(_dir, "playtest.txt"), _report.ToString());
            GameInput.TestMode = false;
            Application.Quit();
        }
    }
}
