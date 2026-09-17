#!/usr/bin/env python3
"""Validate complete native opening A/B recordings; never infer art or play quality."""
import argparse
import hashlib
import json
import math
from pathlib import Path
import re
import struct
import zlib

COUNT, RATE = 432, 24
ANCHORS = {"01-approach": .15, "02-bend": .22, "03-reveal": .30}
TOLERANCE = .0001
SCOPE = (
    "Recorded native camera/FOV, racer and visual-pose parity, simulation cadence, "
    "build identity and complete PNG integrity/coverage only. This does not establish "
    "visual acceptance, continuously observed motion, audio, real-time performance, "
    "human driving or absence of transform injection; capture provenance requires "
    "the identified runtime source and launch command."
)


def require(condition, message):
    if not condition:
        raise ValueError(message)


def number(value, label):
    require(type(value) in (int, float) and math.isfinite(value),
            f"{label}: expected finite number")
    return value


def vector(value, keys, label):
    require(isinstance(value, dict) and set(value) == set(keys), f"{label}: invalid vector")
    result = tuple(number(value[k], label + "." + k) for k in keys)
    if keys == "xyzw":
        require(abs(sum(x*x for x in result) - 1) < .001, f"{label}: non-unit quaternion")
    return result


def png_info(path, dimensions):
    """Check PNG framing, CRCs, decompression and complete 8-bit RGB(A) scanlines."""
    data = path.read_bytes()
    require(data[:8] == b"\x89PNG\r\n\x1a\n", f"{path}: not a PNG")
    pos, compressed, header, ended = 8, bytearray(), None, False
    while pos < len(data):
        require(pos + 12 <= len(data), f"{path}: truncated chunk")
        length = struct.unpack(">I", data[pos:pos+4])[0]
        require(pos + 12 + length <= len(data), f"{path}: truncated payload")
        kind = data[pos+4:pos+8]
        payload = data[pos+8:pos+8+length]
        crc = struct.unpack(">I", data[pos+8+length:pos+12+length])[0]
        require(zlib.crc32(kind + payload) & 0xffffffff == crc, f"{path}: CRC mismatch")
        if kind == b"IHDR":
            require(pos == 8 and header is None and length == 13, f"{path}: invalid IHDR")
            header = struct.unpack(">IIBBBBB", payload)
        elif kind == b"IDAT":
            require(header is not None, f"{path}: IDAT before IHDR")
            compressed.extend(payload)
        elif kind == b"IEND":
            require(length == 0, f"{path}: invalid IEND")
            ended = True
            pos += 12
            break
        pos += 12 + length
    require(ended and pos == len(data) and header is not None, f"{path}: incomplete PNG")
    w, h, depth, color, compression, filtering, interlace = header
    require((w, h) == dimensions, f"{path}: dimensions {(w, h)} != {dimensions}")
    require(depth == 8 and color in (2, 6) and
            (compression, filtering, interlace) == (0, 0, 0),
            f"{path}: unsupported native screenshot encoding")
    stride = w * (3 if color == 2 else 4) + 1
    expected = stride * h
    decoder = zlib.decompressobj()
    raw = decoder.decompress(bytes(compressed), expected + 1)
    require(len(raw) == expected and decoder.eof and not decoder.unused_data
            and not decoder.unconsumed_tail, f"{path}: invalid scanline payload")
    require(all(raw[i] <= 4 for i in range(0, len(raw), stride)), f"{path}: invalid row filter")
    return {"file": path.name, "bytes": len(data), "sha256": hashlib.sha256(data).hexdigest()}


