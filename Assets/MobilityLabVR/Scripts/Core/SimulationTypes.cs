using System;
using UnityEngine;

namespace MobilityLabVR
{
    public enum ScenarioKind
    {
        Baseline = 0,
        SuddenPedestrian = 1,
        VehicleFailsToYield = 2,
        LowVisibility = 3
    }

    public enum RoadAxis
    {
        NorthSouth,
        EastWest
    }

    public enum RoadSignalState
    {
        Green,
        Yellow,
        Red
    }

    public enum SignalPhase
    {
        NorthSouthGreen,
        NorthSouthYellow,
        AllRedToEastWest,
        EastWestGreen,
        EastWestYellow,
        AllRedToNorthSouth
    }

    public enum TrialCompletionStatus
    {
        NotStarted,
        InProgress,
        Completed,
        TimedOut,
        Reset,
        Aborted
    }

    public enum SafetyEventType
    {
        Collision,
        NearMiss,
        ExcessiveSpeed,
        SuddenBraking,
        ScenarioCompleted,
        TrialTimeout,
        HazardActivated
    }

    [Serializable]
    public struct SafetyEventData
    {
        public SafetyEventType Type;
        public float ElapsedSeconds;
        public Vector3 Position;
        public float DistanceMeters;
        public float ClosingSpeedMetersPerSecond;
        public float TimeToClosestApproachSeconds;
        public string OtherObjectId;
        public string Details;

        public SafetyEventData(
            SafetyEventType type,
            float elapsedSeconds,
            Vector3 position,
            float distanceMeters,
            float closingSpeedMetersPerSecond,
            float timeToClosestApproachSeconds,
            string otherObjectId,
            string details)
        {
            Type = type;
            ElapsedSeconds = elapsedSeconds;
            Position = position;
            DistanceMeters = distanceMeters;
            ClosingSpeedMetersPerSecond = closingSpeedMetersPerSecond;
            TimeToClosestApproachSeconds = timeToClosestApproachSeconds;
            OtherObjectId = otherObjectId ?? string.Empty;
            Details = details ?? string.Empty;
        }
    }

    public interface ITrialResettable
    {
        void ResetForTrial(int seed);
    }

    public interface IHazardTarget
    {
        string HazardId { get; }
        Transform HazardTransform { get; }
        Vector3 Velocity { get; }
        bool IsActiveHazard { get; }
        float CollisionRadius { get; }
    }
}
