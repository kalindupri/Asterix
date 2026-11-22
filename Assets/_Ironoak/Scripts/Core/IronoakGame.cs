using System.Collections;
using Ironoak.Controls;
using Ironoak.Gameplay;
using Ironoak.Screens;
using Ironoak.Sound;
using UnityEngine;

namespace Ironoak.Core
{
    public enum GameState { Title, Playing, Paused, LevelComplete, GameOver }

    /// <summary>
    /// Composition root and game-flow state machine. The Main scene contains only this
    /// component; it builds the display, background, UI and levels at runtime.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class IronoakGame : MonoBehaviour
    {
        const string LevelResource = "Levels/Level01";

        public static IronoakGame Instance { get; private set; }
        public GameState State { get; private set; }

        PixelDisplay _display;
        CameraRig _rig;
        Level _level;
        TitleScreen _title;
        Hud _hud;
        PauseMenu _pause;
        ResultScreen _result;
        Coroutine _pending;

        int _coins, _defeated;
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

            GameEvents.CoinCollected += OnCoinCollected;
            GameEvents.EnemyDefeated += OnEnemyDefeated;
            GameEvents.GoalReached += OnGoalReached;

            ShowTitle();
        }

        void OnDestroy()
        {
            if (Instance != this) return;
            GameEvents.CoinCollected -= OnCoinCollected;
            GameEvents.EnemyDefeated -= OnEnemyDefeated;
            GameEvents.GoalReached -= OnGoalReached;
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

        public void ShowTitle()
        {
            CancelPending();
            Time.timeScale = 1f;
            LoadLevel(false);
            _rig.AutoPan(_level.Bounds, 1.5f);
            SetState(GameState.Title);
        }

        public void StartGame()
        {
            CancelPending();
            Time.timeScale = 1f;
            LoadLevel(true);
            _coins = 0;
            _defeated = 0;
            _time = 0f;

            var player = _level.Player;
            player.Health.HealthChanged += _hud.SetHealth;
            player.Health.Died += OnPlayerDied;
            _rig.Follow(player.Body, _level.Bounds);
            _hud.ResetHud(player.Health.Health, 0, _level.TotalCoins, _level.Name);
            SetState(GameState.Playing);
        }

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

        void SetState(GameState state)
        {
            State = state;
            _title.gameObject.SetActive(state == GameState.Title);
            _hud.gameObject.SetActive(state != GameState.Title);
            _pause.gameObject.SetActive(state == GameState.Paused);
            _result.gameObject.SetActive(false);
        }

        void LoadLevel(bool withPlayer)
        {
            if (_level != null && _level.Root != null) Destroy(_level.Root);
            var asset = Resources.Load<TextAsset>(LevelResource);
            if (asset == null)
            {
                Debug.LogError($"[Ironoak] Missing level file Resources/{LevelResource}.txt");
                return;
            }
            _level = LevelBuilder.Build(asset.text, transform, withPlayer);
        }

        // ---------- events ----------

        void OnCoinCollected()
        {
            if (State != GameState.Playing) return;
            _coins++;
            _hud.SetCoins(_coins, _level.TotalCoins);
        }

        void OnEnemyDefeated()
        {
            if (State == GameState.Playing) _defeated++;
        }

        void OnGoalReached()
        {
            if (State != GameState.Playing) return;
            State = GameState.LevelComplete;
            _level.Player.InputEnabled = false;
            Sfx.Play(SfxId.Win);
            bool newBest = SaveData.Submit(_coins, _time);
            string stats =
                $"COINS     {_coins}/{_level.TotalCoins}\n" +
                $"LEGIONARIES  {_defeated}\n" +
                $"TIME      {SaveData.FormatTime(_time)}" +
                (newBest ? "\n\nNEW BEST!" : "");
            _pending = StartCoroutine(ShowResultAfter(1.2f, true, stats));
        }

        void OnPlayerDied()
        {
            if (State != GameState.Playing) return;
            State = GameState.GameOver;
            string stats = $"COINS  {_coins}/{_level.TotalCoins}\nTIME   {SaveData.FormatTime(_time)}";
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
