using System;
using UnityEngine;

namespace MobilityLabVR
{
    [CreateAssetMenu(menuName = "MobilityLab VR/Scenario Definition", fileName = "ScenarioDefinition")]
    public sealed class ScenarioDefinition : ScriptableObject
    {
        [SerializeField] private ScenarioKind kind;
        [SerializeField] private string displayName = "Scenario";
        [SerializeField, TextArea(2, 5)] private string description = string.Empty;
        [SerializeField, Min(1)] private int seedOffset;
        [SerializeField, Range(1, 5)] private int difficulty = 1;
        [SerializeField, Min(10f)] private float trialTimeoutSeconds = 90f;
        [SerializeField, Min(5f), Tooltip("South-to-north Z coordinate where the scenario hazard activates.")]
        private float triggerDistanceFromIntersection = 14f;
        [SerializeField, Range(0.001f, 0.1f)] private float fogDensity = 0.024f;
        [SerializeField, Range(0.05f, 1f)] private float ambientIntensity = 0.35f;
        [SerializeField, Min(0.5f)] private float hazardSpeedMetersPerSecond = 2f;

        public ScenarioKind Kind => kind;
        public string DisplayName => displayName;
        public string Description => description;
        public int SeedOffset => seedOffset;
        public int Difficulty => difficulty;
        public float TrialTimeoutSeconds => trialTimeoutSeconds;
        public float TriggerDistanceFromIntersection => triggerDistanceFromIntersection;
        public float FogDensity => fogDensity;
        public float AmbientIntensity => ambientIntensity;
        public float HazardSpeedMetersPerSecond => hazardSpeedMetersPerSecond;

        public void Initialize(
            ScenarioKind scenarioKind,
            string scenarioName,
            string scenarioDescription,
            int configuredSeedOffset,
            int configuredDifficulty,
            float timeout,
            float triggerDistance,
            float configuredFogDensity,
            float configuredAmbientIntensity,
            float hazardSpeed)
        {
            kind = scenarioKind;
            displayName = scenarioName;
            description = scenarioDescription;
            seedOffset = configuredSeedOffset;
            difficulty = Mathf.Clamp(configuredDifficulty, 1, 5);
            trialTimeoutSeconds = Mathf.Max(10f, timeout);
            triggerDistanceFromIntersection = Mathf.Max(5f, triggerDistance);
            fogDensity = Mathf.Clamp(configuredFogDensity, 0.001f, 0.1f);
            ambientIntensity = Mathf.Clamp01(configuredAmbientIntensity);
            hazardSpeedMetersPerSecond = Mathf.Max(0.5f, hazardSpeed);
        }
    }
}
