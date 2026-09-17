import copy
import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location(
    "stage2_validator", Path(__file__).with_name("validate-stage2-evidence.py"))
validator = importlib.util.module_from_spec(spec)
spec.loader.exec_module(validator)


class ComparisonTests(unittest.TestCase):
    def setUp(self):
        sample = {"stage": "boost", "phase": "Racing", "boosting": True, "requestedBoost": True}
        for key in ("raceTime", "simulationTime", "speedKph", "throttle", "energy", "progress",
                    "fov", "requestedThrottle"):
            sample[key] = 1
        for key in ("playerPosition", "visualPosition", "cameraPosition"):
            sample[key] = {"x": 0, "y": 0, "z": 0}
        for key in ("playerRotation", "visualRotation", "cameraRotation"):
            sample[key] = {"x": 0, "y": 0, "z": 0, "w": 1}
        self.before = {"buildGuid": "a"*32, "propulsionEnabled": False, "mode": "propulsion",
                       "width": 1600, "height": 900, "samples": [sample]}
        self.after = copy.deepcopy(self.before)
        self.after["propulsionEnabled"] = True

    def test_visual_effect_response_may_change_without_changing_physics(self):
        self.before["samples"][0]["exhaustResponse"] = .2
        self.after["samples"][0]["exhaustResponse"] = .5
        self.assertEqual(validator.compare(self.before, self.after)["matchedNaturalFrames"], 1)

    def test_camera_input_or_visual_pose_changes_fail(self):
        for field in ("cameraPosition", "playerPosition", "visualPosition"):
            with self.subTest(field=field):
                candidate = copy.deepcopy(self.after)
                candidate["samples"][0][field]["x"] = .01
                with self.assertRaisesRegex(ValueError, field):
                    validator.compare(self.before, candidate)
        self.after["samples"][0]["requestedBoost"] = False
        with self.assertRaisesRegex(ValueError, "boost mismatch"):
            validator.compare(self.before, self.after)

    def test_polish_comparison_requires_both_stage2_and_opposite_polish_flags(self):
        self.before.update(propulsionEnabled=True, exhaustPolishEnabled=False)
        self.after.update(exhaustPolishEnabled=True)
        self.assertEqual(validator.compare(self.before, self.after)["matchedNaturalFrames"], 1)
        self.after["exhaustPolishEnabled"] = False
        with self.assertRaisesRegex(ValueError, "polished exhaust"):
            validator.compare(self.before, self.after)

    def test_wrong_binary_cannot_be_matched(self):
        self.after["buildGuid"] = "b"*32
        with self.assertRaisesRegex(ValueError, "binaries"):
            validator.compare(self.before, self.after)

    def test_missing_sample_fails(self):
        self.after["samples"] = []
        with self.assertRaisesRegex(ValueError, "frame counts"):
            validator.compare(self.before, self.after)

    def test_real_time_performance_is_not_claimed_pose_matched(self):
        for report in (self.before, self.after):
            report.update(mode="performance", meanMs=10, p95Ms=15)
        self.after.update(meanMs=12, p95Ms=18, samples=[])
        result = validator.compare(self.before, self.after)
        self.assertEqual(result["meanMsDelta"], 2)
        self.assertNotIn("matchedNaturalFrames", result)


if __name__ == "__main__":
    unittest.main()
