# Ironoak — 2D Puzzle Platformer (POC)

An original puzzle platformer set in Gaul, around 50 BC. Brenna, a young Celtic slinger,
follows the legion that took her grandfather. She uses her sling and the Heartwood Torc,
which raises roots from the earth.

Unity **6000.3 LTS** (6000.3.26f1), **URP 2D Renderer**, hand-painted art at native resolution
(128 px per tile).

**Status: M1 complete; art Phase B (tech spike) complete.** Level 1 is fully playable with
painted terrain, parallax, 2D lighting and Brenna's cut-out puppet. The automated playtest
solves every puzzle (33/33 checks) at a locked 60 fps.

## Run it

1. Unity Hub → **Add project from disk** → select `C:\Repos\Asterix`.
2. On first open the project sets itself up (art, prefabs, Level 1 scene, Main scene).
3. Open **Ironoak → Open Main Scene**, set the Game view to **16:9** and press **Play**.

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Move | Arrows / A D | Left stick / D-pad |
| Jump (hold for higher) | Space / W / Z | A |
| Sling: tap to throw, hold to aim | X / J / left mouse | X / right trigger |
| Aim while holding | Mouse, or Up/Down | Right stick |
| Rootgrow (at a glowing knot) | E / C / K / right mouse (or point at a knot) | Y / left trigger |
| Pause | Esc / P | Start |

## Level 1 — The Oak Road (greybox)

The beat chart is in [docs/design/LEVELS.md](docs/design/LEVELS.md); the puzzle IDs are in
[docs/design/PUZZLES.md](docs/design/PUZZLES.md).

| Beat | What happens |
|---|---|
| P1-01 | Sling the bell to open the gate |
| P1-02 | Cut the guy-rope so the bridge falls across the stream |
| Secret 1A | Break the cracked cliff wall → Ogham stone |
| Torc | Tavo's broken cart, where the Heartwood Torc unlocks Rootgrow |
| P1-03 | Raise a root to reach the ledge |
| Secret 1B | A tall root climbs into the hidden canopy nest → Golden torc |
| P1-04 | Push the crate onto a knot; the root and crate together reach the wall top |
| P1-05 | Dam the brook with a root, drain the pool, walk the tunnel. Secret 1C is under the dam |
| P1-06 | Root lifts the crate, the sling knocks it onto the plate, the gate opens |
| P1-T | Druid's Trial: two knots, one crate, two plates, plus a reset bell |
| P1-07 | Boar stampede: climb three walls with roots before the herd catches you |

## Architecture

```
Assets/_Ironoak/
  Scripts/
    Core/       IronoakGame (flow + additive level loading), PixelDisplay, CameraRig (zones, zoom, shake),
                ParallaxBackground + ParallaxSet (data-driven layers), SceneLighting (URP 2D lights,
                post-processing, LevelLook), GameEvents, SaveData, Fx, DevPlaytest
    Gameplay/   PlayerController/Health/Visual, RomanSoldier, LevelRoot, SecretArea, Collectible,
                PowerPickup, CameraZone, StampedeChase, Coin, Spikes, Checkpoint, Goal
    Puzzles/    Signals (SignalSender/Receiver, IHittable), SapKnot, Bell, Rope, Gate, Drawbridge,
                PressurePlate, Crate, CrackedWall, WaterPool
    Powers/     PlayerPowers, SlingAbility, SlingStone, RootgrowAbility
    Screens/    TitleScreen, Hud (toasts/banners), PauseMenu, ResultScreen, FadeOverlay, PixelText
    Art/        SpriteLibrary (painted overrides), PaintedTerrain (paints the collision tilemap),
                ProceduralArt (+ .Puzzles, .Painted), PixelFont, PixelArt
    Characters/ PuppetDefinition, PuppetRig, PuppetAnimator (cut-out Brenna)
    Controls/   GameInput (Input System or legacy)
    Sound/      Sfx (procedural chiptune)
  Editor/       ProjectSetup, RenderSetup (URP), ArtBaker, PrefabFactory, GreyboxBuilder,
                Level1Blueprint, CharacterTools, ArtImportPostprocessor, BuildTools
  Prefabs/      generated level-building blocks (drag into level scenes)
  Scenes/       Main.unity (one GameObject) + Levels/L1_OakRoad.unity
  Resources/Art Codex pixel art + baked placeholders; Art/Painted/ holds the hand-painted art
                (same names → drop-in replacement)
  Settings/     URP pipeline asset and 2D renderer
```

