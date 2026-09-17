#!/usr/bin/env python3
"""Encode an existing Stage 2 native sequence; invoke under the shared heavy-process lease."""
import argparse
from fractions import Fraction
import hashlib
import json
from pathlib import Path
import shutil
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("capture", type=Path)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    folder = args.capture.resolve()
    source = folder / "stage2-evidence.json"
    report = json.loads(source.read_text())
    if report.get("complete") is not True or report.get("simulationCaptureRate") != 24:
        parser.error("A complete 24 Hz native capture report is required.")
    count = report.get("sequenceFrames", 0)
    if type(count) is not int or count <= 0:
        parser.error("The report has no continuous sequence.")
    expected = {f"frame-{i:05d}.png" for i in range(count)}
    actual = {p.name for p in (folder / "frames").glob("*.png")}
    if actual != expected:
        parser.error(f"Frame coverage mismatch: {len(expected-actual)} missing, {len(actual-expected)} extra.")
    output = (args.output or folder / (report["mode"] + ".mp4")).resolve()
    metadata = output.with_suffix(output.suffix + ".encoding.json")
    temporary = output.with_name("." + output.stem + ".partial.mp4")
    if output.exists() or temporary.exists() or metadata.exists():
        parser.error("Output/temporary/encoding report already exists; choose a fresh output.")
    ffmpeg, ffprobe = shutil.which("ffmpeg"), shutil.which("ffprobe")
    if not ffmpeg or not ffprobe:
        parser.error("ffmpeg and ffprobe must be on PATH.")
    output.parent.mkdir(parents=True, exist_ok=True)
    command = [ffmpeg, "-hide_banner", "-loglevel", "error", "-nostdin",
               "-framerate", "24", "-start_number", "0", "-i", str(folder / "frames/frame-%05d.png"),
               "-frames:v", str(count), "-an", "-c:v", "libx264", "-preset", "medium", "-crf", "18",
               "-pix_fmt", "yuv420p", "-fps_mode", "cfr", "-movflags", "+faststart", str(temporary)]
    subprocess.run(command, check=True)
    probe = json.loads(subprocess.check_output(
        [ffprobe, "-v", "error", "-count_frames", "-show_streams", "-show_format",
         "-of", "json", str(temporary)], text=True))
    streams = probe.get("streams", [])
    video = [s for s in streams if s.get("codec_type") == "video"]
    if len(video) != 1 or any(s.get("codec_type") == "audio" for s in streams):
        raise SystemExit("Encoded output must contain exactly one video stream and no audio.")
    stream = video[0]
    checks = (
        int(stream.get("nb_read_frames", -1)) == count,
        (stream.get("width"), stream.get("height")) == (report["width"], report["height"]),
        Fraction(stream.get("avg_frame_rate", "0")) == 24,
        stream.get("codec_name") == "h264",
        stream.get("pix_fmt") == "yuv420p",
        abs(float(probe["format"]["duration"]) - count/24) <= .05,
    )
    if not all(checks):
        raise SystemExit("ffprobe verification failed; preserved partial output for inspection.")
    temporary.rename(output)
    result = {"verified": True, "buildGuid": report["buildGuid"], "capture": str(folder),
              "captureReportSha256": hashlib.sha256(source.read_bytes()).hexdigest(),
              "file": str(output), "videoSha256": hashlib.sha256(output.read_bytes()).hexdigest(),
              "frames": count, "rate": 24, "seconds": count/24,
              "width": report["width"], "height": report["height"], "audioStreams": 0,
              "scope": "Silent 24 Hz simulation-time recording. Encoding verified by ffprobe; "
                       "not real-time performance, sound review or human driving.",
              "encodeCommand": command, "probe": probe}
    metadata.write_text(json.dumps(result, indent=2) + "\n")
    print(json.dumps({k: result[k] for k in ("verified", "file", "frames", "seconds", "buildGuid")}))


if __name__ == "__main__":
    main()
