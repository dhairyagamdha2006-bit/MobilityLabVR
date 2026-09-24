# OpenXR and desktop setup

VR is optional. The simulator must be verified in desktop mode first and remains
fully usable when no headset is connected.

## Included architecture

- `com.unity.xr.management` and `com.unity.xr.openxr` are declared packages.
- `XRInputSource` maps common left-controller thumbstick input to throttle and
  steering, the right thumbstick to optional look, right trigger to emergency
  brake, primary button to reset, and menu button to pause.
- `XRHeadPoseDriver` applies relative head pose only when a valid HMD is present.
- `CompositeInputSource` merges XR with desktop input without changing scooter,
  scenarios, safety, or telemetry.
- With no XR devices, both XR adapters return neutral values.

## Target-machine configuration

1. Open the project with Unity 6.3 LTS and wait for packages to resolve.
2. Open **Edit > Project Settings > XR Plug-in Management**.
3. Select the desktop tab and enable **OpenXR** for the intended OS.
4. Open **OpenXR** settings, add only the controller interaction profile needed
   by the available headset, and resolve every item in **Project Validation**.
5. Keep **Initialize XR on Startup** enabled only for an XR build. A desktop
   reviewer can leave it disabled.
6. Run `Tools > MobilityLab VR > Create or Rebuild Demo`, open MainMenu, and
   verify desktop mode again before connecting hardware.
7. Start the platform OpenXR runtime, connect the headset, and enter Play Mode.

The repository does not preselect vendor-specific profiles because committing a
profile that is unavailable on the reviewer’s platform can create avoidable
validation warnings.

## Comfort choices

- Use a seated or stable standing position; the experience simulates a vehicle
  rather than room-scale walking.
- No artificial camera bob, roll, or forced head rotation is applied.
- HMD translation/orientation is relative to the pose captured at trial start.
- Steering rotates the vehicle body gradually; keep initial speeds low while
  assessing comfort.
- Escape/menu pauses immediately, and desktop fallback remains available.
- Stop on discomfort. Do not conduct a human study without an approved safety
  and adverse-event procedure.

## XR Device Simulator (optional)

The base repository avoids an XR Interaction Toolkit dependency because its
simulator is a package sample and is not needed for grading the desktop build.
To validate simulated headset/controller paths without hardware:

1. Install the stable XR Interaction Toolkit version recommended by Unity 6.3
   through Package Manager.
2. In its **Samples** tab, import **XR Device Simulator** and its required input
   actions.
3. Add the simulator prefab to a temporary validation scene or the generated
   Simulation scene.
4. Do not commit imported sample content unless its license and version are
   recorded in `ThirdPartyNotices.md`.
5. Verify HMD rotation/translation, thumbsticks, trigger braking, reset, and
   pause, then discard or isolate the temporary sample changes.

Desktop keyboard/mouse input is the always-available development simulator for
all scenario, physics, safety, UI, and telemetry behavior.

## Hardware verification checklist

- [ ] OpenXR project validation has no unresolved errors.
- [ ] Main menu remains readable in the headset.
- [ ] Desktop mode still starts with the headset disconnected.
- [ ] HMD pose is centered and recaptured after trial reset.
- [ ] Controller throttle, steering, emergency brake, reset, and pause work.
- [ ] No duplicate AudioListener or camera warnings occur.
- [ ] Pausing stops trial time and releases desktop cursor lock.
- [ ] All four scenarios complete and produce telemetry.
- [ ] Frame timing is profiled on target hardware; actual measurements are
      recorded before any performance claim.
