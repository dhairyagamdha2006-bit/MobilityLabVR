#!/usr/bin/env python3
"""Train an interpretable demonstration risk classifier from trial telemetry.

The binary label is a project-specific demonstration outcome: a trial is marked
high risk when it contains a collision, a near miss, or a final safety score
below 70. This label is not a clinical or transportation-safety ground truth.
"""

from __future__ import annotations

import argparse
import json
from pathlib import Path
import sys

import joblib
import numpy as np
import pandas as pd
from sklearn.compose import ColumnTransformer
from sklearn.impute import SimpleImputer
from sklearn.linear_model import LogisticRegression
from sklearn.metrics import accuracy_score, balanced_accuracy_score, classification_report, confusion_matrix, roc_auc_score
from sklearn.model_selection import train_test_split
from sklearn.pipeline import Pipeline
from sklearn.preprocessing import OneHotEncoder, StandardScaler

from telemetry_common import build_trial_summaries, load_telemetry


NUMERIC_FEATURES = [
    "max_speed_mps",
    "mean_speed_mps",
    "speed_std_mps",
    "mean_longitudinal_input",
    "braking_fraction",
    "emergency_brake_fraction",
    "red_signal_moving_fraction",
    "minimum_hazard_distance_m",
    "reaction_time_seconds",
    "total_duration_seconds",
]
CATEGORICAL_FEATURES = ["scenario"]
FEATURES = [*NUMERIC_FEATURES, *CATEGORICAL_FEATURES]


def build_model_dataset(frame: pd.DataFrame) -> tuple[pd.DataFrame, pd.Series, pd.DataFrame]:
    trials = build_trial_summaries(frame)
    label = (
        trials["collision_count"].gt(0)
        | trials["near_miss_count"].gt(0)
        | trials["safety_score"].fillna(100).lt(70)
    ).astype(int)
    return trials[FEATURES].copy(), label.rename("high_risk"), trials


def train_model(
    frame: pd.DataFrame,
    output_directory: str | Path,
    random_seed: int = 42,
    test_size: float = 0.25,
) -> dict[str, object]:
    features, labels, trials = build_model_dataset(frame)
    if len(features) < 12:
        raise ValueError(f"At least 12 trials are required for a demonstration split; found {len(features)}.")
    counts = labels.value_counts()
    if len(counts) < 2 or counts.min() < 2:
        raise ValueError("Training requires at least two high-risk and two lower-risk trials.")

    numeric_pipeline = Pipeline(
        [
            ("imputer", SimpleImputer(strategy="median")),
            ("scaler", StandardScaler()),
        ]
    )
    categorical_pipeline = Pipeline(
        [
            ("imputer", SimpleImputer(strategy="most_frequent")),
            ("one_hot", OneHotEncoder(handle_unknown="ignore")),
        ]
    )
    transform = ColumnTransformer(
        [
            ("numeric", numeric_pipeline, NUMERIC_FEATURES),
            ("categorical", categorical_pipeline, CATEGORICAL_FEATURES),
        ]
    )
    pipeline = Pipeline(
        [
            ("features", transform),
            (
                "classifier",
                LogisticRegression(
                    random_state=random_seed,
                    class_weight="balanced",
                    max_iter=2000,
                    solver="liblinear",
                ),
            ),
        ]
    )

    x_train, x_test, y_train, y_test = train_test_split(
        features,
        labels,
        test_size=test_size,
        random_state=random_seed,
        stratify=labels,
    )
    pipeline.fit(x_train, y_train)
    predicted = pipeline.predict(x_test)
    probabilities = pipeline.predict_proba(x_test)[:, 1]
    metrics: dict[str, object] = {
        "random_seed": random_seed,
        "train_trials": int(len(x_train)),
        "test_trials": int(len(x_test)),
        "positive_trials_total": int(labels.sum()),
        "accuracy": float(accuracy_score(y_test, predicted)),
        "balanced_accuracy": float(balanced_accuracy_score(y_test, predicted)),
        "confusion_matrix": confusion_matrix(y_test, predicted, labels=[0, 1]).tolist(),
        "classification_report": classification_report(y_test, predicted, output_dict=True, zero_division=0),
        "roc_auc": float(roc_auc_score(y_test, probabilities)) if y_test.nunique() == 2 else None,
        "limitations": [
            "The project-specific label is not validated ground truth.",
            "Synthetic or demonstration trials do not establish generalization to real riders.",
            "Small datasets produce unstable estimates; use grouped participant splits for real studies.",
            "The model is offline and is not part of the Unity runtime or a safety-critical decision path.",
        ],
    }

    output = Path(output_directory)
    output.mkdir(parents=True, exist_ok=True)
    model_path = output / "risk_model.joblib"
    metrics_path = output / "risk_model_metrics.json"
    coefficients_path = output / "feature_coefficients.csv"
    joblib.dump(
        {
            "pipeline": pipeline,
            "input_features": FEATURES,
            "numeric_features": NUMERIC_FEATURES,
            "categorical_features": CATEGORICAL_FEATURES,
            "label_definition": "collision_count > 0 OR near_miss_count > 0 OR safety_score < 70",
            "research_status": "demonstration-only; not scientifically validated",
            "training_trial_ids": trials["trial_id"].tolist(),
        },
        model_path,
    )
    metrics_path.write_text(json.dumps(metrics, indent=2), encoding="utf-8")

    transformed_names = pipeline.named_steps["features"].get_feature_names_out()
    coefficients = pipeline.named_steps["classifier"].coef_[0]
    pd.DataFrame(
        {"feature": transformed_names, "log_odds_coefficient": coefficients, "absolute_magnitude": np.abs(coefficients)}
    ).sort_values("absolute_magnitude", ascending=False).to_csv(coefficients_path, index=False)
    metrics["model_path"] = str(model_path)
    metrics["metrics_path"] = str(metrics_path)
    metrics["coefficients_path"] = str(coefficients_path)
    return metrics


def parse_args(argv: list[str] | None = None) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("inputs", nargs="+", help="Telemetry CSV files, directories, or glob patterns")
    parser.add_argument("--output", default="Analysis/models", help="Output directory")
    parser.add_argument("--seed", type=int, default=42, help="Reproducible train/test split seed")
    parser.add_argument("--test-size", type=float, default=0.25, help="Fraction held out for evaluation")
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(argv)
    if not 0.1 <= args.test_size <= 0.5:
        print("--test-size must be between 0.1 and 0.5", file=sys.stderr)
        return 2
    try:
        frame, _ = load_telemetry(args.inputs)
        metrics = train_model(frame, args.output, args.seed, args.test_size)
    except ValueError as exc:
        print(f"Training failed: {exc}", file=sys.stderr)
        return 2

    print(json.dumps(metrics, indent=2))
    print("LIMITATION: this demonstration model must not be interpreted as validated evidence about real riders.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
