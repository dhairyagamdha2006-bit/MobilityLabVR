#!/usr/bin/env python3
"""Generate prominently labeled synthetic telemetry for pipeline demonstrations.

This output is artificial. It must never be represented as human-participant
data, scientific evidence, or a measure of real-world rider safety.
"""

from __future__ import annotations

import argparse
import csv
from datetime import datetime, timezone, timedelta
import math
from pathlib import Path
import random

from telemetry_common import EXPECTED_COLUMNS, SCHEMA_VERSION


SCENARIOS = ["Baseline", "Sudden Pedestrian", "Vehicle Fails to Yield", "Low Visibility"]


def generate(path: str | Path, trials: int = 80, seed: int = 2026) -> Path:
    if trials < 12:
        raise ValueError("Generate at least 12 trials so the model workflow can split the data.")
    rng = random.Random(seed)
    destination = Path(path)
    destination.parent.mkdir(parents=True, exist_ok=True)
    start_time = datetime(2026, 1, 1, tzinfo=timezone.utc)

    with destination.open("w", newline="", encoding="utf-8") as handle:
        writer = csv.DictWriter(handle, fieldnames=EXPECTED_COLUMNS)
        writer.writeheader()
        for trial_index in range(trials):
            scenario = SCENARIOS[trial_index % len(SCENARIOS)]
            difficulty = SCENARIOS.index(scenario)
            participant = f"SYNTHETIC-P{trial_index % 20:03d}"
            trial_id = f"synthetic-trial-{trial_index:04d}"
            trial_seed = seed + trial_index
            duration = rng.uniform(24, 45) + difficulty * 2
            max_speed = rng.uniform(5.0, 8.8) + difficulty * 0.18
            reaction = max(0.25, rng.gauss(0.85 + difficulty * 0.2, 0.22)) if difficulty else math.nan
            minimum_distance = max(0.05, rng.gauss(3.2 - difficulty * 0.65, 0.7))
            risk_probability = min(0.85, 0.07 + difficulty * 0.13 + max(0, max_speed - 7) * 0.08)
            high_risk = rng.random() < risk_probability
            collision_count = 1 if high_risk and rng.random() < 0.18 + difficulty * 0.04 else 0
            near_miss_count = 1 if high_risk and collision_count == 0 else 0
            if high_risk:
                minimum_distance = min(minimum_distance, rng.uniform(0.05, 1.25))
            safety_score = max(0, 100 - collision_count * 35 - near_miss_count * 12 - max(0, 1.5 - minimum_distance) * 8)
            samples = max(12, int(duration // 2))
            timestamp = start_time + timedelta(minutes=trial_index * 3)

            for sample_index in range(samples):
                elapsed = duration * sample_index / max(1, samples - 1)
                progress = sample_index / max(1, samples - 1)
                speed = max(0, math.sin(progress * math.pi) * max_speed + rng.gauss(0, 0.18))
                braking = "emergency" if high_risk and 0.55 < progress < 0.62 else ("normal" if progress > 0.85 else "accelerating")
                distance = max(minimum_distance, abs(0.58 - progress) * 35)
                writer.writerow(
                    _row(
                        "sample", participant, trial_id, scenario, trial_seed, timestamp, elapsed,
                        speed=speed, longitudinal=1 if progress < 0.7 else -0.35,
                        braking=braking, distance=distance, minimum=minimum_distance,
                        signal="Red" if 0.42 < progress < 0.52 else "Green",
                        collisions=collision_count if progress > 0.62 else 0,
                        near_misses=near_miss_count if progress > 0.62 else 0,
                        reaction=reaction,
                    )
                )

            if collision_count or near_miss_count:
                event_type = "Collision" if collision_count else "NearMiss"
                event = _row(
                    "event", participant, trial_id, scenario, trial_seed, timestamp, duration * 0.6,
                    speed=max_speed * 0.8, braking="emergency", distance=minimum_distance,
                    minimum=minimum_distance, signal="Green", collisions=collision_count,
                    near_misses=near_miss_count, reaction=reaction,
                )
                event["event_type"] = event_type
                event["event_details"] = "SYNTHETIC demonstration event; not observed from a human participant."
                event["event_closing_speed_mps"] = f"{rng.uniform(1, 6):.3f}"
                event["event_tca_seconds"] = f"{rng.uniform(0.1, 1.4):.3f}"
                writer.writerow(event)

            summary = _row(
                "summary", participant, trial_id, scenario, trial_seed, timestamp, duration,
                speed=0, braking="coasting", distance=minimum_distance, minimum=minimum_distance,
                signal="Green", collisions=collision_count, near_misses=near_miss_count, reaction=reaction,
            )
            summary["completion_status"] = "Completed"
            summary["total_duration_seconds"] = f"{duration:.3f}"
            summary["safety_score"] = f"{safety_score:.1f}"
            writer.writerow(summary)
    return destination


def _row(
    record_type: str,
    participant: str,
    trial_id: str,
    scenario: str,
    seed: int,
    timestamp: datetime,
    elapsed: float,
    *,
    speed: float,
    braking: str,
    distance: float,
    minimum: float,
    signal: str,
    collisions: int,
    near_misses: int,
    reaction: float,
    longitudinal: float = 0,
) -> dict[str, object]:
    row: dict[str, object] = {column: "" for column in EXPECTED_COLUMNS}
    row.update(
        {
            "schema_version": SCHEMA_VERSION,
            "record_type": record_type,
            "participant_id": participant,
            "trial_id": trial_id,
            "scenario": scenario,
            "random_seed": seed,
            "timestamp_utc": (timestamp + timedelta(seconds=elapsed)).isoformat(),
            "elapsed_seconds": f"{elapsed:.3f}",
            "position_x": "5.500",
            "position_y": "0.150",
            "position_z": f"{-38 + elapsed * 1.8:.3f}",
            "speed_mps": f"{speed:.3f}",
            "longitudinal_input": f"{longitudinal:.3f}",
            "braking_state": braking,
            "distance_to_hazard_m": f"{distance:.3f}",
            "minimum_hazard_distance_m": f"{minimum:.3f}",
            "traffic_signal_state": signal,
            "collision_count": collisions,
            "near_miss_count": near_misses,
            "reaction_time_seconds": "" if math.isnan(reaction) else f"{reaction:.3f}",
        }
    )
    return row


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", default="Analysis/sample_data/SYNTHETIC_demo.telemetry.csv")
    parser.add_argument("--trials", type=int, default=80)
    parser.add_argument("--seed", type=int, default=2026)
    args = parser.parse_args()
    path = generate(args.output, args.trials, args.seed)
    print(f"Created SYNTHETIC demonstration data at {path}")
    print("Do not represent this file as human-participant data or scientific evidence.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
