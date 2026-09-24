using System;

namespace MobilityLabVR
{
    [Serializable]
    public struct ExperimentSummary
    {
        public string ParticipantId;
        public string TrialId;
        public ScenarioKind Scenario;
        public int Seed;
        public TrialCompletionStatus Status;
        public float DurationSeconds;
        public float MinimumHazardDistanceMeters;
        public int CollisionCount;
        public int NearMissCount;
        public int ExcessiveSpeedCount;
        public int SuddenBrakingCount;
        public float ReactionTimeSeconds;
        public float SafetyScore;
        public string TelemetryPath;
        public string EventLogPath;
    }

    public static class SafetyScoreCalculator
    {
        /// <summary>
        /// Demonstration-only score. This heuristic is not a validated safety,
        /// clinical, diagnostic, or transportation assessment.
        /// </summary>
        public static float Calculate(
            TrialCompletionStatus status,
            int collisions,
            int nearMisses,
            int speedEvents,
            int brakingEvents,
            float minimumDistanceMeters)
        {
            float score = 100f;
            score -= Math.Max(0, collisions) * 35f;
            score -= Math.Max(0, nearMisses) * 12f;
            score -= Math.Max(0, speedEvents) * 3f;
            score -= Math.Max(0, brakingEvents) * 2f;
            if (minimumDistanceMeters >= 0f && minimumDistanceMeters < 1.5f)
            {
                score -= (1.5f - minimumDistanceMeters) * 8f;
            }

            if (status == TrialCompletionStatus.TimedOut) score -= 10f;
            if (status == TrialCompletionStatus.Aborted) score -= 15f;
            return Math.Max(0f, Math.Min(100f, score));
        }
    }
}
