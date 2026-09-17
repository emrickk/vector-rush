#!/usr/bin/env python3
"""Check Stage 2 native evidence coverage; appearance and human input require separate review."""
import argparse
import importlib.util
import json
import math
from pathlib import Path
import re

spec = importlib.util.spec_from_file_location(
    "opening_png_validation", Path(__file__).with_name("validate-opening-evidence.py"))
png_validation = importlib.util.module_from_spec(spec)
spec.loader.exec_module(png_validation)
require = png_validation.require
SCOPE = ("Capture/report integrity and recorded state coverage only. Programmatic menu opening "
         "does not prove pointer, keyboard or physical-controller operation. Simulation-time "
         "recordings do not prove real-time performance, audio, human feel or visual acceptance.")


def finite(value, label):
    require(type(value) in (int, float) and math.isfinite(value), label + ": non-finite number")
    return value


def check(folder):
    report = json.loads((folder / "stage2-evidence.json").read_text())
    require(report.get("complete") is True and not report.get("error"), "Incomplete runtime report")
    require(re.fullmatch(r"[0-9a-fA-F]{32}", report.get("buildGuid", "")), "Missing build GUID")
    require(Path(report.get("profileDirectory", "")).is_absolute(), "Missing isolated profile")
    mode = report.get("mode")
    require(mode in ("screens", "loop", "propulsion", "performance"), "Unknown mode")
    dimensions = report["width"], report["height"]
    require(all(type(v) is int and v > 0 for v in dimensions), "Invalid dimensions")
    images, files, sequence = [], set(), []
    samples, views = report.get("samples", []), report.get("views", [])
    require(isinstance(samples, list) and isinstance(views, list), "Invalid sample/view arrays")
    for sample, is_sequence in [(s, True) for s in samples] + [(s, False) for s in views]:
        require((sample["width"], sample["height"]) == dimensions, "Resolution changed during capture")
        for key in ("raceTime", "simulationTime", "speedKph", "throttle", "energy", "progress",
                    "exhaustDemand", "exhaustResponse", "fov"):
            finite(sample[key], key)
        require(0 <= sample["throttle"] <= 1 and 0 <= sample["energy"] <= 1, "Invalid input/energy")
        for key, axes in (("playerPosition", "xyz"), ("visualPosition", "xyz"),
                          ("cameraPosition", "xyz"), ("velocity", "xyz"),
                          ("playerRotation", "xyzw"), ("visualRotation", "xyzw"),
                          ("cameraRotation", "xyzw")):
            png_validation.vector(sample[key], axes, key)
        filename = sample.get("file")
        if not filename:
            continue
        path = Path(filename)
        require(not path.is_absolute() and ".." not in path.parts and path.suffix == ".png",
                "Invalid capture path")
        require(filename not in files, "Duplicate image reference: " + filename)
        files.add(filename)
        info = png_validation.png_info(folder / path, dimensions)
        info["file"] = filename
        images.append(info)
        if is_sequence:
            sequence.append(filename)
    count = report.get("sequenceFrames")
    require(type(count) is int and count == len(sequence), "Sequence count mismatch")
    require(sequence == [f"frames/frame-{i:05d}.png" for i in range(count)],
            "Sequence has gaps or incorrect names")
    if mode == "performance":
        require(report["simulationCaptureRate"] == 0 and not images and not views,
                "Performance run included screenshots or simulation stepping")
        require(not list(folder.glob("**/*.png")), "Performance folder contains screenshots")
        require(report.get("performanceSamples", 0) >= 20, "Insufficient boost interval samples")
        for key in ("meanMs", "p50Ms", "p95Ms", "p99Ms"):
            require(finite(report[key], key) > 0, "Invalid performance statistic")
        require(report["p50Ms"] <= report["p95Ms"] <= report["p99Ms"], "Unordered percentiles")
    else:
        require(report["simulationCaptureRate"] == 24, "Expected 24 Hz simulation-time evidence")
        require(views, "No screen views captured")
    if mode in ("screens", "loop"):
        required = {"01-title.png", "02-settings.png", "03-controls.png", "03b-bindings.png",
                    "03c-comfort.png", "04-countdown.png", "05-racing.png", "06-paused.png",
                    "07-pause-settings.png", "08-resumed.png", "12-returned-title.png"}
        if mode == "loop":
            required |= {"09-results-provisional.png", "10-results-final.png", "11-retry.png"}
            records = report.get("results", [])
            require(len(records) == 6 and len({r["racerId"] for r in records}) == 6,
                    "Missing/duplicate race results")
            player = next((r for r in records if r["racerId"] == "racer-0"), None)
            require(player is not None and player["status"] == "Finished" and player["hasTime"],
                    "Player did not finish through normal lifecycle")
            require(player["time"] == report["officialFinishTime"] > 0, "Official time mismatch")
        require(required <= files, "Missing screen coverage: " + str(sorted(required - files)))
    else:
        for key in ("cruiseObserved", "accelerationObserved", "boostObserved", "releaseObserved",
                    "emptyBoostObserved"):
            require(report.get(key) is True, "Input stage missing: " + key)
        require(samples and samples[-1]["simulationTime"] - samples[0]["simulationTime"] >= 17.9,
                "Incomplete propulsion passage")
        require(all(s["manualEligible"] is False for s in samples), "Diagnostic counted as manual run")
    return report, {"folder": str(folder.resolve()), "buildGuid": report["buildGuid"],
                    "mode": mode, "dimensions": list(dimensions), "imagesVerified": len(images),
                    "sequenceFrames": count, "images": images}


