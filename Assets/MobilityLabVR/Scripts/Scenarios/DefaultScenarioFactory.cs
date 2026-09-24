using System.Collections.Generic;
using UnityEngine;

namespace MobilityLabVR
{
    public static class DefaultScenarioFactory
    {
        public static IReadOnlyList<ScenarioDefinition> CreateRuntimeDefinitions()
        {
            ScenarioDefinition[] configured = Resources.LoadAll<ScenarioDefinition>("Scenarios");
            if (configured.Length >= 4)
            {
                System.Array.Sort(configured, (left, right) => left.Kind.CompareTo(right.Kind));
                return configured;
            }

            return CreateBuiltInDefinitions();
        }

        public static IReadOnlyList<ScenarioDefinition> CreateBuiltInDefinitions()
        {
            return new[]
            {
                Create(ScenarioKind.Baseline, "Baseline",
                    "Coordinated signals, yielding vehicles, and signal-compliant pedestrians.",
                    101, 1, 90f, 14f, 0.008f, 0.85f, 1.35f),
                Create(ScenarioKind.SuddenPedestrian, "Sudden Pedestrian",
                    "A pedestrian enters the crosswalk early with enough sight distance for a controlled response.",
                    211, 3, 90f, 16f, 0.008f, 0.85f, 2.1f),
                Create(ScenarioKind.VehicleFailsToYield, "Vehicle Fails to Yield",
                    "A turning vehicle proceeds across the scooter lane instead of yielding.",
                    307, 4, 90f, 18f, 0.008f, 0.85f, 4.4f),
                Create(ScenarioKind.LowVisibility, "Low Visibility",
                    "Evening light and moderate fog reduce hazard contrast while normal traffic rules remain active.",
                    401, 3, 100f, 14f, 0.03f, 0.28f, 1.35f)
            };
        }

        private static ScenarioDefinition Create(
            ScenarioKind kind,
            string title,
            string description,
            int seedOffset,
            int difficulty,
            float timeout,
            float triggerDistance,
            float fogDensity,
            float ambientIntensity,
            float hazardSpeed)
        {
            ScenarioDefinition definition = ScriptableObject.CreateInstance<ScenarioDefinition>();
            definition.name = title.Replace(" ", string.Empty);
            definition.Initialize(kind, title, description, seedOffset, difficulty, timeout,
                triggerDistance, fogDensity, ambientIntensity, hazardSpeed);
            return definition;
        }
    }
}
