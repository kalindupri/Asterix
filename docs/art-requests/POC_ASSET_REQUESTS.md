# Ironoak — POC Asset Requests for Codex (hand-painted)

Everything the Level 1 vertical slice ("The Oak Road") needs in the new painted style, in the
order I'll integrate it. Brenna's 8 body parts are **done**; batch delivery status is below.

**Batch 1 delivered, 23 November 2026:** [23 exported assets, contact sheet, tiling previews
and integration notes](../art-source/poc-batch-1/README.md). **Integrated 23 November 2026**
(Phase B, screenshots in `docs/screenshots-phase-b/`).

**Batch 2 delivered, 23 November 2026:** [37 exported assets, contact sheet, tiling previews
and integration notes](../art-source/poc-batch-2/README.md). **Integrated 23 November 2026**
(screenshots in `docs/screenshots-batch-2/`; playtest 33/33 at 60 fps). Batches 3–5 are still needed.

**Batch 1 feedback (optional v2, low priority):**
- `fg_bushes_bottom.png` covers only about 8% of its canvas, so it barely shows in game. A v2
  with fuller, darker fern and bush clumps every 600–900 px, rising 250–400 px from the
  bottom edge, would add real foreground depth.
- `fg_leaves_top.png` (21% coverage) works; a few longer hanging leaf clusters would help.
- Everything else fits as delivered: terrain walkable line, tiling, prop pivots and the HUD.

- **Look:** locked at Gate A. Follow `docs/art-direction/phase-a/` (Oak Road and Druid's Trial
  style frames, concept sheets, lighting key 01) and the style pillars in `docs/design/ART_DIRECTION.md` §2.
- **Level layout:** `docs/design/LEVELS.md` (Level 1 card) and `docs/screenshots-m1/` (the greybox).
- **Characters:** the cut-out conventions in `docs/design/CHARACTER_PIPELINE.md`.

---

## 0. Rules for every asset

