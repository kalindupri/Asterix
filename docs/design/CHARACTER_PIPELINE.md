# Ironoak — Cut-out Character Pipeline (no artist needed)

**Decision (22 Nov 2026):** characters are **cut-out puppets**: separate painted body parts,
joined into a skeleton and **animated procedurally in code**. Codex paints the parts, you tidy
them in a free editor, and the game does the rest. No keyframing, no paid tools, no artist.

**Status:** the eight Brenna placeholders have been replaced with painted parts generated from
the approved Phase A concept. The head attachment and size were tuned in `Brenna.asset`.
Sources, prompts and export details are in [the painted-part handoff](../art-source/brenna-cutout-v1/README.md).
Final-fit pose captures are in `docs/screenshots-brenna-painted-final/`; the earlier placeholder
captures remain in `docs/screenshots-rig/`.

---

## How it works

```
Codex (parts, one at a time)  →  You (cleanup in Photopea/Krita)  →  drop PNGs into the project
        ↓                                                              ↓
  Brenna concept sheet as reference                 Unity builds the puppet automatically:
  (docs/art-direction/phase-a/brenna-concepts-v1.png)   joints, draw order, procedural run/jump/
                                                         aim/throw/cast, braid physics, squash
```

- **Folder:** `Assets/_Ironoak/Resources/Art/Painted/Brenna/` (any PNG here replaces the placeholder of the same name)
- **Rig settings:** `Assets/_Ironoak/Resources/Characters/Brenna.asset`. Joint positions,
  sizes and draw order are all editable in the Inspector; press Play to see changes.
- **Templates:** `docs/character-templates/*_template.png`. Each part at 4× size on a
  checkerboard, with the **joint marked by a magenta cross**. Use them as a size and
  orientation guide.
- **Animation:** `PuppetAnimator.cs`. Stride, leg swing, knee bend, arm swing, lean, bob,
  braid stiffness and drag are Inspector values.

## The 8 parts

All parts are **side view, facing right**. Limbs are drawn **hanging straight down** with the
joint at the top. Back limbs reuse the front images (the game darkens them), so there are only
8 images.

| File name | Contains | Template canvas (px) | Joint (x, y from bottom-left) | Notes |
|---|---|---|---|---|
| `brenna_head.png` | Head, face, loose wavy hair, ear, a short neck stub | 288 × 320 | 0.42, 0.06 (bottom of neck) | Neck stub extends down so it tucks into the collar. **No braid** (separate) |
| `brenna_torso.png` | Tunic chest from collar to belt, belt with knot buckle, **shoulder strap and stone pouch** | 224 × 240 | 0.50, 0.06 (waist, bottom-centre) | No arms. Rounded shoulders. Bottom edge goes a little below the belt |
| `brenna_pelvis.png` | Tunic skirt below the belt with knotwork hem | 240 × 144 | 0.50, 0.88 (hip, near top) | Top edge goes a little above the belt line so it tucks under the torso |
| `brenna_arm_upper.png` | Sleeve from shoulder to elbow, with knotwork cuff | 88 × 152 | 0.50, 0.90 (shoulder, top) | Round top end (ball joint) |
| `brenna_arm_lower.png` | Forearm, leather bracer, **fist** | 88 × 160 | 0.50, 0.92 (elbow, top) | Round top end. Hand closed (it holds the sling) |
| `brenna_leg_upper.png` | Plaid trouser, hip to knee | 104 × 176 | 0.50, 0.92 (hip, top) | Round top end. Baggy is fine; keep the outline simple |
| `brenna_leg_lower.png` | Plaid trouser, knee to ankle, plus the wrapped leather **boot, toe pointing right** | 136 × 200 | 0.40, 0.94 (knee, top) | Boot sole flat along the bottom edge |
| `brenna_braid.png` | One section of the plait (repeated 3× by the game) | 80 × 112 | 0.50, 0.92 (top) | Tapers slightly. Include the leather band only if it reads well repeated |

The canvas sizes are guides. Any resolution works as long as the **proportions and joint
position** match, because the game scales each part to its on-screen height. Bigger is
better quality (aim for at least the template size).

## Golden rules (they make or break the result)

1. **One part per image, nothing else.** Transparent background, no ground shadow, no text, no other limbs.
2. **Same pose every time:** side view facing right; limbs perfectly vertical, hanging down.
3. **Overlap at every joint.** The joint end of each part is a smooth round "ball" that extends
   about 10% past the joint, so it hides under the parent part when it rotates. No flat cut ends.
4. **Same light and style as the concept:** light from the top-left, soft painterly shading,
   thin dark-warm outline.
