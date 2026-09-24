using UnityEngine;
using UnityEngine.InputSystem;

namespace MobilityLabVR
{
    /// <summary>
    /// Keyboard and mouse adapter. Movement does not depend on this concrete
    /// implementation, allowing XR or accessibility input to be substituted.
    /// </summary>
    public sealed class DesktopInputSource : MonoBehaviour, IPlayerInputSource
    {
        [SerializeField, Range(0.01f, 1f), Tooltip("Mouse look scale in degrees per input unit.")]
        private float lookSensitivity = 0.12f;

        private bool gameplayFocused = true;

        public PlayerControlState ReadInput()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (keyboard == null)
            {
                return default;
            }

            float throttle = 0f;
            float steering = 0f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) throttle += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) throttle -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) steering += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) steering -= 1f;

            return new PlayerControlState
            {
                Throttle = Mathf.Clamp(throttle, -1f, 1f),
                Steering = Mathf.Clamp(steering, -1f, 1f),
                LookDelta = gameplayFocused && mouse != null
                    ? mouse.delta.ReadValue() * (lookSensitivity * SessionContext.LookSensitivity / 0.12f)
                    : Vector2.zero,
                EmergencyBrakeHeld = keyboard.spaceKey.isPressed,
                ResetPressed = keyboard.rKey.wasPressedThisFrame,
                PausePressed = keyboard.escapeKey.wasPressedThisFrame
            };
        }

        public void SetGameplayFocus(bool focused)
        {
            gameplayFocused = focused;
            Cursor.lockState = focused ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !focused;
        }
    }
}
