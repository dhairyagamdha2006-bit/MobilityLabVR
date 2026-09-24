using NUnit.Framework;

namespace MobilityLabVR.Tests
{
    public sealed class TelemetryAndSummaryTests
    {
        [TestCase("plain", "plain")]
        [TestCase("contains,comma", "\"contains,comma\"")]
        [TestCase("say \"safe\"", "\"say \"\"safe\"\"\"")]
        [TestCase("two\nlines", "\"two\nlines\"")]
        public void CsvEscape_FollowsRfc4180Rules(string input, string expected)
        {
            Assert.That(CsvUtility.Escape(input), Is.EqualTo(expected));
        }

        [Test]
        public void SafetyScore_IsBoundedAndPenalizesSafetyEvents()
        {
            float clean = SafetyScoreCalculator.Calculate(TrialCompletionStatus.Completed, 0, 0, 0, 0, 3f);
            float hazardous = SafetyScoreCalculator.Calculate(TrialCompletionStatus.Completed, 2, 3, 4, 2, 0.2f);
            float extreme = SafetyScoreCalculator.Calculate(TrialCompletionStatus.TimedOut, 100, 100, 100, 100, 0f);

            Assert.That(clean, Is.EqualTo(100f));
            Assert.That(hazardous, Is.LessThan(clean));
            Assert.That(extreme, Is.EqualTo(0f));
        }

        [Test]
        public void SummaryBuilder_WithNoMonitor_ProducesExplicitMissingMeasurements()
        {
            ExperimentSummary summary = ExperimentSummaryBuilder.Build(
                "P001", "trial-1", ScenarioKind.Baseline, 42, TrialCompletionStatus.Completed,
                24.5f, null, "/tmp/trial.csv", "/tmp/events.jsonl");

            Assert.That(summary.ParticipantId, Is.EqualTo("P001"));
            Assert.That(summary.TrialId, Is.EqualTo("trial-1"));
            Assert.That(summary.CollisionCount, Is.Zero);
            Assert.That(summary.MinimumHazardDistanceMeters, Is.EqualTo(-1f));
            Assert.That(summary.ReactionTimeSeconds, Is.EqualTo(-1f));
            Assert.That(summary.SafetyScore, Is.EqualTo(100f));
        }
    }
}
