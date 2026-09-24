# Telemetry schema

Schema version: `1.0`

One UTF-8 CSV is written per trial, accompanied by a JSONL file containing
safety events. CSV values use invariant culture and RFC 4180-compatible quoting:
fields containing commas, quotes, carriage returns, or newlines are quoted and
embedded quotes are doubled.

## Record types

| `record_type` | When written | Purpose |
| --- | --- | --- |
| `sample` | Configurable periodic interval; 10 Hz default | Trajectory and state |
| `event` | Immediately when a safety event is classified | Explainable event snapshot |
| `summary` | Once when a trial closes | Final status, duration, counts, reaction, score |

## Columns

| Column | Type / unit | Description |
| --- | --- | --- |
| `schema_version` | string | Schema contract, currently `1.0` |
| `record_type` | enum | `sample`, `event`, or `summary` |
| `participant_id` | string | Sanitized anonymous/session code; maximum 32 characters |
| `trial_id` | string | UTC-derived unique ID within the application session |
| `scenario` | string | Human-readable scenario name |
| `random_seed` | integer | Effective scenario seed including the definition offset |
| `timestamp_utc` | ISO 8601 | UTC write time with round-trip precision |
| `elapsed_seconds` | seconds | Monotonic active trial time; paused time is excluded |
| `position_x` | metres | Scooter world X coordinate |
| `position_y` | metres | Scooter world Y coordinate |
| `position_z` | metres | Scooter world Z coordinate |
| `speed_mps` | m/s | Absolute longitudinal scooter speed |
| `longitudinal_input` | `[-1,1]` | Requested throttle/brake axis |
| `braking_state` | enum | `accelerating`, `coasting`, `normal`, `emergency`, or `reversing` |
| `distance_to_hazard_m` | metres | Designated hazard distance, or nearest active hazard; `-1` if unavailable |
| `minimum_hazard_distance_m` | metres | Smallest surface separation so far; `-1` if none observed |
| `traffic_signal_state` | enum | Rider-facing `Green`, `Yellow`, or `Red` |
| `event_type` | enum / blank | Collision, NearMiss, ExcessiveSpeed, SuddenBraking, completion, timeout, or activation |
| `event_details` | string / blank | Human-readable classification reason and thresholds |
| `event_closing_speed_mps` | m/s / blank | Positive relative approach speed used for event classification |
| `event_tca_seconds` | seconds / blank | Predicted time to closest approach |
| `collision_count` | integer | Trial cumulative collisions at this row |
| `near_miss_count` | integer | Trial cumulative near misses at this row |
| `reaction_time_seconds` | seconds / blank | Hazard-to-first-brake interval when measurable |
| `completion_status` | enum / blank | Final `Completed`, `TimedOut`, `Reset`, or `Aborted` on summary rows |
| `total_duration_seconds` | seconds / blank | Final active duration on summary rows |
| `safety_score` | `[0,100]` / blank | Unvalidated demonstration heuristic on summary rows |

Blank values mean “not applicable or not yet measurable,” not zero.

## Near-miss calculation

Let `r` be the horizontal relative position from rider to hazard, `v` the
horizontal hazard velocity minus rider velocity, and `R` the combined collision
radii. The classifier calculates:

```text
closing_speed = -dot(normalize(r), v)
t_closest = clamp(-dot(r, v) / dot(v, v), 0, prediction_horizon)
predicted_surface_separation = max(0, length(r + v * t_closest) - R)
```

The event is recorded when closing speed is at least `0.75 m/s`, predicted
surface separation is no more than `1.5 m`, and time to closest approach is in
`(0.01 s, 1.6 s]`. The same hazard ID is suppressed for `4 s`. The details field
stores the measured values and thresholds so the decision is auditable.

## Demonstration safety score

The score starts at 100 and subtracts:

- 35 per collision;
- 12 per near miss;
- 3 per excessive-speed event;
- 2 per sudden-braking event;
- up to 12 points for minimum distance below 1.5 m;
- 10 for timeout or 15 for abort.

The result is clamped to `[0,100]`. This presentation heuristic has not been
scientifically validated and must not be treated as a diagnostic or safety
assessment.

## Persistence behavior

- Directory: `Application.persistentDataPath/MobilityLabVR/Telemetry`
- CSV filename suffix: `.telemetry.csv`
- Event filename suffix: `.events.jsonl`
- Periodic flush: every 25 rows by default
- Immediate flush: after every safety event
- Cleanup: trial end, component destruction, and application quit

If the directory cannot be opened or a write fails, Unity logs the error and the
recorder closes its writers rather than blocking the simulation.
