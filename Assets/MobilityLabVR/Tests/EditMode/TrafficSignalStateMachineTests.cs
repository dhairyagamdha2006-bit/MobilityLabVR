using NUnit.Framework;

namespace MobilityLabVR.Tests
{
    public sealed class TrafficSignalStateMachineTests
    {
        [Test]
        public void Tick_TransitionsThroughCoordinatedPhases()
        {
            TrafficSignalTiming timing = new TrafficSignalTiming
            {
                GreenSeconds = 10f,
                YellowSeconds = 3f,
                AllRedSeconds = 1f
            };
            TrafficSignalStateMachine machine = new TrafficSignalStateMachine(timing);

            machine.Tick(10f);
            Assert.That(machine.Phase, Is.EqualTo(SignalPhase.NorthSouthYellow));
            Assert.That(machine.GetVehicleState(RoadAxis.NorthSouth), Is.EqualTo(RoadSignalState.Yellow));
            Assert.That(machine.GetVehicleState(RoadAxis.EastWest), Is.EqualTo(RoadSignalState.Red));

            machine.Tick(3f);
            Assert.That(machine.Phase, Is.EqualTo(SignalPhase.AllRedToEastWest));
            Assert.That(machine.GetVehicleState(RoadAxis.NorthSouth), Is.EqualTo(RoadSignalState.Red));

            machine.Tick(1f);
            Assert.That(machine.Phase, Is.EqualTo(SignalPhase.EastWestGreen));
            Assert.That(machine.PedestriansMayCross(RoadAxis.NorthSouth), Is.True);
        }

        [Test]
        public void Tick_LargeDelta_PreservesRemainderAndAdvancesMultiplePhases()
        {
            TrafficSignalStateMachine machine = new TrafficSignalStateMachine(new TrafficSignalTiming
            {
                GreenSeconds = 4f,
                YellowSeconds = 2f,
                AllRedSeconds = 1f
            });

            machine.Tick(8.5f);

            Assert.That(machine.Phase, Is.EqualTo(SignalPhase.EastWestGreen));
            Assert.That(machine.TimeInPhase, Is.EqualTo(1.5f).Within(0.001f));
        }
    }
}