- **Levels are scenes.** Each level scene has a `LevelRoot` (name, bounds, spawn, starting
  powers). The Main scene loads it additively behind a fade.
- **Puzzles are wired, not coded.** Any sender (bell, rope, plate, knot) drives any receiver
  (gate, bridge, pool, crate) through its `targets` list in the Inspector. Receivers support
  "needs N inputs" and "latch".
- **Art sits on top of the greybox.** The tile collision layer is painted at load time by
  `PaintedTerrain`, so editing tiles changes both the physics and the look. Each level's light,
  colour grading and parallax set live on its `LevelRoot` (`look`).
- **The greybox is generated.** `Level1Blueprint` lays out Level 1 in code through
  `GreyboxBuilder` and saves a normal scene. Once designers hand-edit that scene, stop
  regenerating it.

## Menu tools (Ironoak menu)

- **Open Main Scene / Open Level 1 Scene**
- **Pipeline → 1. Bake Missing Placeholder Art / 2. Generate Prefabs / 3. Build Level 1 Greybox**
- **Apply Project Settings**, **Apply Render Pipeline (URP 2D)**, **Build Windows (Dev)** (writes `Builds/Windows/Ironoak.exe`), **Reset Best Score**

## Automated playtest

Builds the game, then plays Level 1 with scripted input and real physics. It logs `[PLAYTEST]
PASS/FAIL` per puzzle and a `PERF` line (average, p95 and p99 frame time), and writes
`playtest.txt`, 14 screenshots and 7 puppet close-ups:

```
Builds\Windows\Ironoak.exe -screen-fullscreen 0 -ironoak-playtest C:\temp\playtest
```

## Design docs

- [docs/design/LEVELS.md](docs/design/LEVELS.md): identity card, beat chart and secrets for all 6 levels
- [docs/design/PUZZLES.md](docs/design/PUZZLES.md): puzzle registry and uniqueness rules
- [docs/design/ART_DIRECTION.md](docs/design/ART_DIRECTION.md): the switch to hand-painted 2D (style, tech, pipeline, phases)
- [docs/design/CHARACTER_PIPELINE.md](docs/design/CHARACTER_PIPELINE.md): cut-out puppet characters: Codex prompts, cleanup checklist, rig tuning
- [docs/art-requests/POC_ASSET_REQUESTS.md](docs/art-requests/POC_ASSET_REQUESTS.md): every painted asset the POC needs from Codex, in 5 batches
- [docs/ART_SPEC.md](docs/ART_SPEC.md): current pixel sprite list and Codex prompts (greybox/placeholder art)

## Milestones

1. **M1, foundation and greybox:** done.
2. **M2, puzzle and secret pass:** playtest-driven tuning, more guidance cues, timing on P1-02.
3. **M3, art and animation:** hand-painted art direction, cut-out animation, 2D lighting
   (Phases A–E in [ART_DIRECTION.md](docs/design/ART_DIRECTION.md)). Phases A and B are done.
4. **M4, audio and finish:** music, SFX, particles, story intro, settings, save/continue.
5. **M5, chase set piece and QA:** stampede finish, performance, controller pass, release build.

## Legal

All characters, names and art in Ironoak are original. The historical setting (Gaul,
Celtic tribes, the Roman Republic) is public domain. Keep new art and prompts free of any
existing franchise's characters, names or signature looks.
