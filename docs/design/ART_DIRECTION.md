# Ironoak — Art Direction Switch: Pixel Art → Hand-Painted 2D

**Decision:** move from 16 px pixel art to a high-resolution, hand-painted cartoon look in the
spirit of modern premium mobile and console platformers: painterly textures, bold silhouettes,
skeletal animation, dynamic light and deep parallax.

**Why now:** M1's gameplay (physics, puzzles, level geometry, automated playtest) doesn't
depend on art. Switching before M3 wastes no art work and keeps every puzzle intact.

---

## 1. Keep it original (legal guardrails)

A visual *style* can't be owned. Another game's characters, assets, compositions and
signature designs can. Rules for the whole team, Codex included:

1. **Never name another game, studio or artist in a prompt** ("in the style of X" is banned).
   Describe the style with the words in §2 instead.
2. **Never feed another game's screenshots or art to Codex** as references or for img2img.
   Only use our own concept art and style frames as references.
3. **Our own designs:** Brenna, the legionaries, the boars, Ironoak village and Celtic motifs
   come from our story and the historical setting, never from another game's characters or levels.
4. Keep the generation manifest (`docs/art-generation.json`) for every asset: the prompt,
   the source file, the date. It's our provenance record.

## 2. Style pillars

| Pillar | Description |
|---|---|
| **Painted, not pixelated** | Soft hand-painted textures with visible brush grain; smooth shapes; no outlines on environments, thin dark-warm outlines on characters |
| **Bold, chunky silhouettes** | Exaggerated proportions: big heads and hands, small feet. Every character reads as a black silhouette at a glance |
| **Storybook warmth** | Warm saturated palette in the foreground, cooling and desaturating with distance (aerial perspective) |
| **Light tells the story** | Rim light on characters, god-rays through the canopy, glowing sap knots, torchlight in the trial and caves |
| **Celtic identity** | Knotwork, spirals and triskeles on props, carved oak, bronze and gold, woven textiles. This is what makes Ironoak ours |
| **Alive** | Everything moves a little: swaying foliage, drifting pollen, a bouncing braid, breathing idles |

**Readability rule:** gameplay surfaces and interactables are always brighter, more saturated
and higher-contrast than the background. Sap knots, bells, ropes, crates, plates and cracked
walls each have a recognisable shape and a subtle highlight, so a player can always tell what
can be used.

## 3. Technical plan

### 3.1 What stays exactly as it is
- **Gameplay units:** 1 world unit = 1 tile. Jump height (3.6), puzzle geometry, colliders.
- **The greybox Tilemap becomes the invisible collision layer.** Designers keep editing it,
  the art layer is painted over it, and the automated playtest keeps working unchanged.
- **The puzzle framework:** signals, powers, prefabs, LevelRoot, game flow, save, events.

### 3.2 What changes

| Area | Pixel (now) | Painted (target) |
|---|---|---|
| Render pipeline | Built-in | **URP 2D Renderer**: 2D lights, normal maps, shadow casters, post-processing (bloom, colour grading, vignette) |
| Display | 480×270 render texture, point-filtered | Native resolution; reference 1920×1080, 4K-ready art |
| Art density | 16 px per unit | **128 px per unit** authored (4K), mip-mapped, bilinear |
| Camera | Fixed 17-unit-tall view | **Closer camera, about 11 units tall**, with zone-driven zoom: puzzle rooms zoom out to frame the whole puzzle, the chase zooms out for look-ahead (Cinemachine or our CameraRig plus zoom) |
| Terrain | Square tiles | **Sprite Shape** organic ground (painted fill plus edge strips: grassy lips, roots, rock) over the invisible collision tilemap, plus hand-placed painted set dressing |
| Characters | 4-frame sprites | **Skeletal animation (Unity 2D Animation)**: layered PSB from the PSD Importer, bones, IK, secondary motion on the braid and cloth |
| Parallax | 4 layers | **6–8 layers** including **foreground** layers in front of the player (leaves, branches, fog) for depth, with blurred far layers |
| UI text | 5×7 pixel font | **TextMeshPro** with an open-licence (OFL) display font; painted UI frames, icons and button prompts |
| VFX | 2 px particles | Particle System with painted sprites: leaves, dust, sap sparkles, water splashes, light shafts |

### 3.3 Character scale
Brenna's visual is about 1.3 units tall (the collider stays at 0.85 units; the art overhangs
it slightly, as is normal). With the 11-unit camera she fills about 12% of the screen
height, close enough for expressive animation while keeping puzzles readable.

### 3.4 Animation list (Brenna; skeletal)
Idle (breathing plus braid sway) · idle fidget · run · turn · jump-up · apex · fall · land
(soft and hard) · push crate · sling aim (upper-body blend driven by aim angle, so it follows
the cursor or stick) · sling throw · Rootgrow cast · hurt · knockback · death · checkpoint
light · power unlock · victory.
Legionary: patrol walk · alert · stunned (dizzy loop) · squashed · charge.
Boar herd: gallop cycle (2 offsets) · tumble/crash.

### 3.5 Performance budget
60 fps on a mid-range PC (GTX 1060-class) at 1080p, 4K optional. Sprite atlases at most
4096², at most 16 dynamic 2D lights on screen, normal maps only on hero layers.

## 4. Art pipeline: who makes what