5. **Simplify trim near joints.** Keep knotwork away from the ends that rotate (it shows seams).

## Codex prompts

Attach the concept sheet `docs/art-direction/phase-a/brenna-concepts-v1.png` (our own art) as
the reference image for every prompt. Never use another game's images and never name another
game or artist.

**Shared prefix (put it in front of every part prompt):**
> Production game asset for a 2D cut-out puppet. Character: Brenna, from the attached concept
> sheet (our original character); match her colours, materials and painting style exactly.
> Hand-painted storybook cartoon, soft painterly brush texture, thin dark warm outline, light
> from the top-left. Side view facing right. Draw ONLY the single body part described, isolated,
> centred, on a fully transparent background. No shadow on the ground, no text, no other body
> parts, no frame.

**Per part** (append one):

- **head:** "Brenna's head in side view facing right: face, freckles, determined friendly
  expression, ear, loose wavy red hair; NO braid; a short neck stub at the bottom, rounded so it
  can tuck into a collar. Portrait canvas 288×320."
- **torso:** "Brenna's torso only, side view facing right: green wool tunic chest from collar to
  just below the belt, knotwork collar trim, leather belt with round knot buckle, leather shoulder
  strap and a small stone pouch at the hip. No arms, no head, no legs. Rounded shoulders. Canvas 224×240."
- **pelvis:** "Brenna's tunic skirt only: the green wool tunic below the belt down to the
  knotwork hem, seen from the side facing right, slightly flared. The top edge is a soft rounded
  edge. No legs. Canvas 240×144."
- **arm_upper:** "Brenna's upper arm only: green wool sleeve from shoulder to elbow with
  knotwork cuff, hanging straight down, perfectly vertical, rounded ball-joint top end. Canvas 88×152."
- **arm_lower:** "Brenna's forearm only: bare forearm with a leather bracer and a closed fist,
  hanging straight down, perfectly vertical, rounded ball-joint top end at the elbow. Canvas 88×160."
- **leg_upper:** "Brenna's thigh only: baggy brown plaid trouser from hip to knee, hanging
  straight down, perfectly vertical, rounded ball-joint top end. Canvas 104×176."
- **leg_lower:** "Brenna's lower leg only: brown plaid trouser from knee to ankle and a
  wrapped leather boot with the toe pointing right, standing vertical, flat sole along the
  bottom, rounded ball-joint top end at the knee. Canvas 136×200."
- **braid:** "One section of Brenna's thick red plaited braid, hanging straight down,
  perfectly vertical, slightly tapering, rounded top end. Canvas 80×112."

**Tips for consistency:** generate each part 3–4 times and pick the best. If colours drift,
add "use exactly these colours: tunic #4E7A3A, hair #C8452C, trousers #7A4E30, boots #6B4226"
(sample them from the concept sheet). Record each prompt in `docs/art-generation.json`.

## Cleanup checklist (Photopea: free, in the browser)

For each part:

1. Open the Codex PNG in [Photopea](https://www.photopea.com) (or Krita).
2. **Remove any background** (Magic Wand on the background → Delete) so it's truly transparent.
3. **Straighten:** Edit → Free Transform → rotate until the limb is perfectly vertical.
4. **Crop** close around the part (Image → Trim → transparent pixels), leaving a few pixels of margin.
5. **Check the joint end:** it must be round and extend a little past the joint. If it's flat
   or cut off, paint it rounder with a soft brush, picking the colour with the eyedropper.
6. **Compare with the template** (`docs/character-templates/`): same orientation, joint in the same place.
7. Export as **PNG** with the exact file name from the table into
   `Assets/_Ironoak/Resources/Art/Painted/Brenna/` (overwrite the placeholder).

## Test in Unity

1. Open the project. New PNGs import automatically (smooth filtering, correct settings).
2. Open the Main scene, press **Play**, run, jump, hold Fire to aim, and use Rootgrow at a knot.
3. If a part sits wrong, select `Resources/Characters/Brenna.asset` and adjust that part:
   - **pivot**: where the joint is inside the image (0..1)
   - **height**: on-screen size
   - **offset**: where it attaches to its parent
   - **order**: draw order

   Changes show the next time you press Play.
4. If the motion looks stiff or floppy, tune `PuppetAnimator` (stride, swing, lean, braid) in the
   Inspector during Play, then copy the values back.

Automated check: `Builds\Windows\Ironoak.exe -ironoak-playtest C:\temp\pt` also writes close-up
shots `puppet_1_idle … puppet_7_throw.png` of the puppet in its main poses.

## After Brenna

The same pipeline (definition asset, parts, procedural animator) covers the **legionary** and the
**boar**. Each gets its own part list, written once Brenna is proven.
