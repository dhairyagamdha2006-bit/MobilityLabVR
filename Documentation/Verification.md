# Verification record

Date: 2026-09-24 (UTC)

This file records what was actually available in the repository-authoring
environment. Update it with logs and results after opening the project in Unity.

## Environment observed

| Tool | Result |
| --- | --- |
| Unity Editor / Hub | Not installed; no Unity executable found |
| .NET SDK / C# compiler | Not installed |
| Python | 3.12.14 |
| Git | 2.51.1 |
| pandas | 2.2.3 |
| Matplotlib | 3.10.8 |
| scikit-learn | 1.8.0 |
| joblib | 1.5.3 |

The repository targets Unity 6.3 LTS (`6000.3.0f1`). Unity’s current release
page identified Unity 6.3 as the available LTS family when the project was
authored.

## Executed checks

Command:

```bash
python3 -m unittest discover -s Analysis/tests -v
```

Actual result:

```text
Ran 5 tests in 0.392s
OK
```

Covered behavior:

- load and aggregate a generated synthetic export;
- retain all four scenario labels and valid distances;
- coerce/report a malformed numeric value;
- reject a CSV missing required columns;
- exclude collision and near-miss outcomes from model features;
- reproduce a seeded train/test evaluation; and
- save model, metrics, and coefficient artifacts.

The synthetic files were created only in temporary test directories and were
not added to the repository.

An additional temporary end-to-end workflow generated 48 explicitly synthetic
trials, loaded all four scenarios, wrote both summary CSVs and four plots, fit a
seeded 36/12 logistic-regression split, and confirmed the model, metrics, and
coefficient files existed. The temporary directory was then removed. Model
performance is intentionally not reported as a project result because the data
was artificial.

## Static repository checks performed

- Required top-level directories and files were created.
- No generated Unity cache directories are present.
- No TODO, FIXME, placeholder exception, API key, or paid-asset reference is
  intentionally included.
- Runtime C# sources use the `MobilityLabVR` namespace.
- Bootstrap scenes point to committed script GUIDs and Build Settings contains
  both scenes.
- Python source compiles during test import and the end-to-end tests execute.
- Final automated static audit passed across 42 C# files, 79 Unity asset GUIDs,
  14 Markdown files, and matching 27-column Unity/Python telemetry schemas.

Static checks do not substitute for Unity compilation or playtesting.

## Required first-open Unity verification

- [ ] Install Unity 6.3 LTS and open the repository root.
- [ ] Confirm all manifest packages resolve without version errors.
- [ ] Run `Tools > MobilityLab VR > Create or Rebuild Demo` twice and confirm it
      remains idempotent.
- [ ] Run `Tools > MobilityLab VR > Validate Project`.
- [ ] Confirm the Console has no compilation errors, missing-reference errors,
      unhandled exceptions, or unexplained warnings.
- [ ] Run all Edit Mode tests and save the XML/log.
- [ ] Run all Play Mode tests and save the XML/log.
- [ ] Play every scenario from the Main Menu in desktop mode.
- [ ] Check scooter acceleration, normal/emergency brake, reverse, steering,
      collision response, reset, mouse look, pause, and destination.
- [ ] Observe vehicles stopping for signals/agents and pedestrians waiting for
      their phase.
- [ ] Confirm special hazards offer a reasonable reaction opportunity.
- [ ] Complete and time out trials; verify results and output path.
- [ ] Open CSV/JSONL, check escaping and final summary, then analyze the real
      test export with Python.
- [ ] Build and launch a desktop development player.
- [ ] Profile target-laptop frame time before making a performance claim.
- [ ] Follow `VRSetup.md` for OpenXR and optional simulator/hardware checks.
- [ ] Capture genuine screenshots and update the README.

## Batch commands

Replace `UNITY` with the platform-specific Editor executable:

```bash
"$UNITY" -batchmode -projectPath "$PWD" \
  -executeMethod MobilityLabVR.Editor.ProjectSetup.CreateOrRebuildDemo \
  -logFile Logs/setup.log -quit
"$UNITY" -batchmode -projectPath "$PWD" -runTests -testPlatform EditMode \
  -testResults Logs/editmode-results.xml -logFile Logs/editmode.log
"$UNITY" -batchmode -projectPath "$PWD" -runTests -testPlatform PlayMode \
  -testResults Logs/playmode-results.xml -logFile Logs/playmode.log
"$UNITY" -batchmode -projectPath "$PWD" \
  -executeMethod MobilityLabVR.Editor.BuildAutomation.PerformDevelopmentBuild \
  -logFile Logs/build.log -quit
```

Do not mark an item complete or publish a result until its output has actually
been observed.
