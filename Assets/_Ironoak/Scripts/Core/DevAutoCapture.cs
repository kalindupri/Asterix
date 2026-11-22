using System.Collections;
using System.IO;
using UnityEngine;

namespace Ironoak.Core
{
    /// <summary>
    /// Dev-only smoke test. Inactive unless the player is launched with
    /// "-ironoak-capture &lt;folder&gt;": it then screenshots the title screen and a few
    /// gameplay moments, and quits. Used for automated visual checks of builds.
    /// </summary>
    public class DevAutoCapture : MonoBehaviour
    {
        string _dir;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            var args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] != "-ironoak-capture") continue;
                var go = new GameObject("DevAutoCapture");
                DontDestroyOnLoad(go);
                go.AddComponent<DevAutoCapture>()._dir = args[i + 1];
                return;
            }
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(_dir);
            yield return new WaitForSecondsRealtime(2.5f);
            yield return Shot("01_title");

            var game = IronoakGame.Instance;
            game.StartGame();
            yield return new WaitForSecondsRealtime(1.0f);
            yield return Shot("02_level_start");

            var player = FindAnyObjectByType<Gameplay.PlayerController>();
            if (player != null)
            {
                player.Teleport(new Vector2(36f, 6f));
                yield return new WaitForSecondsRealtime(1.2f);
                yield return Shot("03_spike_platforms");

                player.Teleport(new Vector2(60f, 9f));
                yield return new WaitForSecondsRealtime(0.4f);
                yield return Shot("04_pit_jump");
            }

            game.Pause();
            yield return new WaitForSecondsRealtime(0.5f);
            yield return Shot("05_pause");
            game.Resume();

            if (player != null)
            {
                player.Teleport(new Vector2(96.5f, 3f));
                yield return new WaitForSecondsRealtime(2.5f);
                yield return Shot("06_result");
            }
            Application.Quit();
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(_dir, name + ".png"));
            yield return null;
            yield return null;
        }
    }
}
