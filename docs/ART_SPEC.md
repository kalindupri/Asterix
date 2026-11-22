# Ironoak — Art Spec for Codex

Every sprite in the game is currently a **code-generated placeholder**. Drop a PNG with the
exact file name below into `Assets/_Ironoak/Resources/Art/` and it replaces the placeholder
automatically. No code or Inspector changes are needed: the import settings (16 PPU, point
filter, no compression, pivot) are applied for you by `ArtImportPostprocessor`.

## Global rules (put these in every Codex prompt)

- **Pixel art**, hard edges, no anti-aliasing, no blur, no gradients except dithering.
- **Transparent background** PNG (except `bg_sky`).
- **Exact pixel size** as listed. If Codex outputs a larger image, downscale with
  *nearest-neighbour* only, to the exact size.
- Side view, characters **facing right**. The game flips them for left.
- 1 px dark outline `#2b1d2e` around characters and props.
- Warm, saturated palette, mid-day light from the top-left.
- **Original designs only.** Do not reference or imitate any existing comic, film or game
  characters. Never put franchise names, "in the style of <artist>", winged helmets, or a
  big-sidekick-carrying-a-standing-stone combo in a prompt.

## Setting and characters

Gaul, around 50 BC. A Celtic forest village holds out against a Roman legion.

- **Brenna**, the player. A young Celtic slinger: long red hair in a single braid, green
  wool tunic, leather belt with a gold buckle, brown checked trousers, leather boots.
  Agile and determined.
- **Roman legionary**, the enemy. A generic soldier: steel helmet with a red crest, segmented
  steel armour, red tunic, leather belt, sandals. Comedic, slightly clumsy.

## Sprite list

Pivot: **BC** = bottom-centre (the sprite stands on this point), **C** = centre,
**BL** = bottom-left.

| File name | Size (px) | Pivot | Notes |
|---|---|---|---|
| `player_idle.png` | 16×16 (up to 24×24) | BC | Brenna standing |
| `player_run1.png` | same as idle | BC | Run: legs apart |
| `player_run2.png` | same as idle | BC | Run: passing pose |
| `player_jump.png` | same as idle | BC | Airborne, one knee up |
| `roman_walk1.png` | 16×16 (up to 24×24) | BC | Legionary walk A |
| `roman_walk2.png` | same | BC | Legionary walk B |
| `roman_squash.png` | 16×8 | BC | Flattened after a stomp, dizzy |
| `tile_grass.png` | 16×16 | C | Dirt with a grass top. **Must tile seamlessly left/right** |
| `tile_dirt.png` | 16×16 | C | Plain earth. Seamless on all sides |
| `tile_stone.png` | 16×16 | C | Grey stone bricks (a Roman fort wall). Seamless |
| `tile_plank.png` | 16×16 | C | Wooden plank in the **top 6 px only**, transparent below |
| `coin_0.png` … `coin_3.png` | 16×16 | C | Celtic gold coin spinning: 0 = full face with a triskele, 1 = 3/4, 2 = edge-on, 3 = 3/4 |
| `spikes.png` | 16×16 | BC | Row of 4 sharpened wooden stakes, about 11 px tall |
| `torch_off.png` | 16×32 | BC | Unlit iron brazier on a wooden pole (checkpoint) |
| `torch_on1.png` / `torch_on2.png` | 16×32 | BC | Same, burning, two flicker frames |
| `goal_banner.png` | 16×48 | BC | Tall pole with a green village banner and a gold oak-leaf emblem |
| `heart_full.png` / `heart_empty.png` | 9×8 | C | HUD hearts |
| `bg_sky.png` | 480×270 | C | Opaque sky gradient, deep blue at top to warm cream at the horizon. Dither allowed |
| `bg_clouds.png` | 480×96 | BL | A few puffy clouds. **Seamless left/right** |
| `bg_mountains.png` | 480×112 | BL | Distant blue-grey mountains with snow caps. Seamless |
| `bg_hills.png` | 480×80 | BL | Soft green rolling hills. Seamless |
| `bg_forest.png` | 480×104 | BL | Pine/oak forest silhouette. **Bottom 28 px solid dark forest floor.** Seamless |

The camera shows 480×270 px (30×17 tiles). One tile is 16×16 px.

## Example Codex prompts

**Player sheet (one frame per call, or a 4-frame strip you slice afterwards):**
> Pixel art sprite, 16x16 pixels, transparent background, side view facing right. An original
> young Celtic woman warrior: long red hair in a braid, green wool tunic, brown leather belt
> with a small gold buckle, brown checked trousers, leather boots. Idle standing pose. 1-pixel
> dark purple-black outline (#2b1d2e), limited palette of about 12 colours, crisp hard pixels,
> no anti-aliasing, retro 16-bit platformer style.

**Enemy:**
> Pixel art sprite, 16x16 pixels, transparent background, side view facing right. A generic
> ancient Roman legionary soldier: steel helmet with a red horsehair crest, segmented steel
> armour, red tunic, leather belt, sandals. Walking pose, comedic and slightly clumsy.
> 1-pixel dark outline (#2b1d2e), crisp hard pixels, no anti-aliasing, retro 16-bit style.

**Ground tile:**
> Seamless pixel art tile, 16x16 pixels. Brown earth with small pebbles, a bright green grass
> layer on the top 4 pixels with uneven blades. Tiles seamlessly left and right. Crisp hard
> pixels, no anti-aliasing, retro 16-bit platformer style.

**Parallax layer:**
> Seamless horizontally tiling pixel art background strip, 480x104 pixels, transparent
> background above the trees. Dark green pine and oak forest silhouette; the bottom 28 pixels
> are solid dark forest floor (#27493a). Limited palette, crisp hard pixels, no anti-aliasing.

## Checking the art

1. Save the PNGs into `Assets/_Ironoak/Resources/Art/`.
2. Press Play. The new art appears right away; anything still missing stays a placeholder.
3. If a sprite floats or sinks, the content isn't touching the bottom edge of the image.
   Trim the empty rows at the bottom.
