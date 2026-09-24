# Contributing to MobilityLab VR

Thank you for improving the prototype. Contributions should preserve its core
goals: repeatable behavior, desktop accessibility, transparent research
measures, and a self-contained repository.

## Development setup

1. Use Unity 6.3 LTS; avoid beta or experimental Editor versions.
2. Run `Tools > MobilityLab VR > Create or Rebuild Demo` after first import.
3. Work under `Assets/MobilityLabVR` for project-owned Unity content.
4. Keep Python analysis independent from the Unity runtime.
5. Never commit `Library`, `Temp`, `Logs`, `Obj`, builds, local telemetry, virtual
   environments, participant identifiers, credentials, or editor caches.

## Code standards

- Use the `MobilityLabVR` namespace.
- Prefer focused components and dependency injection through `Configure`
  methods over scene-wide searches.
- Avoid allocation-heavy work in `Update`/`FixedUpdate`; use cached references,
  events, and non-allocating physics queries.
- Add tooltips and sensible bounds to tunable serialized fields.
- Add or update tests when changing pure logic or data formats.
- Do not silence warnings without documenting why they are unavoidable.
- Keep deterministic behavior seeded and do not use unseeded randomness in
  scenario outcomes.

## Research and telemetry rules

- Collect only anonymous session codes; never add names, emails, biometrics, or
  other sensitive data to the default schema.
- Treat thresholds, labels, and scores as prototype definitions, not validated
  safety claims.
- Update `Documentation/TelemetrySchema.md` and the Python expected-column list
  together whenever the schema changes.
- Label synthetic records and outputs conspicuously.

## Assets

Use original procedural content, public-domain assets, or properly licensed
free assets. Record the source, author, license, and modifications in
`Documentation/ThirdPartyNotices.md`. Do not add paid Asset Store content.

## Before a pull request

1. Rebuild and validate the demo from the Tools menu.
2. Run Unity Edit Mode and Play Mode tests.
3. Run `python3 -m unittest discover -s Analysis/tests -v`.
4. Make a desktop development build and inspect the Player log.
5. Confirm no local telemetry, generated caches, secrets, or large binaries are
   staged.
6. Update the changelog and relevant documentation.

Describe actual test results and remaining limitations. Do not report tests,
performance figures, screenshots, or functionality that were not observed.
