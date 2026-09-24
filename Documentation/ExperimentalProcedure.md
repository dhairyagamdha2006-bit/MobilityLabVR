# Experimental procedure

This procedure is appropriate for a portfolio demonstration or software pilot.
It is **not** an approved human-subject protocol. Obtain the necessary
institutional review, consent, privacy, accessibility, and risk approvals before
collecting data from people for research.

## Research question

> How do hazardous traffic conditions affect rider reaction time, minimum
> hazard distance, and near-miss frequency in a controlled micromobility
> simulation?

## Demonstration variables

- Independent condition: Baseline, Sudden Pedestrian, Vehicle Fails to Yield,
  or Low Visibility.
- Outcomes: measured brake reaction time, minimum hazard distance, near-miss
  count, collision count, and completion time.
- Context: seed, signal phase, speed, control state, and timestamped trajectory.

The safety score is presentation feedback only and should not be an inferential
outcome.

## Before a session

1. Confirm the Unity build and exact software version.
2. Verify desktop controls, output storage, and all four scenarios with a test
   session code.
3. If using VR, complete the checklist in `VRSetup.md`, sanitize the headset,
   configure the guardian/boundary, and provide a stable seated position.
4. Remove earlier demonstration CSV files from the collection directory or use
   a new study-specific folder outside source control.
5. Choose a counterbalanced scenario order before data collection. The menu
   permits free selection; it does not claim to counterbalance automatically.
6. Explain stopping criteria and allow the participant to withdraw immediately.

## Trial procedure

1. Enter an anonymous code such as `P014`. Do not enter a name, email, student
   ID, or other identifying information.
2. Read the controls and complete a non-recorded familiarization run if an
   approved protocol calls for one.
3. Select the assigned scenario and start the experiment.
4. Travel from the marked start line to the teal destination gate while
   responding naturally to traffic controls and hazards.
5. Stop the trial for discomfort, loss of tracking, unexpected software
   behavior, or a participant request.
6. Record only whether the file was produced successfully; do not manually edit
   raw telemetry.
7. Repeat using the preregistered order and rest periods.

## Operational definitions

- Reaction time: seconds between designated scenario-hazard activation and the
  first observed normal/emergency brake. It remains missing if no designated
  hazard exists or braking is not observed.
- Minimum hazard distance: smallest registered surface-to-surface distance from
  the scooter to an active vehicle/pedestrian hazard during the trial.
- Near miss: the transparent relative-motion rule documented in
  `TelemetrySchema.md`; one hazard is suppressed for four seconds after a
  record.
- Collision: a new physical collider contact, rate-limited by object for one
  second.
- Completion: the rider enters the destination trigger before timeout.

These definitions are implementation choices and require validation before use
as scientific measures.

## Data handling

- Treat telemetry as research data even though the default ID is anonymous.
- Keep raw files read-only, record checksums, and analyze copies.
- Do not commit participant telemetry to GitHub.
- Define retention, access, encryption, sharing, deletion, and breach-response
  policies before collection.
- A session code may still become identifying when combined with scheduling or
  external records; minimize linkages.

## Analysis cautions

Use participant-aware repeated-measures methods for a real study; do not treat
multiple trials from one person as independent. Predefine exclusions, missing
reaction-time handling, outlier rules, and multiplicity corrections. The
included logistic regression is a software demonstration, not a validated risk
model.
