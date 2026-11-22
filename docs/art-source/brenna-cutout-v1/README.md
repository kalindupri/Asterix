# Brenna painted cut-outs — first integrated pass

Created 22 November 2026 with the built-in image_gen tool from the approved Phase A Brenna
sheet and Claude's eight joint templates. Full prompts and source paths are recorded in
[generation.json](generation.json) and `docs/art-generation.json` under `brennaCutout`.

Eight transparent PNGs replace the code-painted placeholders in
`Assets/_Ironoak/Resources/Art/Painted/Brenna/`. The source images here retain their native
resolution. The first torso draft is preserved; `brenna_torso-v2-source.png` is the selected
version with the belt at the waist and the pouch raised above it.

`tools/prepare_brenna_parts.ps1` trims the transparent export fringe and resamples each source
into its template canvas and occupied bounds. `exportBounds` in the manifest records the
placement. The torso extends to the bottom of its canvas to cover the configured waist
joint. No checkerboard or magenta guide marks are included in the exported art.

All eight outputs were checked for canvas size, RGBA transparency, and opaque coverage at
their configured joint. The head attachment in `Brenna.asset` was lowered from 0.36 to 0.29
and its height increased from 0.52 to 0.58 after reviewing assembled poses, bringing the
neck into the collar and the face closer to the approved proportions. The other rig values
and animation code are unchanged.

The final Windows build succeeded and both integrated Level 1 playtests passed (33/33).
All seven final-fit pose captures and the final report are saved under
`docs/screenshots-brenna-painted-final/`.
These are the first painted parts integrated with the existing puppet, not a full painted
environment or UI pass. The seven pose screenshots expose overlap and silhouette details
for any later polish.
