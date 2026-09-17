#!/usr/bin/env python3
"""Serial native Stage 2 capture. Run through opening-city-run.py's heavy-process lease."""
import argparse
import json
from pathlib import Path
import plistlib
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("app", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--mode", choices=("screens", "loop", "propulsion", "performance"), default="screens")
    parser.add_argument("--matrix", action="store_true", help="720p, 900p, 1080p and 2560x1080")
    parser.add_argument("--size", default="1600x900")
    parser.add_argument("--video", action="store_true")
    parser.add_argument("--hide-hud", action="store_true")
    parser.add_argument("--comparison", action="store_true", help="Same-binary legacy/new propulsion")
    parser.add_argument("--exhaust-comparison", action="store_true", help="Same-binary previous Stage 2 / polished jet")
    parser.add_argument("--blue-comparison", action="store_true", help="Paused blue sheets/wake versus layered animated blue exhaust")
    parser.add_argument("--road-light-comparison", action="store_true", help="Opening road response off/on; blue exhaust remains enabled")
    args = parser.parse_args()
    comparisons = sum((args.comparison, args.exhaust_comparison, args.blue_comparison, args.road_light_comparison))
    if comparisons > 1:
        parser.error("Select only one comparison mode.")
    if args.blue_comparison and (args.mode not in ("propulsion", "performance")):
        parser.error("Blue comparison requires propulsion/performance and no other comparison.")
    if args.exhaust_comparison and args.mode not in ("propulsion", "performance"):
        parser.error("Exhaust comparison requires propulsion/performance and cannot combine legacy comparison.")
    if args.road_light_comparison and args.mode not in ("propulsion", "performance"):
        parser.error("Road-light comparison requires propulsion/performance.")
    if args.mode == "performance" and (args.video or args.hide_hud):
        parser.error("Performance runs must not capture screenshots or change HUD visibility.")
    if args.comparison and args.mode not in ("propulsion", "performance"):
        parser.error("The comparison switch changes propulsion only.")
    with (args.app / "Contents/Info.plist").open("rb") as stream:
        executable = args.app.resolve() / "Contents/MacOS" / plistlib.load(stream)["CFBundleExecutable"]
    args.output = args.output.resolve()
    args.output.mkdir(parents=True, exist_ok=False)
    sizes = ["1280x720", "1600x900", "1920x1080", "2560x1080"] if args.matrix else [args.size]
    results = []
    for size in sizes:
        width, height = map(int, size.split("x"))
        for treatment in (("before", "after") if comparisons else ("after",)):
            folder = args.output / size / treatment
            folder.mkdir(parents=True)
            command = [str(executable), "-screen-fullscreen", "0", "-screen-width", str(width),
                       "-screen-height", str(height), "-stage2Evidence", str(folder),
                       "-stage2Profile", str(args.output / "profile"), "-stage2Mode", args.mode,
                       "-logFile", str(folder / "player.log")]
            if args.video:
                command.append("-stage2Video")
            if args.hide_hud:
                command.append("-stage2HideHud")
            if treatment == "before":
                command += (["-vrRoadLightBaseline"] if args.road_light_comparison else
                            ["-vrBlueBaseline"] if args.blue_comparison else
                            ["-vrExhaustPolish", "off"] if args.exhaust_comparison else ["-vrPropulsion", "off"])
            (folder / "launch.json").write_text(json.dumps({"command": command}, indent=2) + "\n")
            run = subprocess.run(command, timeout=1800 if args.mode == "loop" else 600)
            report_path = folder / "stage2-evidence.json"
            report = json.loads(report_path.read_text()) if report_path.exists() else {}
            passed = run.returncode == 0 and report.get("complete") is True
            results.append({"folder": str(folder), "exitCode": run.returncode,
                            "complete": passed, "error": report.get("error"),
                            "buildGuid": report.get("buildGuid")})
            (args.output / "capture-summary.json").write_text(json.dumps(results, indent=2) + "\n")
            if not passed:
                raise SystemExit(f"Capture failed; preserve and inspect {folder}")
            if (report.get("width"), report.get("height")) != (width, height):
                raise SystemExit(f"Native window resolution differs from requested {size}; inspect {folder}")
    print(json.dumps(results, indent=2))


if __name__ == "__main__":
    main()
