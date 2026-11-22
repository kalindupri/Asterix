using System.Collections;
using Ironoak.Controls;
using Ironoak.Gameplay;
using Ironoak.Powers;
using Ironoak.Screens;
using Ironoak.Sound;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ironoak.Core
{
    public enum GameState { Loading, Title, Playing, Paused, LevelComplete, GameOver }

    /// <summary>
    /// Composition root and game-flow state machine. Lives in the Main scene and builds the
    /// display, background and UI. Levels are separate scenes (with a LevelRoot) that are
    /// loaded additively; the player is spawned into the level scene at runtime.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class IronoakGame : MonoBehaviour
    {
        public static readonly string[] LevelScenes = { "L1_OakRoad" };

        public static IronoakGame Instance { get; private set; }
        public GameState State { get; private set; } = GameState.Loading;
        public LevelRoot Level => _level;
        public PlayerController Player => _player;
        public int SecretsFound => _secrets;
        public bool TrialFound => _trial;

        PixelDisplay _display;
        CameraRig _rig;
        LevelRoot _level;
        Scene _levelScene;
        PlayerController _player;
        TitleScreen _title;
        Hud _hud;
        PauseMenu _pause;
        ResultScreen _result;
        FadeOverlay _fade;
        Coroutine _pending;
        bool _busy;

        int _coins, _totalCoins, _defeated, _secrets, _totalSecrets, _collectibles;
        bool _trial;
        float _time;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
            Physics2D.queriesStartInColliders = false;

            gameObject.AddComponent<Sfx>();
            _display = PixelDisplay.Create(transform);
            _rig = _display.WorldCamera.gameObject.AddComponent<CameraRig>();
            ParallaxBackground.Create(transform, _display.WorldCamera);

            var ui = _display.UICanvas.transform;
            _hud = Hud.Create(ui);
            _title = TitleScreen.Create(ui, this);
            _pause = PauseMenu.Create(ui, this);
            _result = ResultScreen.Create(ui, this);
            _fade = FadeOverlay.Create(ui);
            SetState(GameState.Loading);

            GameEvents.CoinCollected += OnCoinCollected;
            GameEvents.EnemyDefeated += OnEnemyDefeated;
            GameEvents.GoalReached += OnGoalReached;
            GameEvents.SecretFound += OnSecretFound;
            GameEvents.CollectibleFound += OnCollectibleFound;
            GameEvents.PowerUnlocked += OnPowerUnlocked;
            GameEvents.Message += OnMessage;
        }

        void Start() => ShowTitle();

        void OnDestroy()
        {
            if (Instance != this) return;
            GameEvents.CoinCollected -= OnCoinCollected;
            GameEvents.EnemyDefeated -= OnEnemyDefeated;
            GameEvents.GoalReached -= OnGoalReached;
            GameEvents.SecretFound -= OnSecretFound;
            GameEvents.CollectibleFound -= OnCollectibleFound;
            GameEvents.PowerUnlocked -= OnPowerUnlocked;
            GameEvents.Message -= OnMessage;
            Time.timeScale = 1f;
            Instance = null;
        }

        void Update()
        {
            GameInput.Tick();

            if (State == GameState.Playing)
            {
                _time += Time.deltaTime;
                _hud.SetTime(_time);
                if (GameInput.PausePressed) Pause();
            }
        }

        // ---------- flow ----------

        public void ShowTitle() => StartCoroutine(Transition(false));
        public void StartGame() => StartCoroutine(Transition(true));

        public void Pause()
        {
            if (State != GameState.Playing) return;
            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void Resume()
        {
            if (State != GameState.Paused) return;
            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        IEnumerator Transition(bool play)
        {
            if (_busy) yield break;
            _busy = true;
            CancelPending();
            Time.timeScale = 1f;
            yield return _fade.FadeTo(1f, 0.25f);
            SetState(GameState.Loading);

            yield return LoadLevel(LevelScenes[0]);
            if (_level == null)
            {
                Debug.LogError("[Ironoak] Level scene has no LevelRoot. Run Ironoak > Build Level 1 Greybox.");
                _busy = false;
                yield break;
            }

            if (play) BeginRun();
            else
            {
                _rig.AutoPan(_level.bounds, 1.5f);
                SetState(GameState.Title);
            }
            yield return _fade.FadeTo(0f, 0.35f);
            _busy = false;
        }

        IEnumerator LoadLevel(string sceneName)
        {
            _level = null;
            _player = null;
            if (_levelScene.IsValid() && _levelScene.isLoaded)
            {
                var unload = SceneManager.UnloadSceneAsync(_levelScene);
                while (unload != null && !unload.isDone) yield return null;
            }

            var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (load == null)
            {
                Debug.LogError($"[Ironoak] Scene '{sceneName}' is not in Build Settings.");
                yield break;
            }
            while (!load.isDone) yield return null;

            _levelScene = SceneManager.GetSceneByName(sceneName);
            SceneManager.SetActiveScene(_levelScene); // runtime spawns (player, stones, fx) live and die with the level
            foreach (var go in _levelScene.GetRootGameObjects())
            {
                _level = go.GetComponentInChildren<LevelRoot>();
                if (_level != null) break;
            }
        }

        void BeginRun()
        {
            _player = PlayerController.Spawn(null, _level.Spawn);
            foreach (var p in _level.startingPowers) _player.Powers.Unlock(p, true);
            foreach (var s in FindObjectsByType<RomanSoldier>(FindObjectsSortMode.None)) s.IgnorePlayer(_player.Collider);

            var health = _player.GetComponent<PlayerHealth>();
            health.HealthChanged += _hud.SetHealth;
            health.Died += OnPlayerDied;

            _coins = _defeated = _secrets = _collectibles = 0;
            _trial = false;
            _time = 0f;
            _totalCoins = _level.TotalCoins;
            _totalSecrets = _level.TotalSecrets;

            _rig.Follow(_player.Body, _level.bounds);
            _hud.ResetHud(health.maxHealth, 0, _totalCoins, _level.displayName);
            SetState(GameState.Playing);
        }

        void SetState(GameState state)
        {
            State = state;
            _title.gameObject.SetActive(state == GameState.Title);
            _hud.gameObject.SetActive(state == GameState.Playing || state == GameState.Paused || state == GameState.LevelComplete || state == GameState.GameOver);
            _pause.gameObject.SetActive(state == GameState.Paused);
            _result.gameObject.SetActive(false);
        }

        // ---------- events ----------

        void OnCoinCollected()
        {
            if (State != GameState.Playing) return;
            _coins++;
            _hud.SetCoins(_coins, _totalCoins);
        }

        void OnEnemyDefeated()
        {
            if (State == GameState.Playing) _defeated++;
        }

        void OnSecretFound(SecretArea s)
        {
            if (State != GameState.Playing) return;
            if (s.isTrial)
            {
                _trial = true;
                _hud.ShowBanner("DRUID'S TRIAL", s.secretName, 2.5f);
            }
            else
            {
                _secrets++;
                _hud.Toast($"SECRET FOUND  {_secrets}/{_totalSecrets}");
            }
        }

        void OnCollectibleFound(Collectible c)
        {
            if (State != GameState.Playing) return;
            _collectibles++;
            string label = Collectible.TypeName(c.type);
            _hud.Toast(string.IsNullOrEmpty(c.displayName) ? label : $"{label}: {c.displayName}");
        }

        void OnPowerUnlocked(Power p)
        {
            if (State != GameState.Playing) return;
            string key = GameInput.UsingGamepad ? "Y" : "E";
            _hud.ShowBanner(p == Power.Rootgrow ? "HEARTWOOD TORC" : PlayerPowers.DisplayName(p),
                p == Power.Rootgrow ? $"PRESS {key} AT A GLOWING KNOT" : "NEW POWER", 4f);
        }

        void OnMessage(string text)
        {
            if (State == GameState.Playing) _hud.Toast(text);
        }

        void OnGoalReached()
        {
            if (State != GameState.Playing) return;
            State = GameState.LevelComplete;
            _player.InputEnabled = false;
            Sfx.Play(SfxId.Win);
            bool newBest = SaveData.Submit(_coins, _time);
            string stats =
                $"COINS        {_coins}/{_totalCoins}\n" +
                $"SECRETS      {_secrets}/{_totalSecrets}\n" +
                $"DRUID TRIAL  {(_trial ? "FOUND" : "-")}\n" +
                $"LEGIONARIES  {_defeated}\n" +
                $"TIME         {SaveData.FormatTime(_time)}" +
                (newBest ? "\n\nNEW BEST!" : "");
            _pending = StartCoroutine(ShowResultAfter(1.2f, true, stats));
        }

        void OnPlayerDied()
        {
            if (State != GameState.Playing) return;
            State = GameState.GameOver;
            string stats = $"COINS    {_coins}/{_totalCoins}\nSECRETS  {_secrets}/{_totalSecrets}\nTIME     {SaveData.FormatTime(_time)}";
            _pending = StartCoroutine(ShowResultAfter(1.4f, false, stats));
        }

        IEnumerator ShowResultAfter(float delay, bool win, string stats)
        {
            yield return new WaitForSecondsRealtime(delay);
            _pending = null;
            _result.Show(win, stats);
        }

        void CancelPending()
        {
            if (_pending != null) StopCoroutine(_pending);
            _pending = null;
        }
    }
}