def check_capture(folder, enabled, dimensions):
    report_path = folder / "opening-evidence.json"
    report = json.loads(report_path.read_text())
    require(isinstance(report, dict), f"{folder}: report must be an object")
    require(report.get("complete") is True, f"{folder}: capture incomplete")
    require(report.get("openingEnabled") is enabled, f"{folder}: incorrect opening mode")
    require(isinstance(report.get("buildGuid"), str) and
            re.fullmatch(r"[0-9a-fA-F]{32}", report["buildGuid"]), f"{folder}: invalid build GUID")
    require(isinstance(report.get("revision"), str) and report["revision"],
            f"{folder}: missing revision")
    require(type(report.get("rate")) is int and report["rate"] == RATE,
            f"{folder}: expected {RATE} Hz simulation rate")
    require((report.get("width"), report.get("height")) == dimensions,
            f"{folder}: unexpected resolution")
    frames = report.get("frames")
    require(isinstance(frames, list) and len(frames) == COUNT,
            f"{folder}: expected {COUNT} frames")
    anchors, images = {}, []
    for i, frame in enumerate(frames):
        label = f"{folder}/frame {i}"
        require(isinstance(frame, dict), f"{label}: frame must be an object")
        expected_file = f"frames/frame-{i:04d}.png"
        require(frame.get("file") == expected_file, f"{label}: noncanonical/missing frame file")
        for key in ("time", "progress", "speed", "fov"):
            number(frame.get(key), label + "." + key)
        require(0 < frame["fov"] < 180 and frame["speed"] >= 0, f"{label}: invalid FOV/speed")
        require(0 <= frame["progress"] < 1, f"{label}: invalid first-lap progress")
        vector(frame.get("cameraPosition"), "xyz", label + ".cameraPosition")
        vector(frame.get("cameraRotation"), "xyzw", label + ".cameraRotation")
        for key, axes in (("racerPositions", "xyz"), ("racerRotations", "xyzw"),
                          ("visualPositions", "xyz"), ("visualRotations", "xyzw")):
            values = frame.get(key)
            require(isinstance(values, list) and len(values) == 6, f"{label}: expected six {key}")
            for j, value in enumerate(values):
                vector(value, axes, f"{label}.{key}[{j}]")
        if i:
            previous = frames[i-1]
            require(abs(frame["time"] - previous["time"] - 1/RATE) <= .011,
                    f"{label}: skipped/duplicate simulation sample")
            require(frame["progress"] > previous["progress"], f"{label}: non-forward progress")
        anchor = frame.get("anchor", "")
        require(anchor == "" or anchor in ANCHORS, f"{label}: unexpected anchor")
        if anchor:
            require(anchor not in anchors, f"{label}: duplicate anchor")
            require(i > 0 and frames[i-1]["progress"] < ANCHORS[anchor] <= frame["progress"],
                    f"{label}: anchor is not first threshold crossing")
            anchors[anchor] = i
        info = png_info(folder / expected_file, dimensions)
        info["file"] = expected_file
        images.append(info)
    require(.005 <= frames[0]["progress"] < .01, f"{folder}: wrong passage start")
    require(abs(frames[-1]["time"] - frames[0]["time"] - (COUNT-1)/RATE) <= .011,
            f"{folder}: wrong simulation duration")
    require(set(anchors) == set(ANCHORS), f"{folder}: missing anchors")
    for name in ANCHORS:
        images.append(png_info(folder / (name + ".png"), dimensions))
    return report, {
        "folder": str(folder.resolve()), "buildGuid": report["buildGuid"],
        "revision": report["revision"], "openingEnabled": enabled,
        "reportSha256": hashlib.sha256(report_path.read_bytes()).hexdigest(),
        "resolution": list(dimensions), "framesVerified": COUNT, "anchors": anchors,
        "imagesVerified": len(images), "images": images,
        "raceTimeStart": frames[0]["time"], "raceTimeEnd": frames[-1]["time"],
        "simulationSeconds": COUNT/RATE,
    }


def compare(a, b):
    require(a["buildGuid"] == b["buildGuid"], "A/B build GUID mismatch")
    require(a["revision"] == b["revision"], "A/B opening revision mismatch")
    maximum = 0
    for i, (af, bf) in enumerate(zip(a["frames"], b["frames"])):
        require(af.get("anchor", "") == bf.get("anchor", ""), f"frame {i}: anchor parity mismatch")
        for key in ("time", "progress", "speed", "fov", "cameraPosition", "cameraRotation",
                    "racerPositions", "racerRotations", "visualPositions", "visualRotations"):
            av, bv = af[key], bf[key]
            pairs = zip(av, bv) if isinstance(av, list) else [(av, bv)]
            for j, (left, right) in enumerate(pairs):
                if isinstance(left, dict):
                    delta = max(abs(left[k] - right[k]) for k in left)
                    if "w" in left:  # q and -q are the same physical orientation.
                        delta = min(delta, max(abs(left[k] + right[k]) for k in left))
                else:
                    delta = abs(left - right)
                maximum = max(maximum, delta)
                require(delta <= TOLERANCE,
                        f"frame {i} {key}[{j}]: pose/scalar mismatch {delta} > {TOLERANCE}")
    return maximum


def validate(before, after, dimensions=(1600, 900)):
    a, ac = check_capture(before, False, dimensions)
    b, bc = check_capture(after, True, dimensions)
    maximum = compare(a, b)
    return {"passed": True, "scope": SCOPE, "before": ac, "after": bc,
            "matchedFrames": COUNT, "maxComponentDifference": maximum,
            "componentTolerance": TOLERANCE, "simulationRate": RATE}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("before", type=Path)
    parser.add_argument("after", type=Path)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--width", type=int, default=1600)
    parser.add_argument("--height", type=int, default=900)
    args = parser.parse_args()
    try:
        result = validate(args.before, args.after, (args.width, args.height))
    except (OSError, ValueError, KeyError, TypeError, struct.error, zlib.error) as error:
        result = {"passed": False, "scope": SCOPE, "error": str(error)}
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2, allow_nan=False) + "\n")
    print(json.dumps({key: result[key] for key in ("passed", "error", "matchedFrames")
                      if key in result}))
    return 0 if result["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
