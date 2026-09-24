using System;

namespace MobilityLabVR
{
    [Serializable]
    public struct TrafficSignalTiming
    {
        public float GreenSeconds;
        public float YellowSeconds;
        public float AllRedSeconds;

        public static TrafficSignalTiming Default => new TrafficSignalTiming
        {
            GreenSeconds = 12f,
            YellowSeconds = 3f,
            AllRedSeconds = 1.25f
        };

        public TrafficSignalTiming Validated()
        {
            return new TrafficSignalTiming
            {
                GreenSeconds = Math.Max(1f, GreenSeconds),
                YellowSeconds = Math.Max(0.5f, YellowSeconds),
                AllRedSeconds = Math.Max(0.25f, AllRedSeconds)
            };
        }
    }

    public sealed class TrafficSignalStateMachine
    {
        private TrafficSignalTiming timing;

        public SignalPhase Phase { get; private set; }
        public float TimeInPhase { get; private set; }

        public event Action<SignalPhase> PhaseChanged;

        public TrafficSignalStateMachine(TrafficSignalTiming configuredTiming)
        {
            timing = configuredTiming.Validated();
            Reset();
        }

        public void SetTiming(TrafficSignalTiming configuredTiming)
        {
            timing = configuredTiming.Validated();
        }

        public void Reset(SignalPhase initialPhase = SignalPhase.NorthSouthGreen)
        {
            Phase = initialPhase;
            TimeInPhase = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            TimeInPhase += deltaTime;
            int transitionGuard = 0;
            while (TimeInPhase >= CurrentDuration() && transitionGuard++ < 8)
            {
                TimeInPhase -= CurrentDuration();
                Phase = NextPhase(Phase);
                PhaseChanged?.Invoke(Phase);
            }
        }

        public RoadSignalState GetVehicleState(RoadAxis axis)
        {
            switch (Phase)
            {
                case SignalPhase.NorthSouthGreen:
                    return axis == RoadAxis.NorthSouth ? RoadSignalState.Green : RoadSignalState.Red;
                case SignalPhase.NorthSouthYellow:
                    return axis == RoadAxis.NorthSouth ? RoadSignalState.Yellow : RoadSignalState.Red;
                case SignalPhase.EastWestGreen:
                    return axis == RoadAxis.EastWest ? RoadSignalState.Green : RoadSignalState.Red;
                case SignalPhase.EastWestYellow:
                    return axis == RoadAxis.EastWest ? RoadSignalState.Yellow : RoadSignalState.Red;
                default:
                    return RoadSignalState.Red;
            }
        }

        public bool PedestriansMayCross(RoadAxis roadBeingCrossed)
        {
            if (roadBeingCrossed == RoadAxis.NorthSouth)
            {
                return Phase == SignalPhase.EastWestGreen;
            }

            return Phase == SignalPhase.NorthSouthGreen;
        }

        private float CurrentDuration()
        {
            switch (Phase)
            {
                case SignalPhase.NorthSouthGreen:
                case SignalPhase.EastWestGreen:
                    return timing.GreenSeconds;
                case SignalPhase.NorthSouthYellow:
                case SignalPhase.EastWestYellow:
                    return timing.YellowSeconds;
                default:
                    return timing.AllRedSeconds;
            }
        }

        private static SignalPhase NextPhase(SignalPhase phase)
        {
            return (SignalPhase)(((int)phase + 1) % 6);
        }
    }
}
