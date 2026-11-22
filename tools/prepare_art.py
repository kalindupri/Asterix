"""Prepare generated PNGs at the logical sizes in docs/ART_SPEC.md.

Usage: python tools/prepare_art.py docs/art-generation.json
Requires Pillow. Source images are preserved; only nearest-neighbour sizing is used.
"""
import json
import sys
from pathlib import Path

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
DEST = ROOT / "Assets/_Ironoak/Resources/Art"


def prepare(manifest):
    DEST.mkdir(parents=True, exist_ok=True)
    previews = []
    for asset in manifest["assets"]:
        name = asset["name"]
        size = tuple(asset["size"])
        source = Image.open(ROOT / asset["source"]).convert("RGBA")
        if name in ("bg_mountains", "bg_hills", "bg_forest"):
            # The handoff requires scenery to meet the bottom edge, without padding.
            bounds = source.getbbox()
            source = source.crop((0, bounds[1], source.width, bounds[3]))
            image = source.resize(size, Image.Resampling.NEAREST)
            if name == "bg_forest":
                ImageDraw.Draw(image).rectangle((0, size[1] - 28, size[0] - 1, size[1] - 1), fill="#27493a")
        elif name in ("tile_grass", "tile_dirt", "tile_stone"):
            # Ignore faint export fringe when finding the actual tile boundary.
            bounds = source.getchannel("A").point(lambda a: 255 if a >= 128 else 0).getbbox()
            source = source.crop(bounds)
            image = source.resize(size, Image.Resampling.NEAREST)
        elif name == "tile_plank":
            bounds = source.getchannel("A").point(lambda a: 255 if a >= 128 else 0).getbbox()
            plank = source.crop(bounds).resize((16, 6), Image.Resampling.NEAREST)
            image = Image.new("RGBA", size)
            image.paste(plank, (0, 0))
        elif name.startswith("bg_"):
            image = source.resize(size, Image.Resampling.NEAREST)
        else:
            bounds = source.getbbox()
            if bounds is None:
                raise ValueError(f"Empty source: {name}")
            sprite = source.crop(bounds)
            # Keep the silhouette's proportions and align feet with the pivot.
            sprite.thumbnail(size, Image.Resampling.NEAREST)
            image = Image.new("RGBA", size)
            x = (size[0] - sprite.width) // 2
            y = (size[1] - sprite.height) // 2 if name.startswith(("coin_", "heart_")) else size[1] - sprite.height
            image.paste(sprite, (x, y))
        # Pixel sprites use hard coverage rather than partially transparent fringes.
        image.putalpha(image.getchannel("A").point(lambda a: 255 if a >= 128 else 0))
        image.save(DEST / f"{name}.png")
        assert Image.open(DEST / f"{name}.png").size == size
        if name != "bg_sky" and name not in ("tile_grass", "tile_dirt", "tile_stone"):
            assert image.getchannel("A").getextrema()[0] == 0, f"Missing transparency: {name}"
        elif name == "bg_sky":
            assert image.getchannel("A").getextrema() == (255, 255)
        preview = Image.new("RGBA", (520, 320), "#392f43")
        zoom = min(4, 480 // size[0], 280 // size[1])
        enlarged = image.resize((size[0] * zoom, size[1] * zoom), Image.Resampling.NEAREST)
        preview.alpha_composite(enlarged, ((520 - enlarged.width) // 2, 28))
        ImageDraw.Draw(preview).text((12, 8), f"{name}  {size[0]} x {size[1]}", fill="white")
        previews.append(preview)
    sheet = Image.new("RGB", (520 * 4, 320 * ((len(previews) + 3) // 4)), "#392f43")
    for i, preview in enumerate(previews):
        sheet.paste(preview.convert("RGB"), ((i % 4) * 520, (i // 4) * 320))
    sheet.save(ROOT / "docs/art-contact-sheet.png")
    print(f"Prepared and checked {len(previews)} PNGs in {DEST}")


if __name__ == "__main__":
    prepare(json.loads(Path(sys.argv[1]).read_text(encoding="utf-8")))
