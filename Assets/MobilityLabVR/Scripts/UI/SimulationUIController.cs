using UnityEngine;
using UnityEngine.UI;

namespace MobilityLabVR
{
    public sealed class SimulationUIController : MonoBehaviour
    {
        private ExperimentManager experiment;
        private ScooterController scooter;
        private SafetyEventMonitor safety;
        private ScenarioManager scenarios;
        private TrafficSignalController signals;
        private GameObject hud;
        private GameObject pausePanel;
        private GameObject resultsPanel;
        private Text speedText;
        private Text scenarioText;
        private Text timeText;
        private Text collisionText;
        private Text nearMissText;
        private Text signalText;
        private Text resultsBody;
        private float nextRefresh;

        public void Configure(
            ExperimentManager experimentManager,
            ScooterController rider,
            SafetyEventMonitor safetyMonitor,
            ScenarioManager scenarioManager,
            TrafficSignalController signalController)
        {
            experiment = experimentManager;
            scooter = rider;
            safety = safetyMonitor;
            scenarios = scenarioManager;
            signals = signalController;
            BuildInterface();
        }

        private void Update()
        {
            if (hud == null || Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.1f;
            speedText.text = $"SPEED\n{scooter.SpeedMetersPerSecond * 3.6f:F1} km/h";
            scenarioText.text = $"SCENARIO\n{(scenarios.Current != null ? scenarios.Current.DisplayName : "—")}";
            timeText.text = $"TRIAL TIME\n{experiment.ElapsedSeconds:F1} s";
            collisionText.text = $"COLLISIONS\n{safety.CollisionCount}";
            nearMissText.text = $"NEAR MISSES\n{safety.NearMissCount}";
            RoadSignalState state = signals.ScooterApproachState;
            signalText.text = $"SIGNAL\n{state.ToString().ToUpperInvariant()}";
            signalText.color = state == RoadSignalState.Green
                ? new Color(0.22f, 1f, 0.45f)
                : state == RoadSignalState.Yellow ? MobilityLabPalette.Amber : MobilityLabPalette.Coral;
        }

        public void ShowExperiment()
        {
            hud.SetActive(true);
            pausePanel.SetActive(false);
            resultsPanel.SetActive(false);
        }

        public void SetPauseVisible(bool visible)
        {
            pausePanel.SetActive(visible);
        }

        public void ShowResults(ExperimentSummary summary)
        {
            hud.SetActive(false);
            pausePanel.SetActive(false);
            resultsPanel.SetActive(true);
            string distance = summary.MinimumHazardDistanceMeters >= 0f ? $"{summary.MinimumHazardDistanceMeters:F2} m" : "not observed";
            string reaction = summary.ReactionTimeSeconds >= 0f ? $"{summary.ReactionTimeSeconds:F2} s" : "not measurable";
            resultsBody.text =
                $"SCENARIO                 {summary.Scenario}\n" +
                $"STATUS                   {summary.Status}\n" +
                $"COMPLETION TIME          {summary.DurationSeconds:F2} s\n" +
                $"MINIMUM HAZARD DISTANCE  {distance}\n" +
                $"COLLISIONS               {summary.CollisionCount}\n" +
                $"NEAR MISSES              {summary.NearMissCount}\n" +
                $"REACTION TIME            {reaction}\n" +
                $"DEMO SAFETY SCORE        {summary.SafetyScore:F0} / 100\n\n" +
                $"CSV OUTPUT\n{summary.TelemetryPath}";
        }

        private void BuildInterface()
        {
            Canvas canvas = RuntimeUIFactory.CreateCanvas("Experiment UI");
            hud = new GameObject("HUD", typeof(RectTransform));
            hud.transform.SetParent(canvas.transform, false);
            RuntimeUIFactory.SetRect(hud.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            GameObject strip = RuntimeUIFactory.CreatePanel(hud.transform, "HUD Strip", new Color(0.025f, 0.06f, 0.085f, 0.9f),
                new Vector2(0.03f, 0.88f), new Vector2(0.97f, 0.97f), Vector2.zero, Vector2.zero);
            speedText = HudCell(strip.transform, "Speed", 0f, 0.15f);
            scenarioText = HudCell(strip.transform, "Scenario", 0.15f, 0.38f);
            timeText = HudCell(strip.transform, "Time", 0.38f, 0.53f);
            collisionText = HudCell(strip.transform, "Collisions", 0.53f, 0.68f);
            nearMissText = HudCell(strip.transform, "NearMiss", 0.68f, 0.83f);
            signalText = HudCell(strip.transform, "Signal", 0.83f, 1f);
            RuntimeUIFactory.CreateText(hud.transform, "Hint", "WASD / arrows to ride   •   SPACE emergency brake   •   R reset   •   ESC pause", 17,
                Color.white, TextAnchor.MiddleCenter, new Vector2(0.20f, 0.025f), new Vector2(0.80f, 0.07f), Vector2.zero, Vector2.zero);

            pausePanel = RuntimeUIFactory.CreatePanel(canvas.transform, "Pause Panel", new Color(0.02f, 0.05f, 0.075f, 0.98f),
                new Vector2(0.35f, 0.25f), new Vector2(0.65f, 0.75f), Vector2.zero, Vector2.zero);
            RuntimeUIFactory.CreateText(pausePanel.transform, "Title", "EXPERIMENT PAUSED", 36, MobilityLabPalette.Teal,
                TextAnchor.MiddleCenter, new Vector2(0.05f, 0.75f), new Vector2(0.95f, 0.92f), Vector2.zero, Vector2.zero);
            Button resume = MenuButton(pausePanel.transform, "Resume", "RESUME", 0.54f); resume.onClick.AddListener(experiment.Resume);
            Button restart = MenuButton(pausePanel.transform, "Restart", "RESET TRIAL", 0.37f); restart.onClick.AddListener(experiment.ResetCurrentTrial);
            Button menu = MenuButton(pausePanel.transform, "Menu", "RETURN TO MENU", 0.20f); menu.onClick.AddListener(experiment.ReturnToMenu);

            resultsPanel = RuntimeUIFactory.CreatePanel(canvas.transform, "Results Panel", new Color(0.02f, 0.05f, 0.075f, 0.99f),
                new Vector2(0.19f, 0.08f), new Vector2(0.81f, 0.92f), Vector2.zero, Vector2.zero);
            RuntimeUIFactory.CreateText(resultsPanel.transform, "Title", "TRIAL RESULTS", 42, MobilityLabPalette.Teal,
                TextAnchor.MiddleLeft, new Vector2(0.07f, 0.84f), new Vector2(0.93f, 0.95f), Vector2.zero, Vector2.zero);
            resultsBody = RuntimeUIFactory.CreateText(resultsPanel.transform, "Results", string.Empty, 22, MobilityLabPalette.OffWhite,
                TextAnchor.UpperLeft, new Vector2(0.07f, 0.29f), new Vector2(0.93f, 0.83f), Vector2.zero, Vector2.zero);
            RuntimeUIFactory.CreateText(resultsPanel.transform, "Disclaimer",
                "The safety score is a demonstration heuristic—not a scientifically validated medical or transportation-safety assessment.",
                17, MobilityLabPalette.Amber, TextAnchor.MiddleLeft,
                new Vector2(0.07f, 0.20f), new Vector2(0.93f, 0.28f), Vector2.zero, Vector2.zero);
            Button repeat = RuntimeUIFactory.CreateButton(resultsPanel.transform, "Repeat", "REPEAT TRIAL", MobilityLabPalette.Teal,
                MobilityLabPalette.DeepNavy, new Vector2(0.07f, 0.08f), new Vector2(0.44f, 0.16f), Vector2.zero, Vector2.zero);
            repeat.onClick.AddListener(experiment.RepeatTrial);
            Button back = RuntimeUIFactory.CreateButton(resultsPanel.transform, "Back", "MAIN MENU", new Color(1f, 1f, 1f, 0.12f),
                MobilityLabPalette.OffWhite, new Vector2(0.56f, 0.08f), new Vector2(0.93f, 0.16f), Vector2.zero, Vector2.zero);
            back.onClick.AddListener(experiment.ReturnToMenu);
            pausePanel.SetActive(false);
            resultsPanel.SetActive(false);
        }

        private static Text HudCell(Transform parent, string name, float minimum, float maximum)
        {
            return RuntimeUIFactory.CreateText(parent, name, name, 18, MobilityLabPalette.OffWhite, TextAnchor.MiddleCenter,
                new Vector2(minimum, 0f), new Vector2(maximum, 1f), new Vector2(4f, 3f), new Vector2(-4f, -3f));
        }

        private static Button MenuButton(Transform parent, string name, string label, float y)
        {
            return RuntimeUIFactory.CreateButton(parent, name, label, new Color(1f, 1f, 1f, 0.1f), MobilityLabPalette.OffWhite,
                new Vector2(0.12f, y), new Vector2(0.88f, y + 0.12f), Vector2.zero, Vector2.zero);
        }
    }
}
