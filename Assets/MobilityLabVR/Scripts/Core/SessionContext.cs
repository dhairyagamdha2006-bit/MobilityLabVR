using System;
using System.Linq;

namespace MobilityLabVR
{
    /// <summary>
    /// Carries non-sensitive choices between the menu and simulation scenes.
    /// It deliberately stores no participant names or contact information.
    /// </summary>
    public static class SessionContext
    {
        private const int MaxIdentifierLength = 32;

        public static string ParticipantId { get; private set; } = "anonymous";
        public static ScenarioKind SelectedScenario { get; set; } = ScenarioKind.Baseline;
        public static int Seed { get; set; } = 2026;
        public static float MasterVolume { get; set; } = 0.8f;
        public static float LookSensitivity { get; set; } = 0.12f;
        public static bool PreferXr { get; set; }

        public static void SetParticipantId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ParticipantId = "anonymous";
                return;
            }

            string sanitized = new string(value.Trim()
                .Where(character => char.IsLetterOrDigit(character) || character == '-' || character == '_')
                .Take(MaxIdentifierLength)
                .ToArray());
            ParticipantId = string.IsNullOrEmpty(sanitized) ? "anonymous" : sanitized;
        }

        public static int StableSeed(string participantId, ScenarioKind scenario, int repetition)
        {
            unchecked
            {
                int hash = 17;
                string source = participantId ?? string.Empty;
                for (int index = 0; index < source.Length; index++)
                {
                    hash = (hash * 31) + source[index];
                }

                hash = (hash * 31) + (int)scenario;
                hash = (hash * 31) + repetition;
                return hash == int.MinValue ? 0 : Math.Abs(hash);
            }
        }
    }
}
