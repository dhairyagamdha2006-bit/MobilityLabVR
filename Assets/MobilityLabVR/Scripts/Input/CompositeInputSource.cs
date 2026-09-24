using UnityEngine;

namespace MobilityLabVR
{
    public sealed class CompositeInputSource : MonoBehaviour, IPlayerInputSource
    {
        private IPlayerInputSource desktop;
        private IPlayerInputSource xr;

        public void Configure(IPlayerInputSource desktopSource, IPlayerInputSource xrSource)
        {
            desktop = desktopSource;
            xr = xrSource;
        }

        public PlayerControlState ReadInput()
        {
            PlayerControlState state = desktop != null ? desktop.ReadInput() : default;
            if (xr != null)
            {
                state = PlayerControlState.Merge(state, xr.ReadInput());
            }

            return state;
        }

        public void SetGameplayFocus(bool focused)
        {
            desktop?.SetGameplayFocus(focused);
            xr?.SetGameplayFocus(focused);
        }
    }
}
