# Ironoak — 2D Pixel Platformer (POC)

An original platformer set in Gaul, around 50 BC. Brenna, a young Celtic slinger, crosses
the forest road to reach the last free village while dodging a Roman legion.

Unity **6000.3 LTS** (6000.3.26f1). Native resolution 480×270, upscaled pixel-perfect.

## Run it

1. Unity Hub → **Add project from disk** → select `C:\Repos\Asterix`.
2. Open `Assets/_Ironoak/Scenes/Main.unity` (or use the **Ironoak → Open Main Scene** menu).
3. Set the Game view to **16:9** and press **Play**.

| Action | Keyboard | Gamepad |
|---|---|---|
| Move | Arrows / A D | Left stick / D-pad |
| Jump (hold for higher) | Space / W / Z | A |
| Pause | Esc / P | Start |
| Menu select / confirm | Up/Down, Enter / Space | Stick, A |

## What's in the POC

- Title screen with an auto-scrolling level behind it, and a Controls panel.
- One full level, "The Oak Road": pits, spikes, one-way planks, stone stairs, a checkpoint
  torch, 18 coins, 6 legionaries and a goal banner.
- Platforming feel: acceleration curves, coyote time, jump buffer, variable jump height,
  faster falling, squash and stretch.
- 3 hearts, invulnerability blink, knockback, pit respawn at the last checkpoint.
- HUD (hearts, coins, timer, level title), pause menu, win and lose screens, best result
  saved.
- 4-layer parallax background, chiptune SFX synthesised at runtime.

## Architecture

```
Assets/_Ironoak/
  Scripts/
    Core/       IronoakGame (composition root + state machine), PixelDisplay (480x270 RT),
                CameraRig, ParallaxBackground, GameEvents, SaveData, DevAutoCapture
    Gameplay/   LevelBuilder (text map -> Tilemaps), PlayerController/Health/Visual,
                RomanSoldier, Coin, Spikes, Checkpoint, Goal, SolidGround
    Screens/    TitleScreen, Hud, PauseMenu, ResultScreen, MenuList, PixelText, UIKit
    Art/        SpriteLibrary (Resources override -> procedural fallback), ProceduralArt,
                PixelFont (5x7 bitmap font), PixelArt
    Controls/   GameInput (Input System or legacy, chosen at compile time)
    Sound/      Sfx (procedural chiptune)
  Editor/       ProjectSetup (scene/build settings), ArtImportPostprocessor, BuildTools
  Resources/
    Levels/Level01.txt   <- edit the level here (legend inside the file)
    Art/                 <- drop final PNGs here (see docs/ART_SPEC.md)
```

- **The scene holds one GameObject.** `IronoakGame` builds everything else at runtime, so the
  scene never merge-conflicts and the whole game is easy to diff.
- **Levels are data.** Each level is a plain-text map, one character per tile. You can add
  levels without writing code.
- **Art is hot-swappable.** `SpriteLibrary.Get("player_idle")` loads `Resources/Art/player_idle.png`
  if it exists and falls back to the procedural placeholder otherwise.
- **Pixel-perfect.** The world and UI render into a 480×270 render texture, which is scaled
  to the screen by a whole-number factor.

## Menu tools (Ironoak menu)

- **Open Main Scene / Rebuild Main Scene**
- **Apply Project Settings**: product name, 1920×1080, 2D mode.
- **Build Windows (Dev)**: writes `Builds/Windows/Ironoak.exe`.
- **Reset Best Score**

Automated visual smoke test of a build (saves 6 screenshots, then quits):

```
Builds\Windows\Ironoak.exe -screen-fullscreen 0 -ironoak-capture C:\temp\shots
```

## Roadmap after the POC

1. Final art from Codex (see `docs/ART_SPEC.md`), then Animator-driven animation.
2. Music, and replacing the procedural SFX with authored clips.
3. Brenna's sling attack, a mead power-up (temporary shield), a second enemy type
   (archer, shield-bearer).
4. A world map with 3 to 4 levels (forest, river, Roman camp, fort siege) plus a boss.
5. Save slots, settings menu (volume, rebinding), and a Steam build pipeline.

## Legal

All characters, names and art in Ironoak are original. The historical setting (Gaul,
Celtic tribes, the Roman Republic) is public domain. Keep new art and prompts free of any
existing franchise's characters, names or signature looks.
