from __future__ import annotations

import csv
from pathlib import Path
import sys
import tempfile
import unittest
import warnings

import pandas as pd

ANALYSIS_DIRECTORY = Path(__file__).resolve().parents[1]
if str(ANALYSIS_DIRECTORY) not in sys.path:
    sys.path.insert(0, str(ANALYSIS_DIRECTORY))

from generate_synthetic_data import generate
from telemetry_common import EXPECTED_COLUMNS, build_scenario_summary, build_trial_summaries, load_telemetry
from train_risk_model import FEATURES, build_model_dataset, train_model


class TelemetryPipelineTests(unittest.TestCase):
    def test_load_and_summarize_synthetic_export(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            source = generate(Path(directory) / "SYNTHETIC.telemetry.csv", trials=16, seed=17)
            frame, messages = load_telemetry([source])
            trials = build_trial_summaries(frame)
            scenarios = build_scenario_summary(trials)

            self.assertFalse(messages)
            self.assertEqual(len(trials), 16)
            self.assertEqual(
                set(trials["scenario"]),
                {"Baseline", "Sudden Pedestrian", "Vehicle Fails to Yield", "Low Visibility"},
            )
            self.assertEqual(scenarios["trials"].sum(), 16)
            self.assertTrue(trials["minimum_hazard_distance_m"].ge(0).all())

    def test_malformed_numeric_value_is_coerced_and_reported(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "malformed.telemetry.csv"
            row = {column: "" for column in EXPECTED_COLUMNS}
            row.update(
                {
                    "schema_version": "1.0",
                    "record_type": "sample",
                    "participant_id": "P001",
                    "trial_id": "trial-1",
                    "scenario": "Baseline",
                    "random_seed": "42",
                    "speed_mps": "not-a-number",
                    "elapsed_seconds": "1.0",
                    "collision_count": "0",
                    "near_miss_count": "0",
                }
            )
            with path.open("w", newline="", encoding="utf-8") as handle:
                writer = csv.DictWriter(handle, fieldnames=EXPECTED_COLUMNS)
                writer.writeheader()
                writer.writerow(row)
                handle.write(",".join(["extra"] * (len(EXPECTED_COLUMNS) + 2)) + "\n")

            with warnings.catch_warnings(record=True) as caught:
                warnings.simplefilter("always")
                frame, messages = load_telemetry([path])
            self.assertTrue(pd.isna(frame.loc[0, "speed_mps"]))
            self.assertTrue(any("speed_mps" in message for message in messages))
            self.assertTrue(any("wrong field count" in message for message in messages))
            self.assertTrue(any("speed_mps" in str(item.message) for item in caught))

    def test_missing_required_columns_rejected(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / "wrong.csv"
            path.write_text("trial_id,scenario\none,Baseline\n", encoding="utf-8")
            with self.assertRaisesRegex(ValueError, "No valid MobilityLab VR telemetry"):
                load_telemetry([path])

    def test_model_dataset_excludes_outcome_counts_from_features(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            source = generate(Path(directory) / "SYNTHETIC.telemetry.csv", trials=24, seed=7)
            frame, _ = load_telemetry([source])
            features, labels, _ = build_model_dataset(frame)

            self.assertEqual(list(features.columns), FEATURES)
            self.assertNotIn("collision_count", features.columns)
            self.assertNotIn("near_miss_count", features.columns)
            self.assertTrue(set(labels.unique()).issubset({0, 1}))

    def test_training_is_reproducible_and_saves_interpretable_outputs(self) -> None:
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = generate(root / "SYNTHETIC.telemetry.csv", trials=48, seed=99)
            frame, _ = load_telemetry([source])
            first = train_model(frame, root / "model-a", random_seed=13)
            second = train_model(frame, root / "model-b", random_seed=13)

            self.assertEqual(first["accuracy"], second["accuracy"])
            self.assertEqual(first["balanced_accuracy"], second["balanced_accuracy"])
            self.assertTrue(Path(first["model_path"]).is_file())
            self.assertTrue(Path(first["metrics_path"]).is_file())
            coefficients = pd.read_csv(first["coefficients_path"])
            self.assertTrue(
                {"feature", "log_odds_coefficient", "absolute_magnitude"}.issubset(coefficients.columns)
            )


if __name__ == "__main__":
    unittest.main()