def compare(before, after):
    require(before["buildGuid"] == after["buildGuid"], "Different A/B binaries")
    if before["propulsionEnabled"] is True and after["propulsionEnabled"] is True:
        require(before.get("exhaustPolishEnabled") is False and after.get("exhaustPolishEnabled") is True,
                "Expected previous Stage 2 / polished exhaust comparison")
    else:
        require(before["propulsionEnabled"] is False and after["propulsionEnabled"] is True,
                "Expected legacy-off / candidate-on comparison")
    require(before["mode"] == after["mode"] and before["mode"] in ("propulsion", "performance"),
            "Only propulsion/performance treatment comparisons are supported")
    require((before["width"], before["height"]) == (after["width"], after["height"]),
            "Different A/B resolutions")
    if before["mode"] == "performance":
        return {"method": "Same identified binary and scripted input schedule; real-time trajectories "
                          "can differ with frame delivery. Frame statistics are source-reported.",
                "meanMsDelta": after["meanMs"] - before["meanMs"],
                "p95MsDelta": after["p95Ms"] - before["p95Ms"]}
    require(len(before["samples"]) == len(after["samples"]), "Different natural frame counts")
    for index, (a, b) in enumerate(zip(before["samples"], after["samples"])):
        require(a["stage"] == b["stage"] and a["phase"] == b["phase"], f"Frame {index}: stage mismatch")
        for key in ("raceTime", "simulationTime", "speedKph", "throttle", "energy", "progress",
                    "fov", "requestedThrottle", "playerPosition", "playerRotation",
                    "visualPosition", "visualRotation", "cameraPosition", "cameraRotation"):
            av, bv = a[key], b[key]
            delta = max(abs(av[k] - bv[k]) for k in av) if isinstance(av, dict) else abs(av - bv)
            if isinstance(av, dict) and "w" in av:
                delta = min(delta, max(abs(av[k] + bv[k]) for k in av))
            require(delta <= .0001, f"Frame {index}: {key} differs by {delta}")
        require(a["boosting"] == b["boosting"] and a["requestedBoost"] == b["requestedBoost"],
                f"Frame {index}: boost mismatch")
    return {"matchedNaturalFrames": len(before["samples"]), "componentTolerance": .0001}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("folder", type=Path)
    parser.add_argument("--after", type=Path, help="Compare this candidate against folder as baseline")
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    try:
        before, summary = check(args.folder)
        result = {"passed": True, "scope": SCOPE, "capture": summary}
        if args.after:
            after, candidate = check(args.after)
            result.update(candidate=candidate, comparison=compare(before, after))
    except Exception as error:
        result = {"passed": False, "scope": SCOPE, "error": str(error)}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2, allow_nan=False) + "\n")
    print(json.dumps({key: result[key] for key in ("passed", "error") if key in result}))
    return 0 if result["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