### Originality (non-negotiable)
1. Never name another game, studio or artist in a prompt. Never use another game's images as a reference.
2. Only reference our own files (the Phase A pack, Brenna's parts, earlier batches).
3. Record every prompt, source and export in the batch's `generation.json` (as for `brenna-cutout-v1`).

### Technical
| Rule | Value |
|---|---|
| Scale | **1 tile = 1 world unit = 128 px.** Brenna is about 1.45 units (≈185 px) tall |
| Format | PNG, RGBA, transparent background (except opaque skies) |
| Light | From the top-left; soft painterly shading; thin dark-warm outline on characters and interactables only (no outlines on scenery) |
| Seamless | Anything marked ↔ must tile left/right; ↕ up/down; ✚ both. Check with a 3×3 tiling preview |
| Readability | Interactables are brighter and more saturated than the scenery behind them. Backgrounds stay calmer |
| Delivery | Originals in `docs/art-source/<batch-name>/`; exported game files in `Assets/_Ironoak/Resources/Art/Painted/<Folder>/` with the **exact file names** below; a contact sheet PNG per batch |
| Don't | Bake shadows onto the ground under props, add text or watermarks, or crop parts off at the canvas edge |

**Pivot / anchor:** unless stated otherwise, props stand on their **bottom-centre** (the base
touches the bottom edge of the canvas). Tileable strips have no pivot. The engine reads the
files by name, so sizes may be larger (2×) as long as the **aspect ratio** matches.

---

## Batch 1 — Phase B tech spike (needed first)

Enough to rebuild one section of Level 1 (village road to the first gate) in the new pipeline.
Folder: `Painted/Environment/Forest/` unless noted.

### 1a. Terrain set (Sprite Shape) — forest earth
The ground becomes smooth painted shapes: a **fill** texture inside, **edge** strips along the outline.

| File | Size | Tiling | Contents |
|---|---|---|---|
| `terrain_earth_fill.png` | 512×512 | ✚ | Rich brown earth: small stones, root threads, a few buried spiral pebbles. Low contrast (sits behind everything) |
| `terrain_earth_top.png` | 512×192 | ↔ | Grassy top edge. **The walkable surface line is 64 px from the top**: grass tufts and small flowers rise above it, a soil lip with hanging root hairs sits below |
| `terrain_earth_side.png` | 192×512 | ↕ | Vertical earth/cliff face for walls: layered soil, roots, embedded stones. The outer face is on the **left** edge (we mirror it for right-facing walls) |
| `terrain_earth_bottom.png` | 512×128 | ↔ | Underside of overhangs: dangling roots and dark soil |
| `terrain_earth_corner_tl.png` / `_tr.png` | 192×192 | — | Rounded grassy corners joining the top and side strips |

### 1b. Parallax layers — Oak Road forest (match the Oak Road style frame)
Seamless left/right, transparent above the silhouette except the sky. Width 3840 px (= 30 units, one screen).

| File | Size | Tiling | Depth | Contents |
|---|---|---|---|---|
| `bg_sky.png` | 2560×1440 | — | camera-locked | Soft blue sky to warm haze, a few painterly clouds. Opaque |
| `bg_far_hills.png` | 3840×900 | ↔ | farthest | Hazy blue-green rolling hills; **one small Celtic hillfort with thatched round houses and smoke** (the village goal; appears once, slightly left of centre) |
| `bg_far_forest.png` | 3840×1000 | ↔ | far | Misty blue oak forest band, low contrast |
| `bg_mid_forest.png` | 3840×1300 | ↔ | mid | Greener oaks and trunks, warm light patches; a tree trunk roughly every 1000 px |
| `bg_near_trunks.png` | 3840×1600 | ↔ | near | Two or three big mossy oak trunks with bronze bands and spirals (like the style frame), mostly empty space between |
| `fg_leaves_top.png` | 3840×600 | ↔ | foreground | Dark, slightly blurred oak-leaf canopy hanging down from the top edge (in front of the player) |
| `fg_bushes_bottom.png` | 3840×500 | ↔ | foreground | Dark ferns and bushes rising from the bottom edge; sparse, so they never hide the playfield |
| `fx_light_shafts.png` | 2048×2048 | — | overlay | Soft diagonal god-rays from the top-left on black (used additively) |

### 1c. First interactables
Folder: `Painted/Props/`.

| File | Size | Anchor | Contents |
|---|---|---|---|
| `sap_knot.png` | 192×96 | bottom-centre, sits in the ground | A swirled spiral knot of living wood in the ground. **Drawn in light neutral tones** (the game tints it: grey when dormant, glowing amber when awake) |
| `sap_knot_glow.png` | 256×256 | centre | Soft amber halo with a spiral hint, on transparent (additive glow behind the knot) |
| `root_segment.png` | 128×256 | ↕ | One section of a thick twisting root column: bark, moss tufts, amber sap veins. **Tiles vertically**, width fills the canvas |
| `root_crown.png` | 160×160 | bottom-centre | The top of the root column: knobbly crown with a few leaves and moss. Its base matches the segment width |
| `crate.png` | 128×128 | bottom-centre | Sturdy wooden crate with iron corners and a carved knot mark. Fills the canvas (it's exactly 1 tile) |
| `coin_gold.png` | 96×96 | centre | Celtic gold coin (stater) with a horse or spiral motif. The game spins it by squashing |

### 1d. HUD basics
Folder: `Painted/UI/`.

| File | Size | Contents |
|---|---|---|
| `ui_heart_full.png` / `ui_heart_empty.png` | 96×96 | Painted heart in a small bronze knotwork frame; empty = dark frame with no fill |
| `ui_coin.png` | 96×96 | Small coin icon matching `coin_gold` |

---

## Batch 2 — All Level 1 puzzle pieces
Folder: `Painted/Props/`. Sizes follow the in-game sizes (1 tile = 128 px).

| File | Size | Anchor / tiling | Used by | Contents |
|---|---|---|---|---|
| `bell_post.png` | 160×416 | bottom-centre | P1-01, Trial | Wooden post with a carved arm reaching right; a hook at the arm's tip at (≈70%, ≈90%) |
| `bell.png` | 112×128 | top-centre (hangs from the hook) | P1-01 | Bronze bell with a knotwork band; separate image so it can swing |
| `gate_portcullis.png` | 128×256 | ↕ | P1-01, P1-06, Trial | Heavy wooden portcullis with iron straps and spiked bottom ends; tiles vertically (gates are 4 tiles tall) |
| `gate_portcullis_bottom.png` | 128×96 | bottom-centre | gates | The spiked bottom row (so the tiled gate ends cleanly) |
| `bridge_plank.png` | 256×64 | ↔ | P1-02 | Lashed plank bridge, side view, rope bindings; tiles horizontally (the bridge is 6 tiles long) |
| `rope.png` | 32×128 | ↕ | P1-02 | Twisted hemp rope, vertical; tiles |
| `rope_stake.png` | 96×128 | bottom-centre | P1-02 | Wooden stake driven into the ground with rope knotted round it |
| `plate_up.png` / `plate_down.png` | 128×48 | bottom-centre | P1-06, Trial | Carved round stone pressure plate with a spiral; up = raised, down = sunk with a faint amber glow in the grooves |
| `wall_cracked.png` | 128×128 | ↕ | Secret 1A | Cracked, weakened stones with fissures: clearly breakable compared with normal rock |
| `debris_rock_1..3.png` | 64×64 each | centre | breaking walls | Three rock chunks for the break burst |
| `water_body.png` | 256×256 | ✚ | P1-02 stream, P1-05 pool | Translucent clear stream water (about 70% alpha), soft caustics; tiles both ways |
| `water_surface.png` | 256×64 | ↔ | water top | Bright foam and ripple line along the water surface |
| `spikes_stakes.png` | 128×128 | bottom-centre | hazards | Row of 3–4 sharpened wooden stakes lashed together (about 90 px tall) |
| `torch_post_off.png` | 128×256 | bottom-centre | checkpoints | Iron brazier on a carved wooden pole, unlit, cold embers |
| `torch_post_on.png` | 128×256 | bottom-centre | checkpoints | Same, glowing coals (the flame is a separate FX) |
| `fx_flame_1..4.png` | 96×128 each | bottom-centre | checkpoints | Four looping flame frames that sit in the brazier |
| `goal_banner.png` | 192×416 | bottom-centre | goal | Tall pole with Ironoak's green village banner and a gold oak-leaf emblem, fluttering to the right |
| `stone_block.png` | 128×128 | ✚ | gate walls, Trial | Mossy dressed-stone block masonry (Celtic ruin), for the stone wall areas. Tiles both ways |
| `stone_block_top.png` | 128×64 | ↔ | stone tops | Moss-capped top edge for stone walls |

### Story and pickups
| File | Size | Anchor | Contents |
|---|---|---|---|
| `cart_broken.png` | 384×256 | bottom-centre | Tavo's overturned two-wheeled smith's cart: broken wheel, spilled tongs and hammer, a torn cloth |
| `item_heartwood_torc.png` | 160×160 | centre | The Heartwood Torc: a neck ring of polished oak heartwood with an iron core showing at the ends, faint green-amber glow. Important story item: make it special |
| `item_ogham.png` | 112×144 | centre | Small standing stone with glowing blue-white Ogham notches (lore) |
| `item_golden_torc.png` | 128×128 | centre | Gold twisted-wire torc with knob terminals |
| `item_shard.png` | 96×128 | centre | Pale blue crystal shrine shard with inner light |
| `item_druid_mark.png` | 144×144 | centre | Purple glowing disc carved with a three-armed spiral (the Druid's Trial reward) |
| `fx_pickup_glow.png` | 256×256 | centre | Soft warm sparkle halo for collectibles (additive) |

### Secrets and set pieces
| File | Size | Anchor / tiling | Contents |
|---|---|---|---|
| `secret_cover_rock.png` | 256×256 | ✚ | Dark rock face that hides a secret chamber until found (fades away) |
| `secret_cover_canopy.png` | 512×256 | ↔ | Dense leaf canopy hiding the treetop nest (Secret 1B) |
| `nest_platform.png` | 768×128 | bottom-centre | Woven branch platform in the treetops (Secret 1B floor) |
| `fallen_oak.png` | 384×320 | bottom-centre | Huge fallen oak trunk with roots: the boar herd crashes into it at the end of the stampede |
| `dam_debris.png` | 256×128 | bottom-centre | Reeds and silt piled against a root (shown after the P1-05 dam drains the pool) |

---

## Batch 3 — Enemies (cut-out parts)
Same rules as Brenna's parts (`CHARACTER_PIPELINE.md`): side view facing right, limbs hanging
straight down with the joint at the top, round overlapping joint ends, isolated on transparent.
Reference: `docs/art-direction/phase-a/legionary-concepts-v1.png` and `boar-concepts-v2.png`.

### 3a. Roman legionary — folder `Painted/Legionary/` (≈1.4 units tall, like Brenna)
| File | Canvas | Joint (x, y from bottom-left) | Contents |
|---|---|---|---|
| `legionary_head.png` | 288×352 | 0.45, 0.06 | Head in a steel helmet with cheek guards and a red horsehair crest; slightly comic, worried face; neck stub |
| `legionary_head_dizzy.png` | 288×352 | 0.45, 0.06 | Same head, helmet askew, spiral-eyed and dazed (stun state) |
| `legionary_torso.png` | 240×240 | 0.50, 0.06 | Segmented steel armour over a red tunic, leather belt with a plain rectangular buckle (Roman geometric decoration, not Celtic) |
| `legionary_pelvis.png` | 240×144 | 0.50, 0.88 | Red tunic skirt with hanging leather strips (pteruges) |
| `legionary_arm_upper.png` | 96×152 | 0.50, 0.90 | Red sleeve with a shoulder plate |
| `legionary_arm_lower.png` | 96×160 | 0.50, 0.92 | Forearm with leather bracer, closed fist |
| `legionary_leg_upper.png` | 104×168 | 0.50, 0.92 | Bare knee-length thigh with tunic edge |
| `legionary_leg_lower.png` | 136×200 | 0.40, 0.94 | Shin with greave, strapped sandal, toe pointing right, flat sole |
| `legionary_flat.png` | 320×128 | bottom-centre | Whole legionary squashed flat after a stomp, helmet popped off (single image) |

### 3b. Wild boar — folder `Painted/Boar/` (≈1.1 units tall, 1.6 long)
| File | Canvas | Joint | Contents |
|---|---|---|---|
| `boar_body.png` | 352×224 | 0.50, 0.55 (body centre) | Bristly dark-brown body with a raised mane ridge, side view facing right, no legs, no head |
| `boar_head.png` | 224×192 | 0.15, 0.55 (neck, left side) | Head with small angry eye, snout and curved tusks, facing right |
| `boar_leg_front_upper.png` / `_lower.png` | 80×128 each | 0.50, 0.90 | Front leg in two pieces; the lower piece ends in a cloven hoof |
| `boar_leg_back_upper.png` / `_lower.png` | 96×128 each | 0.50, 0.90 | Back leg (thicker haunch), two pieces |
| `boar_tail.png` | 64×96 | 0.50, 0.92 | Short tufted tail |

### 3c. Brenna extras — folder `Painted/Brenna/`
| File | Canvas | Joint | Contents |
|---|---|---|---|
| `brenna_head_blink.png` | same as head | same as head | Head with eyes closed (blink and Rootgrow cast) |
| `brenna_head_hurt.png` | same as head | same as head | Head with a wince expression (hurt) |
| `brenna_sling.png` | 96×224 | 0.50, 0.95 (in her fist) | Leather sling hanging from her fist: two cords and a pouch holding a stone |

---

## Batch 4 — Druid's Trial ruin and the village finale

### 4a. Druid's Trial interior — folder `Painted/Environment/Ruin/` (match the Druid's Trial style frame)
| File | Size | Tiling | Contents |
|---|---|---|---|
| `ruin_back_wall.png` | 2048×1024 | ↔ | Dim blue-violet back wall of carved stone arches and alcoves (behind the playfield) |
| `ruin_pillar.png` | 192×768 | bottom-centre | Carved standing pillar with spiral reliefs and moss |
| `ruin_arch.png` | 768×640 | bottom-centre | Big carved arch frame for the room entrance |
| `ruin_shrine.png` | 384×448 | bottom-centre | Druid shrine behind the reward gate: stone altar with a purple glow niche |
| `fx_torch_pool.png` | 512×512 | centre | Warm amber light-pool gradient (additive) for wall torches |
| `fx_purple_glow.png` | 512×512 | centre | Purple shrine glow (additive) |
| `wall_torch.png` | 96×192 | bottom-centre | Iron wall sconce torch (with the flame FX frames from Batch 2) |

### 4b. Village finale — folder `Painted/Environment/Village/`
| File | Size | Anchor | Contents |
|---|---|---|---|
| `village_gate.png` | 768×640 | bottom-centre | Timber palisade gate of a friendly Celtic village, carved posts, open doors |
| `village_houses_bg.png` | 3840×1100 | ↔ | Background layer of thatched round houses with smoke, warm light |
| `villager_wave.png` | 192×256 | bottom-centre | Optional: one friendly villager waving (single image, original design) |

### 4c. Set dressing (place freely around Level 1) — folder `Painted/Dressing/`
| File | Size | Contents |
|---|---|---|
| `dress_fern_1..3.png` | 256×192 | Three fern clumps |
| `dress_flowers_1..2.png` | 192×128 | White and yellow wildflower patches |
| `dress_mushrooms.png` | 160×128 | Mushroom cluster |
| `dress_rock_spiral.png` | 256×192 | Mossy boulder with a carved spiral |
| `dress_rock_small_1..2.png` | 160×112 | Small mossy rocks |
| `dress_bush_1..2.png` | 384×256 | Leafy bushes |
| `dress_hanging_moss.png` | 128×256 | Moss strands hanging from overhangs (top-anchored) |
| `dress_signpost.png` | 160×256 | Carved wooden waymarker with Ogham notches (no letters) |
| `dress_log_small.png` | 320×128 | Small mossy log |

All dressing is anchored bottom-centre except the hanging moss (top-centre).

---

## Batch 5 — Effects, UI, title and story

### 5a. Effects — folder `Painted/FX/` (soft, painterly, mostly additive or alpha)
| File | Size | Contents |
|---|---|---|
| `fx_dust_1..3.png` | 128×96 | Dust puffs for landing, running and roots |
| `fx_leaf_1..3.png` | 48×48 | Falling oak leaves (green, gold, brown) |
| `fx_sap_mote.png` | 32×32 | Glowing amber sap particle |
| `fx_sparkle.png` | 64×64 | Four-point sparkle star |
| `fx_splash_1..3.png` | 128×128 | Water splash frames |
| `fx_dirt_chunk_1..2.png` | 48×48 | Dirt clods for when a root bursts up |
| `fx_stone.png` | 32×32 | The sling stone (river pebble) |
| `fx_aim_dot.png` | 24×24 | Soft dot for the aiming arc |
| `fx_stun_stars.png` | 160×64 | Ring of little stars and spirals over a dizzy legionary |
| `fx_impact.png` | 128×128 | Stylised hit burst |
| `fx_soft_glow.png` | 256×256 | Generic soft round glow |

### 5b. UI — folder `Painted/UI/`
| File | Size | Contents |
|---|---|---|
| `ui_panel.png` | 512×512 | Parchment panel with a carved oak and bronze knotwork border, **9-slice friendly** (the border stays at most 96 px thick, the plain middle area stretches) |
| `ui_button.png` / `ui_button_selected.png` | 512×128 | Wooden plaque button, 9-slice friendly; the selected one glows amber |
| `ui_icon_secret.png` | 96×96 | Keyhole-in-a-spiral icon for secrets |
| `ui_icon_torc.png` | 96×96 | Small golden torc icon |
| `ui_icon_shard.png` | 96×96 | Shrine shard icon |
| `ui_icon_trial.png` | 96×96 | Druid's Mark icon |
| `ui_prompt_key.png` | 96×96 | Blank keyboard keycap (letters are added in game) |
| `ui_prompt_button.png` | 96×96 | Blank round gamepad button (generic, no brand styling) |
| `title_keyart.png` | 3840×2160 | Title-screen key art: Brenna on a root above the Oak Road at golden hour, the hillfort in the distance; **leave the top-centre third calm for the logo** |
| `title_logo_emblem.png` | 1024×512 | The logo ornament **without lettering**: an oak branch with a torc and knotwork, framing an empty band where the game adds "IRONOAK" in the font |
| `results_banner.png` | 1024×256 | Ribbon banner for "VILLAGE REACHED!" (blank; text added in game) |

**Font:** Codex can't make a font. Please pick one **OFL-licensed** display font with a Celtic or
storybook feel (for example from Google Fonts, licence file included) for titles, plus one
readable OFL body font. Put the `.ttf` files and their `OFL.txt` in `Assets/_Ironoak/Fonts/`.
Never use fonts from games or with unclear licences.

### 5c. Story intro cards — folder `Painted/Story/` (shown before Level 1)
| File | Size | Contents |
|---|---|---|
| `story_01_village.png` | 1920×1080 | Ironoak at dusk: forges glowing beneath the giant oak |
| `story_02_raid.png` | 1920×1080 | Night: Roman legionaries leading chained smiths away by torchlight (no violence shown) |
| `story_03_alone.png` | 1920×1080 | Morning: Brenna returns from hunting to the empty forge and finds Tavo's marks on a tree |
| `story_04_road.png` | 1920×1080 | Brenna, sling in hand, sets off down the Oak Road |

Keep the bottom quarter calm (subtitles go there). No text in the images.

---

## Acceptance checklist (per asset)
- [ ] Exact file name and folder; size or aspect ratio as listed
- [ ] Transparent background (no grey fringe or halo), nothing cut off at the edges
- [ ] Seamless where marked (checked in a 3×3 tiling preview)
- [ ] Light from the top-left; matches the Phase A frames' palette and brushwork
- [ ] Interactables read clearly against the matching background layer
- [ ] Characters: joint at the listed position, limbs vertical, round overlapping joint ends
- [ ] Prompt and source recorded in the batch's `generation.json`; contact sheet added

## Integration order (what I'll do with each batch)
1. **Batch 1 (done):** URP 2D renderer and lights, painted terrain over the collision tilemap
   (whole level), the new parallax with foreground layers, and knot, root, crate, coin and
   heart swaps. Playtest 33/33 at 60 fps.
2. ~~**Batch 2:**~~ *(done)* every Level 1 prefab switches to painted art; full-level greybox replaced; playtest stays 33/33.
3. **Batch 3:** legionary and boar rigs plus procedural animation (patrol, stun, squash, gallop, crash).
4. **Batch 4:** Trial room and village finale dressed; set-dressing pass across the level.
5. **Batch 5:** FX, the new UI and title screen with the chosen font, and the story intro sequence.

Not covered here: **music and sound effects** (image tools can't make audio). Those are handled
separately in M4.
