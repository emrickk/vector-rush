"""Failure-oriented coverage for the external opening capture acceptance gate."""
import copy
import importlib.util
import json
from pathlib import Path
import struct
import subprocess
import sys
import tempfile
import unittest
import zlib

SCRIPT = Path(__file__).with_name("validate-opening-evidence.py")
spec = importlib.util.spec_from_file_location("opening_validator", SCRIPT)
validator = importlib.util.module_from_spec(spec)
spec.loader.exec_module(validator)


def png():
    def chunk(kind, data):
        return (struct.pack(">I", len(data)) + kind + data +
                struct.pack(">I", zlib.crc32(kind + data) & 0xffffffff))
    return (b"\x89PNG\r\n\x1a\n" +
            chunk(b"IHDR", struct.pack(">IIBBBBB", 2, 2, 8, 2, 0, 0, 0)) +
            chunk(b"IDAT", zlib.compress(b"\0" * 14)) + chunk(b"IEND", b""))


class EvidenceTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.before, self.after = self.root / "before", self.root / "after"
        frames = []
        for i in range(432):
            p, q = {"x": i*.01, "y": 0, "z": 0}, {"x": 0, "y": 0, "z": 0, "w": 1}
            progress = .005 + i*.001
            anchor = next((name for name, threshold in validator.ANCHORS.items()
                           if i and .005+(i-1)*.001 < threshold <= progress), "")
            frames.append({"file": f"frames/frame-{i:04d}.png", "anchor": anchor,
                           "time": 1.6+i/24, "progress": progress, "speed": 150, "fov": 67,
                           "cameraPosition": p, "cameraRotation": q,
                           "racerPositions": [p]*6, "racerRotations": [q]*6,
                           "visualPositions": [p]*6, "visualRotations": [q]*6})
        self.report = {"complete": True, "buildGuid": "a"*32, "revision": "test",
                       "rate": 24, "width": 2, "height": 2, "frames": frames}
        for folder, enabled in ((self.before, False), (self.after, True)):
            (folder / "frames").mkdir(parents=True)
            self.write(folder, dict(self.report, openingEnabled=enabled))
            for frame in frames:
                (folder / frame["file"]).write_bytes(png())
            for anchor in validator.ANCHORS:
                (folder / (anchor+".png")).write_bytes(png())

    def write(self, folder, report):
        (folder / "opening-evidence.json").write_text(json.dumps(report))

    def change(self, callback):
        report = json.loads((self.after / "opening-evidence.json").read_text())
        callback(report)
        self.write(self.after, report)

    def check(self):
        return validator.validate(self.before, self.after, (2, 2))

    def test_complete_sequence_passes(self):
        result = self.check()
        self.assertEqual(result["matchedFrames"], 432)
        self.assertEqual(result["after"]["imagesVerified"], 435)

    def test_non_anchor_visual_pose_mismatch_fails(self):
        self.change(lambda r: r["frames"][77]["visualPositions"][3].update(x=4))
        with self.assertRaisesRegex(ValueError, "visualPositions"):
            self.check()

    def test_missing_sequence_frame_fails(self):
        (self.after / "frames/frame-0420.png").unlink()
        with self.assertRaises(FileNotFoundError):
            self.check()

    def test_corrupt_png_fails(self):
        path = self.after / "frames/frame-0123.png"
        path.write_bytes(path.read_bytes()[:-7])
        with self.assertRaisesRegex(ValueError, "truncated"):
            self.check()

    def test_wrong_identity_mode_complete_and_count_fail(self):
        for key, value in (("buildGuid", "b"*32), ("openingEnabled", False),
                           ("complete", False), ("width", 3), ("rate", 30),
                           ("frames", self.report["frames"][:-1])):
            with self.subTest(key=key):
                report = copy.deepcopy(dict(self.report, openingEnabled=True))
                report[key] = value
                self.write(self.after, report)
                with self.assertRaises(ValueError):
                    self.check()

    def test_nonfinite_or_skipped_time_fails(self):
        for value in (float("nan"), 100):
            report = copy.deepcopy(dict(self.report, openingEnabled=True))
            report["frames"][101]["time"] = value
            self.write(self.after, report)
            with self.assertRaises(ValueError):
                self.check()

    def test_anchor_moved_off_crossing_fails(self):
        def move(r):
            index = next(i for i, f in enumerate(r["frames"]) if f["anchor"])
            r["frames"][index+1]["anchor"] = r["frames"][index]["anchor"]
            r["frames"][index]["anchor"] = ""
        self.change(move)
        with self.assertRaisesRegex(ValueError, "first threshold"):
            self.check()

    def test_quaternion_antipodes_pass(self):
        self.change(lambda r: r["frames"][77]["cameraRotation"].update(w=-1))
        self.assertTrue(self.check()["passed"])

    def test_cli_failure_writes_json_and_nonzero(self):
        (self.before / "01-approach.png").unlink()
        output = self.root / "result.json"
        run = subprocess.run([sys.executable, str(SCRIPT), str(self.before), str(self.after),
                              "--width", "2", "--height", "2", "--output", str(output)],
                             capture_output=True, text=True)
        self.assertEqual(run.returncode, 1)
        self.assertFalse(json.loads(output.read_text())["passed"])


if __name__ == "__main__":
    unittest.main()
