"""Shared telemetry loading and feature engineering for MobilityLab VR.

The functions in this module operate on exported Unity files only. They do not
participate in the Unity runtime and intentionally make no causal or scientific
claims about rider safety.
"""

from __future__ import annotations

from pathlib import Path
from typing import Iterable, Sequence
import glob
import warnings

import pandas as pd


SCHEMA_VERSION = "1.0"
EXPECTED_COLUMNS = [
    "schema_version",
    "record_type",
    "participant_id",
    "trial_id",
    "scenario",
    "random_seed",
    "timestamp_utc",
    "elapsed_seconds",
    "position_x",
    "position_y",
    "position_z",
    "speed_mps",
    "longitudinal_input",
    "braking_state",
    "distance_to_hazard_m",
    "minimum_hazard_distance_m",
    "traffic_signal_state",
    "event_type",
    "event_details",
    "event_closing_speed_mps",
    "event_tca_seconds",
    "collision_count",
    "near_miss_count",
    "reaction_time_seconds",
    "completion_status",
    "total_duration_seconds",
    "safety_score",
]

NUMERIC_COLUMNS = [
    "random_seed",
    "elapsed_seconds",
    "position_x",
    "position_y",
    "position_z",
    "speed_mps",
    "longitudinal_input",
    "distance_to_hazard_m",
    "minimum_hazard_distance_m",
    "event_closing_speed_mps",
    "event_tca_seconds",
    "collision_count",
    "near_miss_count",
    "reaction_time_seconds",
    "total_duration_seconds",
    "safety_score",
]


def resolve_input_files(inputs: Sequence[str | Path]) -> list[Path]:
    """Resolve files, directories, and glob expressions without duplicates."""
    resolved: list[Path] = []
    seen: set[Path] = set()
    for raw in inputs:
        value = str(raw)
        path = Path(value).expanduser()
        candidates: Iterable[Path]
        if path.is_dir():
            candidates = sorted(path.rglob("*.telemetry.csv"))
        elif path.is_file():
            candidates = [path]
        else:
            candidates = [Path(match) for match in sorted(glob.glob(value, recursive=True))]

        for candidate in candidates:
            canonical = candidate.resolve()
            if canonical.is_file() and canonical not in seen:
                seen.add(canonical)
                resolved.append(canonical)
    return resolved


def load_telemetry(inputs: Sequence[str | Path]) -> tuple[pd.DataFrame, list[str]]:
    """Load valid telemetry files and report skipped/malformed input as warnings.

    A file missing required schema columns is skipped so one corrupt export does
    not discard an otherwise valid batch. A ValueError is raised only if no valid
    data remains.
    """
    files = resolve_input_files(inputs)
    if not files:
        raise ValueError("No telemetry CSV files matched the supplied inputs.")

    frames: list[pd.DataFrame] = []
    messages: list[str] = []
    for path in files:
        malformed_row_count = 0

        def skip_malformed_row(_fields: list[str]) -> None:
            nonlocal malformed_row_count
            malformed_row_count += 1
            return None

        try:
            frame = pd.read_csv(
                path,
                dtype=str,
                keep_default_na=False,
                na_values=[""],
                on_bad_lines=skip_malformed_row,
                engine="python",
            )
        except (OSError, UnicodeError, pd.errors.ParserError) as exc:
            messages.append(f"Skipped {path}: {exc}")
            continue

        if malformed_row_count:
            messages.append(f"{path}: ignored {malformed_row_count} malformed row(s) with the wrong field count")

        missing = [column for column in EXPECTED_COLUMNS if column not in frame.columns]
        if missing:
            messages.append(f"Skipped {path}: missing required columns {', '.join(missing)}")
            continue

        frame = frame[EXPECTED_COLUMNS].copy()
        empty_identity = frame["trial_id"].isna() | frame["scenario"].isna()
        if empty_identity.any():
            count = int(empty_identity.sum())
            messages.append(f"{path}: ignored {count} row(s) with no trial_id or scenario")
            frame = frame.loc[~empty_identity].copy()
        if frame.empty:
            messages.append(f"Skipped {path}: no usable rows")
            continue

        for column in NUMERIC_COLUMNS:
            before = frame[column].notna().sum()
            frame[column] = pd.to_numeric(frame[column], errors="coerce")
            malformed = before - frame[column].notna().sum()
            if malformed:
                messages.append(f"{path}: coerced {malformed} malformed value(s) in {column} to missing")
        frame["source_file"] = str(path)
        frames.append(frame)

    if not frames:
        detail = "; ".join(messages) if messages else "no readable rows"
        raise ValueError(f"No valid MobilityLab VR telemetry could be loaded: {detail}")

    for message in messages:
        warnings.warn(message, RuntimeWarning, stacklevel=2)
    return pd.concat(frames, ignore_index=True), messages


