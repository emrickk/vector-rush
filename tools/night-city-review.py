#!/usr/bin/env python3
"""Package timestamped native Stage 7 footage and matched P4 driving views."""
import argparse
import html
import json
import os
from pathlib import Path

from PIL import Image, ImageDraw, ImageOps


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--candidate", type=Path, required=True)
    parser.add_argument("--baseline", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    arguments = parser.parse_args()
    candidate = arguments.candidate.resolve()
    baseline = arguments.baseline.resolve()
    output = arguments.output.resolve()
    output.mkdir(parents=True, exist_ok=True)
    recordings = [json.loads((folder / "preview.json").read_text()) for folder in (baseline, candidate)]
    if not all(recording["complete"] for recording in recordings):
        raise ValueError("Both native recordings must complete a lap")
    if not (candidate / "full-lap.mp4").is_file():
        raise FileNotFoundError("Encode the completed candidate recording first")
    targets = (.025, .065, .145, .225, .27, .82)
    sheet = Image.new("RGB", (1600, len(targets) * 480), "#10151d")
    drawing = ImageDraw.Draw(sheet)
    comparisons = []
    rows = []
    for row, progress in enumerate(targets):
        panels = []
        for column, (folder, recording, label) in enumerate(zip(
            (baseline, candidate), recordings, ("P4 baseline", "Stage 7 city integration")
        )):
            frame = min(recording["frames"], key=lambda sample: abs(sample["progress"] - progress))
            filename = f"view-{row:02d}-{column}.jpg"
            with Image.open(folder / frame["file"]) as source:
                native_size = source.size
                source.convert("RGB").save(output / filename, quality=92)
                thumbnail = ImageOps.pad(source.convert("RGB"), (800, 450), Image.Resampling.LANCZOS, color="black")
                sheet.paste(thumbnail, (column * 800, row * 480))
            caption = f"{label} | course {frame['progress']:.4f} | {frame['seconds']:.2f}s | {native_size[0]} x {native_size[1]}"
            drawing.text((column * 800 + 12, row * 480 + 460), caption, fill="white")
            panels.append(f'<figure><a href="{filename}"><img src="{filename}" alt="{html.escape(caption)}"></a><figcaption>{html.escape(caption)}</figcaption></figure>')
            comparisons.append({"targetProgress": progress, "label": label, "buildGuid": recording["buildGuid"], "frame": frame, "image": filename, "nativeSize": native_size})
        rows.append('<section class="pair">' + "".join(panels) + "</section>")
    sheet.save(output / "comparison.jpg", quality=91)
    (output / "comparison.json").write_text(json.dumps(comparisons, indent=2) + "\n")
    video = html.escape(os.path.relpath(candidate / "full-lap.mp4", output), quote=True)
    build = html.escape(recordings[1]["buildGuid"])
    (output / "index.html").write_text(f'''<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Vector Rush: Stage 7 native review</title>
<style>body{{margin:0;background:#0b1018;color:#e5f0fc;font:16px system-ui}}main{{max-width:1600px;margin:auto;padding:32px}}h1{{font-size:32px}}p{{line-height:1.6;max-width:1000px}}video{{width:100%;background:#000}}.pair{{display:grid;grid-template-columns:1fr 1fr;gap:16px;margin:24px 0}}figure{{margin:0}}img{{width:100%;display:block}}figcaption{{padding:10px;color:#a9c0d5}}code{{overflow-wrap:anywhere}}a{{color:#65def1}}@media(max-width:800px){{.pair{{grid-template-columns:1fr}}main{{padding:16px}}}}</style></head>
<body><main><h1>Stage 7 · native city integration</h1>
<p>Actual standalone Unity gameplay with automated steering through ordinary physics. The full lap uses captured game audio and recorded frame timestamps. Capture can affect frame delivery; this is not a performance benchmark or human/controller validation. Artistic acceptance remains with the owner.</p>
<p>Build <code>{build}</code>. Five facade-connected service terraces supplement P4's animated displays. Road, handling, ship, HUD and weather remain the P4 baseline. No speed-blur integration.</p>
<video controls preload="metadata" src="{video}" aria-label="Stage 7 full native driving lap"></video>
<h2>Matched driving-camera comparisons</h2><p>Left: preserved P4. Right: Stage 7. Samples match nearest course progress, not exact simulation or animation time. Select a frame for its full-size native image.</p>
{''.join(rows)}</main></body></html>''')
    print(output / "index.html")


if __name__ == "__main__":
    main()
