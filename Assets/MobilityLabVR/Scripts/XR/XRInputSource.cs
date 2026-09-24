using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace MobilityLabVR
{
    /// <summary>
    /// Lightweight OpenXR-compatible input adapter using Unity XR common usages.
    /// It has no dependency on XR Interaction Toolkit locomotion and remains
    /// inactive when no controller is connected.
    /// </summary>
    public sealed class XRInputSource : MonoBehaviour, IPlayerInputSource
    {
        private readonly List<InputDevice> devices = new List<InputDevice>(4);
        private InputDevice leftController;
        private InputDevice rightController;
        private bool previousReset;
        private bool previousPause;

        private void OnEnable()
        {
            InputDevices.deviceConnected += OnDeviceChanged;
            InputDevices.deviceDisconnected += OnDeviceChanged;
            RefreshDevices();
        }

        private void OnDisable()
        {
            InputDevices.deviceConnected -= OnDeviceChanged;
            InputDevices.deviceDisconnected -= OnDeviceChanged;
        }

        public PlayerControlState ReadInput()
        {
            if (!leftController.isValid || !rightController.isValid)
            {
                RefreshDevices();
            }

            leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 moveAxis);
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 lookAxis);
            rightController.TryGetFeatureValue(CommonUsages.triggerButton, out bool brake);
            leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool resetHeld);
            leftController.TryGetFeatureValue(CommonUsages.menuButton, out bool pauseHeld);

            bool resetPressed = resetHeld && !previousReset;
            bool pausePressed = pauseHeld && !previousPause;
            previousReset = resetHeld;
            previousPause = pauseHeld;

            return new PlayerControlState
            {
                Throttle = moveAxis.y,
                Steering = moveAxis.x,
                LookDelta = lookAxis * 1.75f,
                EmergencyBrakeHeld = brake,
                ResetPressed = resetPressed,
                PausePressed = pausePressed
            };
        }

        public void SetGameplayFocus(bool focused)
        {
            enabled = focused;
        }

        private void OnDeviceChanged(InputDevice device)
        {
            RefreshDevices();
        }

        private void RefreshDevices()
        {
            devices.Clear();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Left,
                devices);
            leftController = devices.Count > 0 ? devices[0] : default;

            devices.Clear();
            InputDevices.GetDevicesWithCharacteristics(
                InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right,
                devices);
            rightController = devices.Count > 0 ? devices[0] : default;
        }
    }
}
