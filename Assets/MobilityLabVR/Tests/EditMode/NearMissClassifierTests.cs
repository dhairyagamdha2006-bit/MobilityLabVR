using NUnit.Framework;
using UnityEngine;

namespace MobilityLabVR.Tests
{
    public sealed class NearMissClassifierTests
    {
        [Test]
        public void Evaluate_ConvergingClosePaths_ClassifiesNearMissWithEvidence()
        {
            NearMissEvaluation result = NearMissClassifier.Evaluate(
                Vector3.zero,
                new Vector3(0f, 0f, 5f),
                0.5f,
                new Vector3(0.8f, 0f, 5f),
                new Vector3(0f, 0f, -1f),
                0.4f,
                NearMissThresholds.Default);

            Assert.That(result.IsNearMiss, Is.True);
            Assert.That(result.ClosingSpeedMetersPerSecond, Is.GreaterThan(0.75f));
            Assert.That(result.TimeToClosestApproachSeconds, Is.GreaterThan(0f));
            Assert.That(result.Explanation, Does.Contain("predicted separation"));
        }

        [Test]
        public void Evaluate_DivergingPaths_DoesNotClassifyNearMiss()
        {
            NearMissEvaluation result = NearMissClassifier.Evaluate(
                Vector3.zero,
                new Vector3(0f, 0f, -2f),
                0.5f,
                new Vector3(0.5f, 0f, 3f),
                new Vector3(0f, 0f, 3f),
                0.4f,
                NearMissThresholds.Default);

            Assert.That(result.IsNearMiss, Is.False);
        }

        [Test]
        public void DuplicateSuppressor_BlocksRepeatedHazardDuringCooldown()
        {
            NearMissDuplicateSuppressor suppressor = new NearMissDuplicateSuppressor();

            Assert.That(suppressor.ShouldRecord("ped-1", 1f, 4f), Is.True);
            Assert.That(suppressor.ShouldRecord("ped-1", 2.5f, 4f), Is.False);
            Assert.That(suppressor.ShouldRecord("ped-2", 2.5f, 4f), Is.True);
            Assert.That(suppressor.ShouldRecord("ped-1", 5.1f, 4f), Is.True);
        }
    }
}
