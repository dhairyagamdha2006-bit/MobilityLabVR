#!/usr/bin/env python3
"""Validate and summarize MobilityLab VR Unity telemetry exports."""

from __future__ import annotations

import argparse
from pathlib import Path
import sys

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
import pandas as pd

from telemetry_common import build_scenario_summary, build_trial_summaries, load_telemetry


COLORS = ["#1bb5a6", "#5cbce0", "#f7b731", "#e85b4b"]


def analyze(inputs: list[str], output_directory: str | Path) -> tuple[pd.DataFrame, pd.DataFrame, list[Path]]:
    frame, _ = load_telemetry(inputs)
    trials = build_trial_summaries(frame)
    scenarios = build_scenario_summary(trials)
    output = Path(output_directory)
    output.mkdir(parents=True, exist_ok=True)
    trials_path = output / "summary_by_trial.csv"
    scenario_path = output / "summary_by_scenario.csv"
    trials.to_csv(trials_path, index=False)
    scenarios.to_csv(scenario_path, index=False)
    plots = create_plots(trials, output)
    return trials, scenarios, [trials_path, scenario_path, *plots]


def create_plots(trials: pd.DataFrame, output: Path) -> list[Path]:
    paths: list[Path] = []
    order = sorted(trials["scenario"].dropna().unique())
    if not order:
        return paths

    outcomes = trials.groupby("scenario")[["collision_count", "near_miss_count"]].sum().reindex(order)
    axis = outcomes.plot(kind="bar", color=COLORS[2:4], figsize=(10, 5))
    axis.set_title("Safety events by scenario")
    axis.set_xlabel("Scenario")
    axis.set_ylabel("Recorded events")
    axis.legend(["Collisions", "Near misses"])
    axis.tick_params(axis="x", rotation=20)
    axis.grid(axis="y", alpha=0.25)
    plt.tight_layout()
    paths.append(_save(output / "safety_events_by_scenario.png"))

    paths.extend(
        _boxplot_if_data(
            trials,
            "total_duration_seconds",
            "Trial duration by scenario",
            "Duration (seconds)",
            output / "trial_duration_by_scenario.png",
            order,
        )
    )
    paths.extend(
        _boxplot_if_data(
            trials,
            "minimum_hazard_distance_m",
            "Minimum hazard distance by scenario",
            "Minimum distance (metres)",
            output / "minimum_distance_by_scenario.png",
            order,
        )
    )
    paths.extend(
        _boxplot_if_data(
            trials,
            "reaction_time_seconds",
            "Measured reaction time by scenario",
            "Reaction time (seconds)",
            output / "reaction_time_by_scenario.png",
            order,
        )
    )
    return paths


def _boxplot_if_data(
    trials: pd.DataFrame,
    column: str,
    title: str,
    ylabel: str,
    path: Path,
    order: list[str],
) -> list[Path]:
    data = [trials.loc[trials["scenario"].eq(name), column].dropna().to_numpy() for name in order]
    nonempty = [(name, values) for name, values in zip(order, data) if len(values)]
    if not nonempty:
        return []
    labels, values = zip(*nonempty)
    _, axis = plt.subplots(figsize=(10, 5))
    boxes = axis.boxplot(values, patch_artist=True, showmeans=True)
    axis.set_xticks(range(1, len(labels) + 1), labels)
    for index, box in enumerate(boxes["boxes"]):
        box.set_facecolor(COLORS[index % len(COLORS)])
        box.set_alpha(0.78)
    axis.set_title(title)
    axis.set_xlabel("Scenario")
    axis.set_ylabel(ylabel)
    axis.tick_params(axis="x", rotation=20)
    axis.grid(axis="y", alpha=0.25)
    plt.tight_layout()
    return [_save(path)]


def _save(path: Path) -> Path:
    plt.savefig(path, dpi=160, bbox_inches="tight")
    plt.close()
    return path


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("inputs", nargs="+", help="Telemetry CSV files, directories, or glob patterns")
    parser.add_argument("--output", default="Analysis/output", help="Output directory for summaries and plots")
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    try:
        trials, scenarios, paths = analyze(args.inputs, args.output)
    except ValueError as exc:
        print(f"Analysis failed: {exc}", file=sys.stderr)
        return 2
    print(f"Loaded {len(trials)} trial(s) across {len(scenarios)} scenario(s).")
    for path in paths:
        print(f"Saved {path}")
    print("These descriptive outputs are for a portfolio research prototype and are not scientifically validated.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
