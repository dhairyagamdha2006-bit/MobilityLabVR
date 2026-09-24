# Changelog

All notable changes to MobilityLab VR are documented here. The format follows
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project intends
to use semantic versioning after its first verified Unity release.

## [0.1.0] - 2026-09-24

### Added

- Unity 6.3 LTS project configuration with URP, Input System, XR Management,
  OpenXR, uGUI, and Unity Test Framework dependencies.
- Programmatic main menu, instructions, scenario selection, settings, HUD,
  pause menu, and experiment-results interface.
- Desktop/XR-separated input architecture and rigidbody scooter controller.
- Procedural low-poly campus intersection, buildings, signals, signage,
  landscaping, micromobility lane, start line, and destination gate.
- Coordinated traffic-signal state machine, waypoint vehicles, crosswalk
  pedestrians, and four repeatable scenarios.
- Explainable collision, near-miss, speeding, braking, completion, and timeout
  detection.
- Anonymous experiment workflow with CSV state/event/summary records and JSONL
  safety events.
- Python aggregation, plots, synthetic demonstration generator, interpretable
  logistic-regression training, and five unit tests.
- Unity Edit Mode logic tests, Play Mode bootstrap smoke test, deterministic
  scene/setup utility, validator, and desktop build automation.
- Architecture, procedure, telemetry, scenario, VR, demo, verification, and
  GitHub-publishing documentation.

### Verification boundary

- Python unit tests pass in the authoring environment.
- Unity compilation, Unity tests, rendering, build, screenshots, and XR checks
  remain pending because the Unity Editor was unavailable.
