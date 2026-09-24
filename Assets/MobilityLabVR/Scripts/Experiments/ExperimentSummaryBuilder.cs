namespace MobilityLabVR
{
    public static class ExperimentSummaryBuilder
    {
        public static ExperimentSummary Build(
            string participantId,
            string trialId,
            ScenarioKind scenario,
            int seed,
            TrialCompletionStatus status,
            float durationSeconds,
            SafetyEventMonitor safety,
            string telemetryPath,
            string eventLogPath)
        {
            float minimumDistance = safety == null || float.IsPositiveInfinity(safety.MinimumHazardDistanceMeters)
                ? -1f
                : safety.MinimumHazardDistanceMeters;
            int collisions = safety != null ? safety.CollisionCount : 0;
            int nearMisses = safety != null ? safety.NearMissCount : 0;
            int speedEvents = safety != null ? safety.ExcessiveSpeedCount : 0;
            int brakingEvents = safety != null ? safety.SuddenBrakingCount : 0;

            return new ExperimentSummary
            {
                ParticipantId = participantId,
                TrialId = trialId,
                Scenario = scenario,
                Seed = seed,
                Status = status,
                DurationSeconds = durationSeconds,
                MinimumHazardDistanceMeters = minimumDistance,
                CollisionCount = collisions,
                NearMissCount = nearMisses,
                ExcessiveSpeedCount = speedEvents,
                SuddenBrakingCount = brakingEvents,
                ReactionTimeSeconds = safety != null ? safety.ReactionTimeSeconds : -1f,
                SafetyScore = SafetyScoreCalculator.Calculate(status, collisions, nearMisses, speedEvents, brakingEvents, minimumDistance),
                TelemetryPath = telemetryPath ?? string.Empty,
                EventLogPath = eventLogPath ?? string.Empty
            };
        }
    }
}
