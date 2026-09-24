# 60–90 second demo recording plan

Record only the running project and outputs you actually verify. Do not use
mocked screenshots, staged telemetry presented as observed data, or unmeasured
performance claims.

## Preparation

1. Complete the Unity verification checklist in `Verification.md`.
2. Use a test ID such as `DEMO001`; remove personal notifications from view.
3. Record at 1080p/60 if the laptop sustains it; otherwise use 1080p/30 and state
   no frame-rate claim.
4. Keep the Unity Console clear and prepare the telemetry directory plus a
   terminal with the Python environment activated.
5. Capture three genuine stills after recording: menu, intersection/HUD, and
   results. Save them in `Screenshots/` and link them from the README.

## Shot list

| Time | Visual | Suggested caption | Suggested narration |
| ---: | --- | --- | --- |
| 0–7 s | Polished main menu; enter `DEMO001` | `Anonymous, repeatable sessions` | “MobilityLab VR is a desktop-first, OpenXR-ready micromobility safety research prototype.” |
| 7–14 s | Instructions and scenario selector | `Four deterministic traffic conditions` | “Each scenario uses a recorded seed and the same modular simulation systems.” |
| 14–24 s | Baseline riding, HUD, compliant vehicle/pedestrian | `Signals coordinate vehicles and crosswalks` | “Waypoint agents obey a finite-state signal controller and detect traffic ahead.” |
| 24–34 s | Sudden Pedestrian from approach view | `Controlled hazard with reaction opportunity` | “The pedestrian starts from a visible route after a configurable trigger—not an unavoidable spawn.” |
| 34–44 s | Vehicle Fails to Yield | `Scenario-specific agent behavior` | “A turning vehicle ignores the normal yield rule while the rest of the traffic remains deterministic.” |
| 44–53 s | Emergency brake; near-miss HUD count or real collision | `Explainable safety-event detection` | “Near misses use separation, closing speed, and time-to-closest-approach thresholds with duplicate suppression.” |
| 53–62 s | Destination and results screen | `Transparent demonstration metrics` | “The result summarizes duration, distance, reaction, and events; the score is explicitly unvalidated.” |
| 62–72 s | Open genuine CSV and JSONL | `10 Hz samples + immediate event records` | “Every trial writes an escaped CSV plus an event log under Unity’s persistent data path.” |
| 72–82 s | Run analyzer; show genuine plots | `Offline reproducible analysis` | “The Python pipeline validates schema, summarizes conditions, plots outcomes, and can train an interpretable demo model.” |
| 82–90 s | Project Settings OpenXR / VRSetup document | `Optional OpenXR; no headset required` | “Input is device-independent, so reviewers can run everything on desktop and add OpenXR hardware separately.” |

## Capture cautions

- If a near miss does not occur naturally, do another trial; do not edit the HUD
  or CSV.
- Label any synthetic-data plot on screen as synthetic and say it is a pipeline
  demonstration, not a research result.
- Do not state frame rate, latency, accuracy, risk-model performance, or
  usability findings unless the displayed measurement comes from a documented
  run.
- Keep the telemetry path visible long enough to show end-to-end traceability.
