using System;
using UnityEngine;

namespace MobilityLabVR
{
    public sealed class TrafficSignalController : MonoBehaviour, ITrialResettable
    {
        [SerializeField, Tooltip("Durations used by both coordinated road axes.")]
        private TrafficSignalTiming timing = default;

        private TrafficSignalStateMachine machine;

        public event Action<SignalPhase> PhaseChanged;

        public SignalPhase Phase => Machine.Phase;
        public RoadSignalState ScooterApproachState => GetVehicleState(RoadAxis.NorthSouth);

        private TrafficSignalStateMachine Machine
        {
            get
            {
                if (machine == null)
                {
                    if (timing.GreenSeconds <= 0f) timing = TrafficSignalTiming.Default;
                    machine = new TrafficSignalStateMachine(timing);
                    machine.PhaseChanged += phase => PhaseChanged?.Invoke(phase);
                }

                return machine;
            }
        }

        public void Configure(TrafficSignalTiming configuredTiming)
        {
            timing = configuredTiming.Validated();
            Machine.SetTiming(timing);
        }

        private void Update()
        {
            Machine.Tick(Time.deltaTime);
        }

        public RoadSignalState GetVehicleState(RoadAxis axis)
        {
            return Machine.GetVehicleState(axis);
        }

        public bool PedestriansMayCross(RoadAxis roadBeingCrossed)
        {
            return Machine.PedestriansMayCross(roadBeingCrossed);
        }

        public void ResetForTrial(int seed)
        {
            Machine.Reset(SignalPhase.NorthSouthGreen);
            PhaseChanged?.Invoke(Machine.Phase);
        }
    }
}