| Asset type | Source | Notes |
|---|---|---|
| Concept art and style frames | **Codex** | Fast exploration; we pick and lock a direction |
| Backgrounds / parallax layers | **Codex** → cleanup | AI is strong here. Separate layers, seamless horizontally |
| Terrain fill and edge textures | **Codex** → cleanup | Must tile seamlessly; checked in a tiling preview |
| Props, set dressing, interactables | **Codex** → cleanup | Consistent lighting direction (top-left), transparent PNG |
| VFX sprites, UI frames, icons | **Codex** | Simple, quick |
| **Characters (rig-ready parts)** | **Concept by Codex → parts painted or cleaned by a human artist** | AI can't reliably paint consistent separate body parts (head, torso, upper arm, forearm, hand…) across poses. Budget a 2D artist for Brenna, the legionary and the boar, or accept heavy cleanup |
| Rigging and animation | Me (Unity 2D Animation) | From layered PSB files |

**Honest risk:** character art is the bottleneck in this style. Everything else Codex can
carry; the hero characters need a human hand to look production quality.

### Prompt template (no references to other games)
> Hand-painted 2D game art, storybook cartoon style, soft painterly brush textures, bold
> chunky silhouette, exaggerated proportions, warm saturated palette, gentle rim light from the
> top-left, atmospheric blue haze in the distance, Celtic knotwork details. [SUBJECT].
> Side view. Transparent background. No text, no watermark.

## 5. Phased plan

Each phase ends with a **gate**: you approve before the next phase spends effort.

### Phase A — Look development (Codex, parallel with B)

**Review pack:** [Phase A concepts, palettes and lighting keys](../art-direction/phase-a/README.md)
— generated 22 November 2026; **Gate A approval pending**.

1. **Style frame 1:** the Oak Road forest at 1920×1080: foreground, playfield, three
   background layers, Brenna standing on a path, a glowing sap knot.
2. **Style frame 2:** the Druid's Trial ruin (stone, torchlight, purple druid glow).
3. **Brenna concept sheet:** front, side and back turnaround, 3 expressions, sling pose.
4. **Legionary and boar concept sheets.**
5. **Palette and lighting key** per level (taken from the identity cards in LEVELS.md).

**Gate A:** you pick and lock the look. Everything after this follows the locked frames.

### Phase B — Tech spike (me, parallel with A)
1. Add URP with the 2D Renderer; convert materials; 2D global, point and spot lights; post-processing volume.
2. Replace the pixel display with a native-resolution display mode, plus camera zoom zones.
3. Art layer over collision: Sprite Shape ground on one section of Level 1 over the hidden tilemap.
4. Placeholder rigged character: a simple layered PSB with bones and run/jump/aim driven
   by PlayerController, proving the animation hookup.
5. TextMeshPro UI with an OFL font; painted button prompts.
6. Automated playtest still 33/33 and frame time within budget.

**Gate B:** the playable section in the new pipeline (with placeholder art) runs at 60 fps.

### Phase C — Character production
Brenna parts → rig → full animation list. Then the legionary, then the boars.
**Gate C:** Brenna looks and moves at final quality in the spike section.

### Phase D — Level 1 art pass
Paint and dress Level 1 section by section, in beat order: village road → stream and bridge →
cliff and Secret 1A → Tavo's cart → root grove (P1-03/04) → dam and pool → ledge and plate →
Druid's Trial ruin → stampede path → friendly village banner.
Interactables get final art (knots, bells, ropes, crates, plates, gates, cracked walls, water).
**Gate D:** full Level 1 playthrough at final art; the playtest still passes.

### Phase E — Light, FX and UI finish
God-rays, sap glow, torch flicker, water shader, foliage sway, particle pass; title screen,
HUD, pause and results reskin; screen transitions.
**Gate E = the POC slice is done (merges with M4/M5).**

## 6. Code changes (for reference)

| Component | Change |
|---|---|
| `PixelDisplay` | Becomes `GameDisplay` with a `Painted` mode (direct-to-screen URP camera) |
| `CameraRig` | Adds zoom: zones define position bounds plus orthographic size |
| `ParallaxBackground` | Data-driven layer list (any count, foreground layers, per-layer blur and tint) |
| `PlayerVisual` | Drives an Animator (speed, grounded, vertical speed, aim angle, events) instead of swapping sprites |
| `PixelText` / `PixelFont` | Replaced by TextMeshPro in the UI factory |
| `ArtImportPostprocessor` | New rules for `Art/Painted/`: 128 PPU, bilinear, mipmaps, compression, PSB import |
| Greybox `Ground` tilemap | Renderer hidden at runtime (kept in the editor as a toggle) |
| `ProceduralArt` placeholders | Kept only as the greybox fallback |

## 7. Decisions needed from you

1. ~~**Character artist**~~ **Decided (22 Nov 2026): no artist.** Characters are cut-out
   puppets: Codex paints separate parts, you clean them up, and the game rigs and animates them
   procedurally in code. Rig system built; workflow in [CHARACTER_PIPELINE.md](CHARACTER_PIPELINE.md).
   Fallback if Codex parts don't match well enough: lean into a deliberate paper-cut-out look
   (visible joint pins), keeping painted environments.
2. **Target platforms:** PC only for the POC, or mobile as well? Mobile changes the
   performance budget, texture sizes and controls.
3. **Pixel art assets:** archive the current pixel set (kept in git history and
   `docs/art-source/`) once the painted pipeline replaces it.
