# MobilityLab VR analysis tools

These scripts analyze telemetry exported by the Unity application. They are
offline research-demo tools and are never required to run the simulator.

## Setup

```bash
cd Analysis
python3 -m venv .venv
source .venv/bin/activate              # Windows: .venv\Scripts\activate
python -m pip install -r requirements.txt
```

## Analyze Unity exports

Pass one file, a directory, or a quoted glob. The command writes per-trial and
per-scenario CSV summaries plus labeled PNG plots.

```bash
python analyze_sessions.py "/path/to/Telemetry/*.telemetry.csv" --output output
```

## Synthetic demonstration workflow

If no Unity trial has been recorded yet, generate artificial data solely to
exercise the scripts:

```bash
python generate_synthetic_data.py --output sample_data/SYNTHETIC_demo.telemetry.csv --trials 80
python analyze_sessions.py sample_data/SYNTHETIC_demo.telemetry.csv --output output
python train_risk_model.py sample_data/SYNTHETIC_demo.telemetry.csv --output models --seed 42
```

The generator labels participant IDs, filename, console output, and event text
as **SYNTHETIC**. Its output is not human-participant data and must not be used
as scientific evidence.

## Model

`train_risk_model.py` fits a regularized logistic regression after median
imputation, standardization, and scenario one-hot encoding. It uses a seeded,
stratified train/test split and saves:

- `risk_model.joblib`
- `risk_model_metrics.json`
- `feature_coefficients.csv`

The demo label is `collision > 0 OR near miss > 0 OR safety score < 70`.
Outcome counts are deliberately excluded from predictor features. A model built
from synthetic or small demonstration data does not generalize to real riders.

## Tests

```bash
python -m unittest discover -s tests -v
```
