# Scenario design

Scenarios are `ScenarioDefinition` ScriptableObjects with a kind, display name,
description, seed offset, difficulty, timeout, activation distance, visibility
parameters, and hazard speed. Four editable assets are committed under
`Assets/MobilityLabVR/Resources/Scenarios`; the Editor setup tool refreshes them,
and runtime defaults keep a fresh clone functional if they are unavailable.

## Shared geometry

The rider starts approximately 38 m south of the intersection in a marked
northbound micromobility lane. The activation zone is placed at a configurable
distance south of the intersection. A destination gate approximately 38 m north
provides an unambiguous completion condition. Signal timing begins from the same
north–south green phase on every reset.

## Conditions

| Scenario | Seed offset | Trigger | Hazard behavior | Reaction opportunity |
| --- | ---: | --- | --- | --- |
| Baseline | 101 | Zone has no special activation | All agents obey signals and crosswalk phases | Normal intersection observation |
| Sudden Pedestrian | 211 | 16 m south | Waiting pedestrian begins a 2.1 m/s crossing before its walk phase | Visible approach and emergency braking capacity; not an unavoidable spawn |
| Vehicle Fails to Yield | 307 | 18 m south | A 4.4 m/s eastbound vehicle ignores the signal, turns north, and merges across the rider path | Vehicle begins in view outside the conflict point and follows a continuous route |
| Low Visibility | 401 | No special moving hazard | Fog density 0.03, reduced ambient and sun intensity; normal traffic rules | Geometry and signals remain visible at a reduced contrast |

The routes and values are plausible demonstration settings, not calibrated
traffic-engineering parameters.

## Determinism and reset

- The menu derives a stable integer from anonymous ID, scenario, and repetition.
- The definition-specific offset produces the effective seed recorded in the
  telemetry.
- Reset restores scooter pose/velocity, agents, signal phase, trigger,
  visibility, event suppression, counters, minimum distance, and trial clock.
- The `R` key repeats with the same seed; a new telemetry file prevents earlier
  records from being overwritten.

## Avoiding “gotcha” collisions

Special agents exist at their route start rather than materializing on top of
the rider. Activation distances are longer than the stopping distance of the
default scooter under emergency braking at its speed limit. Exact experience
still depends on rendering, physics, frame rate, and user speed, so the first
Unity playtest should tune route timing if target hardware differs.

## Adding or modifying a condition

1. Add or edit a `ScenarioDefinition` asset.
2. Keep activation distances above the worst-case stopping distance plus a
   perception margin.
3. Implement special behavior through a focused agent method rather than in
   telemetry or UI code.
4. Reset every new state deterministically.
5. Add tests for seed/reset logic and event classification.
6. Record the rationale, units, limitations, and any calibration evidence.
