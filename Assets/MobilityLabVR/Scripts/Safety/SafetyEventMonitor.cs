using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobilityLabVR
{
    public sealed class SafetyEventMonitor : MonoBehaviour, ITrialResettable
    {
        [SerializeField] private NearMissThresholds nearMissThresholds = default;
        [SerializeField, Range(2f, 20f)] private float checksPerSecond = 10f;
        [SerializeField, Min(1f)] private float excessiveSpeedThreshold = 7.5f;
        [SerializeField, Min(1f)] private float suddenBrakingThreshold = 6f;

        private readonly List<IHazardTarget> hazards = new List<IHazardTarget>(16);
        private readonly Dictionary<string, float> recentCollisions = new Dictionary<string, float>();
        private readonly NearMissDuplicateSuppressor duplicateSuppressor = new NearMissDuplicateSuppressor();
        private ScooterController scooter;
        private ScenarioManager scenarios;
        private SimulationClock clock;
        private float nextCheckTime;
        private float lastSpeedEventTime = -10f;
        private float lastBrakeEventTime = -10f;
        private bool reactionRecorded;
        private bool wasBraking;

        public event Action<SafetyEventData> EventRecorded;

        public int CollisionCount { get; private set; }
        public int NearMissCount { get; private set; }
        public int ExcessiveSpeedCount { get; private set; }
        public int SuddenBrakingCount { get; private set; }
        public float MinimumHazardDistanceMeters { get; private set; } = float.PositiveInfinity;
        public float ReactionTimeSeconds { get; private set; } = -1f;
        public float DistanceToActiveHazardMeters { get; private set; } = -1f;
        public float ElapsedSeconds => clock != null ? clock.ElapsedSeconds : 0f;

        public void Configure(ScooterController rider, ScenarioManager scenarioManager, SimulationClock simulationClock)
        {
            scooter = rider;
            scenarios = scenarioManager;
            clock = simulationClock;
            if (nearMissThresholds.MaximumSeparationMeters <= 0f) nearMissThresholds = NearMissThresholds.Default;
        }

        public void RegisterHazard(IHazardTarget hazard)
        {
            if (hazard != null && !hazards.Contains(hazard)) hazards.Add(hazard);
        }

        private void Update()
        {
            if (scooter == null || clock == null || !clock.IsRunning)
            {
                return;
            }

            CaptureReactionTime();
            if (clock.ElapsedSeconds < nextCheckTime)
            {
                return;
            }

            nextCheckTime = clock.ElapsedSeconds + (1f / Mathf.Max(1f, checksPerSecond));
            EvaluateHazards();
            EvaluateRiderBehavior();
        }

        public void RegisterCollision(string otherObjectId, Vector3 position, float relativeSpeed)
        {
            string id = string.IsNullOrWhiteSpace(otherObjectId) ? "unknown" : otherObjectId;
            if (recentCollisions.TryGetValue(id, out float previousTime) && ElapsedSeconds - previousTime < 1f)
            {
                return;
            }

            recentCollisions[id] = ElapsedSeconds;
            CollisionCount++;
            Record(new SafetyEventData(SafetyEventType.Collision, ElapsedSeconds, position, 0f,
                relativeSpeed, 0f, id, "Physical collider contact."));
        }

        public void RegisterHazardActivation(string hazardId)
        {
            Record(new SafetyEventData(SafetyEventType.HazardActivated, ElapsedSeconds,
                scooter != null ? scooter.transform.position : Vector3.zero, DistanceToActiveHazardMeters,
                0f, 0f, hazardId, "Scenario trigger activated the designated hazard."));
        }

        public void RegisterCompletion(bool timedOut)
        {
            Record(new SafetyEventData(
                timedOut ? SafetyEventType.TrialTimeout : SafetyEventType.ScenarioCompleted,
                ElapsedSeconds,
                scooter != null ? scooter.transform.position : Vector3.zero,
                DistanceToActiveHazardMeters,
                0f,
                0f,
                string.Empty,
                timedOut ? "Trial exceeded configured duration." : "Destination trigger reached."));
        }

        public void ResetForTrial(int seed)
        {
            CollisionCount = 0;
            NearMissCount = 0;
            ExcessiveSpeedCount = 0;
            SuddenBrakingCount = 0;
            MinimumHazardDistanceMeters = float.PositiveInfinity;
            DistanceToActiveHazardMeters = -1f;
            ReactionTimeSeconds = -1f;
            reactionRecorded = false;
            wasBraking = false;
            nextCheckTime = 0f;
            lastSpeedEventTime = -10f;
            lastBrakeEventTime = -10f;
            recentCollisions.Clear();
            duplicateSuppressor.Reset();
        }

        private void EvaluateHazards()
        {
            float nearest = float.PositiveInfinity;
            IHazardTarget designated = scenarios != null ? scenarios.ActiveHazard : null;

            for (int index = hazards.Count - 1; index >= 0; index--)
            {
                IHazardTarget hazard = hazards[index];
                if (hazard == null || hazard.HazardTransform == null)
                {
                    hazards.RemoveAt(index);
                    continue;
                }

                if (!hazard.IsActiveHazard) continue;
                float surfaceDistance = Mathf.Max(0f,
                    Vector3.Distance(scooter.transform.position, hazard.HazardTransform.position) - hazard.CollisionRadius - 0.55f);
                nearest = Mathf.Min(nearest, surfaceDistance);

                NearMissEvaluation evaluation = NearMissClassifier.Evaluate(
                    scooter.transform.position,
                    scooter.Velocity,
                    0.55f,
                    hazard.HazardTransform.position,
                    hazard.Velocity,
                    hazard.CollisionRadius,
                    nearMissThresholds);

                if (evaluation.IsNearMiss && duplicateSuppressor.ShouldRecord(
                        hazard.HazardId, ElapsedSeconds, nearMissThresholds.DuplicateCooldownSeconds))
                {
                    NearMissCount++;
                    Record(new SafetyEventData(
                        SafetyEventType.NearMiss,
                        ElapsedSeconds,
                        scooter.transform.position,
                        evaluation.PredictedMinimumSeparationMeters,
                        evaluation.ClosingSpeedMetersPerSecond,
                        evaluation.TimeToClosestApproachSeconds,
                        hazard.HazardId,
                        evaluation.Explanation));
                }

                if (designated == hazard) DistanceToActiveHazardMeters = surfaceDistance;
            }

            if (float.IsPositiveInfinity(nearest))
            {
                DistanceToActiveHazardMeters = designated == null ? -1f : DistanceToActiveHazardMeters;
            }
            else
            {
                MinimumHazardDistanceMeters = Mathf.Min(MinimumHazardDistanceMeters, nearest);
                if (designated == null) DistanceToActiveHazardMeters = nearest;
            }
        }

        private void EvaluateRiderBehavior()
        {
            if (scooter.SpeedMetersPerSecond > excessiveSpeedThreshold && ElapsedSeconds - lastSpeedEventTime >= 3f)
            {
                lastSpeedEventTime = ElapsedSeconds;
                ExcessiveSpeedCount++;
                Record(new SafetyEventData(SafetyEventType.ExcessiveSpeed, ElapsedSeconds, scooter.transform.position,
                    DistanceToActiveHazardMeters, 0f, 0f, string.Empty,
                    $"Speed {scooter.SpeedMetersPerSecond:F2}m/s exceeded {excessiveSpeedThreshold:F2}m/s."));
            }

            if (scooter.LongitudinalAcceleration <= -suddenBrakingThreshold && scooter.SpeedMetersPerSecond > 1.5f &&
                ElapsedSeconds - lastBrakeEventTime >= 2f)
            {
                lastBrakeEventTime = ElapsedSeconds;
                SuddenBrakingCount++;
                Record(new SafetyEventData(SafetyEventType.SuddenBraking, ElapsedSeconds, scooter.transform.position,
                    DistanceToActiveHazardMeters, 0f, 0f, string.Empty,
                    $"Longitudinal deceleration {scooter.LongitudinalAcceleration:F2}m/s²."));
            }
        }

        private void CaptureReactionTime()
        {
            bool braking = scooter.IsBraking;
            if (reactionRecorded || scenarios == null || !scenarios.HazardActivated || !braking || wasBraking)
            {
                wasBraking = braking;
                return;
            }

            ReactionTimeSeconds = Mathf.Max(0f, ElapsedSeconds - scenarios.HazardActivationElapsedSeconds);
            reactionRecorded = true;
            wasBraking = braking;
        }

        private void Record(SafetyEventData data)
        {
            EventRecorded?.Invoke(data);
        }
    }
}
