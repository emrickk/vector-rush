"""Editable architectural surface studies. One tile is 12m wide and 96m high.

Floor occupancy is grouped into suites and horizontal bands; glass, opaque
spandrels and framing have separate roughness. No reference pixels are copied.
"""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import random

root = Path(__file__).resolve().parents[2] / "UnityProject/Assets/Art/Stage1/Textures"
root.mkdir(parents=True, exist_ok=True)
for style in range(4):
    rng = random.Random(971 + style)
    base = Image.new("RGB", (768, 6144), (44, 57, 64))
    emission = Image.new("RGB", base.size)
    surface = Image.new("RGBA", base.size, (65, 65, 65, 100))
    normal = Image.new("RGB", base.size, (128, 128, 255))
    b, e, s, n = map(ImageDraw.Draw, (base, emission, surface, normal))
    # Twenty-four floors retain broad glazing while breaking short repeated occupancy bands.
    for floor in range(24):
        top = floor * 256
        occupied = rng.random() < (0.72 if style == 1 else 0.62)
        tint = rng.choice([(175, 194, 201), (205, 174, 124), (120, 163, 184), (205, 211, 205)])
        if style == 0:  # Tall vertical frame / broad office suites.
            bays, gap, sill = 3, 13, 58
        elif style == 1:  # Horizontal ribbon glass and opaque floor slabs.
            bays, gap, sill = 2, 5, 78
        elif style == 2:  # Hotel: grouped bays with generous blank wall zones.
            bays, gap, sill = 3, 25, 64
        else:  # Service floors: mostly dark, occasional full-width occupancy.
            bays, gap, sill = 2, 10, 102
        for bay in range(bays):
            left = bay * 768 // bays + gap
            right = (bay + 1) * 768 // bays - gap
            y0, y1 = top + 18, top + 256 - sill
            glass = (30 + rng.randrange(8), 49 + rng.randrange(8), 59 + rng.randrange(10))
            b.rectangle((left, y0, right, y1), fill=glass)
            s.rectangle((left, y0, right, y1), fill=(145, 145, 145, 218))
            lit = occupied and rng.random() < .68
            if lit:
                exposure = rng.choice([.32, .48, .65, .85, 1.0])
                tint = tuple(int(v * exposure) for v in tint)
                e.rectangle((left + 3, y0 + 5, right - 3, y1 - 3), fill=tint)
                # Interior ceiling and furniture create horizontal, varied light.
                e.rectangle((left + 3, y0 + 5, right - 3, y0 + 12),
                            fill=tuple(min(255, int(v * 1.2)) for v in tint))
                for k in range(3):
                    xx = rng.randint(left + 8, right - 30)
                    e.rectangle((xx, y1 - rng.randint(18, 43), xx + 22, y1),
                                fill=tuple(int(v * .14) for v in tint))
                if rng.random() < .65:
                    e.rectangle((left + 4, y0 + 16, (left + right)//2, y1 - 4),
                                fill=tuple(int(v * .4) for v in tint))
            for xx in range(left + 64, right, 64):
                b.rectangle((xx, y0, xx + 2, y1), fill=(18, 28, 33))
                e.rectangle((xx, y0, xx + 2, y1), fill=(0, 0, 0))
            # Tangent-space bevel normal at the glazing inset.
            n.line((left, y0, right, y0), fill=(128, 190, 234), width=4)
            n.line((left, y1, right, y1), fill=(128, 65, 234), width=4)
            n.line((left, y0, left, y1), fill=(65, 128, 234), width=4)
            n.line((right, y0, right, y1), fill=(190, 128, 234), width=4)
        b.rectangle((0, top + 246, 767, top + 255), fill=(21, 32, 38))
        b.line((0, top + 244, 767, top + 244), fill=(67, 78, 82), width=2)
        if style == 3 and floor % 3 == 0:
            e.rectangle((0, top, 767, top + 255), fill=(0, 0, 0))
            for y in range(top + 28, top + 220, 12):
                b.rectangle((10, y, 758, y + 4), fill=(80, 87, 89))
                n.line((10, y, 758, y), fill=(128, 195, 230), width=3)
    base.save(root / f"Architecture{style}_Base.png")
    emission.save(root / f"Architecture{style}_Emission.png")
    surface.save(root / f"Architecture{style}_Surface.png")
    normal.save(root / f"Architecture{style}_Normal.png")
print("Architectural base, emission, surface and normal maps written.")

# Repaint the four existing advertisement surfaces with broad legible fields.
# Names and source slots remain unchanged; no new signs or services are introduced.
font = "/System/Library/Fonts/Supplemental/Arial Bold.ttf"
for i, (name, color) in enumerate([("NOCTURNE", (17, 126, 151)), ("ION", (190, 23, 79)), ("VESPER", (31, 89, 164)), ("NOVA 07", (184, 108, 39))]):
    im = Image.new("RGB", (512, 1024), color)
    d = ImageDraw.Draw(im)
    dark = (10, 20, 29)
    pale = (195, 215, 216)
    d.rectangle((0, 0, 511, 210), fill=dark)
    size = 66 if len(name) < 8 else 53
    f = ImageFont.truetype(font, size)
    d.text((30, 75), name, font=f, fill=pale)
    # Oversized graphic reads as a designed commercial surface from chase camera.
    d.ellipse((-90, 260, 530, 880), fill=pale)
    d.ellipse((35, 365, 407, 737), fill=color)
    d.polygon([(180, 742), (337, 396), (390, 423), (232, 770)], fill=dark)
    d.rectangle((0, 880, 511, 1023), fill=dark)
    d.text((31, 908), ["AFTER DARK", "PURE ELECTRIC", "BEYOND", "NEXT GENERATION"][i], font=ImageFont.truetype(font, 34), fill=pale)
    d.text((32, 964), "SECTOR 01 / NIGHT SERIES", font=ImageFont.truetype(font, 22), fill=color)
    im.save(root / f"Advert{i}.png")
