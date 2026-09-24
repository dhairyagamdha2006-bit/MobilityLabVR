using System;
using UnityEngine;

namespace MobilityLabVR
{
    [Serializable]
    public struct NearMissThresholds
    {
        public float MaximumSeparationMeters;
        public float MinimumClosingSpeedMetersPerSecond;
        public float PredictionHorizonSeconds;
        public float DuplicateCooldownSeconds;

        public static NearMissThresholds Default => new NearMissThresholds
        {
            MaximumSeparationMeters = 1.5f,
            MinimumClosingSpeedMetersPerSecond = 0.75f,
            PredictionHorizonSeconds = 1.6f,
            DuplicateCooldownSeconds = 4f
        };
    }

    public struct NearMissEvaluation
    {
        public bool IsNearMiss;
        public float CurrentSeparationMeters;
        public float PredictedMinimumSeparationMeters;
        public float ClosingSpeedMetersPerSecond;
        public float TimeToClosestApproachSeconds;
        public string Explanation;
    }

    public static class NearMissClassifier
    {
        public static NearMissEvaluation Evaluate(
            Vector3 riderPosition,
            Vector3 riderVelocity,
            float riderRadius,
            Vector3 hazardPosition,
            Vector3 hazardVelocity,
            float hazardRadius,
            NearMissThresholds thresholds)
        {
            Vector3 relativePosition = hazardPosition - riderPosition;
            relativePosition.y = 0f;
            Vector3 relativeVelocity = hazardVelocity - riderVelocity;
            relativeVelocity.y = 0f;
            float combinedRadius = Mathf.Max(0f, riderRadius) + Mathf.Max(0f, hazardRadius);
            float currentSeparation = Mathf.Max(0f, relativePosition.magnitude - combinedRadius);
            float closingSpeed = relativePosition.sqrMagnitude > 0.0001f
                ? -Vector3.Dot(relativePosition.normalized, relativeVelocity)
                : 0f;

            float timeToClosest = 0f;
            if (relativeVelocity.sqrMagnitude > 0.0001f)
            {
                timeToClosest = Mathf.Clamp(
                    -Vector3.Dot(relativePosition, relativeVelocity) / relativeVelocity.sqrMagnitude,
                    0f,
                    Mathf.Max(0f, thresholds.PredictionHorizonSeconds));
            }

            Vector3 predictedOffset = relativePosition + (relativeVelocity * timeToClosest);
            float predictedSeparation = Mathf.Max(0f, predictedOffset.magnitude - combinedRadius);
            bool converging = closingSpeed >= thresholds.MinimumClosingSpeedMetersPerSecond;
            bool closeEnough = predictedSeparation <= thresholds.MaximumSeparationMeters;
            bool futureConflict = timeToClosest > 0.01f && timeToClosest <= thresholds.PredictionHorizonSeconds;
            bool isNearMiss = converging && closeEnough && futureConflict;

            return new NearMissEvaluation
            {
                IsNearMiss = isNearMiss,
                CurrentSeparationMeters = currentSeparation,
                PredictedMinimumSeparationMeters = predictedSeparation,
                ClosingSpeedMetersPerSecond = closingSpeed,
                TimeToClosestApproachSeconds = timeToClosest,
                Explanation = string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    "predicted separation={0:F2}m; closing speed={1:F2}m/s; TCA={2:F2}s; thresholds={3:F2}m/{4:F2}m/s",
                    predictedSeparation,
                    closingSpeed,
                    timeToClosest,
                    thresholds.MaximumSeparationMeters,
                    thresholds.MinimumClosingSpeedMetersPerSecond)
            };
        }
    }

    public sealed class NearMissDuplicateSuppressor
    {
        private readonly System.Collections.Generic.Dictionary<string, float> lastRecorded =
            new System.Collections.Generic.Dictionary<string, float>();

        public bool ShouldRecord(string hazardId, float elapsedSeconds, float cooldownSeconds)
        {
            string key = string.IsNullOrEmpty(hazardId) ? "unknown" : hazardId;
            if (lastRecorded.TryGetValue(key, out float previous) && elapsedSeconds - previous < cooldownSeconds)
            {
                return false;
            }

            lastRecorded[key] = elapsedSeconds;
            return true;
        }

        public void Reset()
        {
            lastRecorded.Clear();
        }
    }
}
