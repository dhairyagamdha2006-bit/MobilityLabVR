using UnityEngine;

namespace MobilityLabVR
{
    public struct PlayerControlState
    {
        public float Throttle;
        public float Steering;
        public Vector2 LookDelta;
        public bool EmergencyBrakeHeld;
        public bool ResetPressed;
        public bool PausePressed;

        public static PlayerControlState Merge(PlayerControlState primary, PlayerControlState secondary)
        {
            if (Mathf.Abs(secondary.Throttle) > Mathf.Abs(primary.Throttle))
            {
                primary.Throttle = secondary.Throttle;
            }

            if (Mathf.Abs(secondary.Steering) > Mathf.Abs(primary.Steering))
            {
                primary.Steering = secondary.Steering;
            }

            if (secondary.LookDelta.sqrMagnitude > primary.LookDelta.sqrMagnitude)
            {
                primary.LookDelta = secondary.LookDelta;
            }

            primary.EmergencyBrakeHeld |= secondary.EmergencyBrakeHeld;
            primary.ResetPressed |= secondary.ResetPressed;
            primary.PausePressed |= secondary.PausePressed;
            return primary;
        }
    }

    public interface IPlayerInputSource
    {
        PlayerControlState ReadInput();
        void SetGameplayFocus(bool focused);
    }
}