def build_trial_summaries(frame: pd.DataFrame) -> pd.DataFrame:
    """Collapse sample, event, and summary records into one row per trial."""
    required = set(EXPECTED_COLUMNS)
    missing = sorted(required.difference(frame.columns))
    if missing:
        raise ValueError(f"Telemetry frame is missing columns: {', '.join(missing)}")

    rows: list[dict[str, object]] = []
    keys = ["participant_id", "trial_id", "scenario", "random_seed"]
    for key, group in frame.groupby(keys, dropna=False, sort=True):
        participant, trial_id, scenario, seed = key
        samples = group.loc[group["record_type"].eq("sample")]
        summary_rows = group.loc[group["record_type"].eq("summary")]
        speeds = samples["speed_mps"].dropna()
        distances = group["minimum_hazard_distance_m"].dropna()
        distances = distances.loc[distances >= 0]
        reactions = group["reaction_time_seconds"].dropna()
        reactions = reactions.loc[reactions >= 0]

        status_values = group["completion_status"].dropna().astype(str)
        duration_values = summary_rows["total_duration_seconds"].dropna()
        duration = float(duration_values.iloc[-1]) if not duration_values.empty else _safe_max(group["elapsed_seconds"])
        score_values = summary_rows["safety_score"].dropna()
        score = float(score_values.iloc[-1]) if not score_values.empty else float("nan")
        braking = samples["braking_state"].fillna("").astype(str).str.lower()
        moving_on_red = samples["traffic_signal_state"].fillna("").eq("Red") & samples["speed_mps"].fillna(0).gt(0.5)

        rows.append(
            {
                "participant_id": participant,
                "trial_id": trial_id,
                "scenario": scenario,
                "random_seed": seed,
                "completion_status": status_values.iloc[-1] if not status_values.empty else "Unknown",
                "completed": bool(not status_values.empty and status_values.iloc[-1] == "Completed"),
                "total_duration_seconds": duration,
                "collision_count": int(_safe_max(group["collision_count"], default=0)),
                "near_miss_count": int(_safe_max(group["near_miss_count"], default=0)),
                "minimum_hazard_distance_m": float(distances.min()) if not distances.empty else float("nan"),
                "reaction_time_seconds": float(reactions.iloc[0]) if not reactions.empty else float("nan"),
                "safety_score": score,
                "max_speed_mps": _safe_max(speeds),
                "mean_speed_mps": float(speeds.mean()) if not speeds.empty else float("nan"),
                "speed_std_mps": float(speeds.std(ddof=0)) if not speeds.empty else float("nan"),
                "mean_longitudinal_input": float(samples["longitudinal_input"].mean()) if not samples.empty else float("nan"),
                "braking_fraction": float(braking.isin(["normal", "emergency"]).mean()) if not samples.empty else float("nan"),
                "emergency_brake_fraction": float(braking.eq("emergency").mean()) if not samples.empty else float("nan"),
                "red_signal_moving_fraction": float(moving_on_red.mean()) if not samples.empty else float("nan"),
                "sample_count": int(len(samples)),
            }
        )
    return pd.DataFrame(rows)


def build_scenario_summary(trials: pd.DataFrame) -> pd.DataFrame:
    """Create descriptive, non-inferential aggregate statistics by scenario."""
    if trials.empty:
        raise ValueError("No trial summaries are available.")
    grouped = trials.groupby("scenario", sort=True, dropna=False)
    result = grouped.agg(
        trials=("trial_id", "count"),
        completion_rate=("completed", "mean"),
        total_collisions=("collision_count", "sum"),
        mean_collisions=("collision_count", "mean"),
        total_near_misses=("near_miss_count", "sum"),
        mean_near_misses=("near_miss_count", "mean"),
        mean_completion_seconds=("total_duration_seconds", "mean"),
        median_completion_seconds=("total_duration_seconds", "median"),
        mean_minimum_distance_m=("minimum_hazard_distance_m", "mean"),
        median_minimum_distance_m=("minimum_hazard_distance_m", "median"),
        mean_reaction_time_seconds=("reaction_time_seconds", "mean"),
        median_reaction_time_seconds=("reaction_time_seconds", "median"),
    ).reset_index()
    result["completion_rate"] = result["completion_rate"].round(4)
    return result


def _safe_max(values: pd.Series, default: float = float("nan")) -> float:
    numeric = pd.to_numeric(values, errors="coerce").dropna()
    return float(numeric.max()) if not numeric.empty else default
