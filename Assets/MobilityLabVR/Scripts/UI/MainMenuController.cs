using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MobilityLabVR
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private InputField participantInput;
        private Text selectedScenarioText;
        private GameObject scenarioPanel;
        private GameObject instructionsPanel;
        private GameObject settingsPanel;
        private Slider volumeSlider;
        private Slider sensitivitySlider;
        private ScenarioKind selectedScenario = ScenarioKind.Baseline;

        private void Awake()
        {
            BuildInterface();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 1f;
        }

        private void BuildInterface()
        {
            Canvas canvas = RuntimeUIFactory.CreateCanvas("Main Menu Canvas");
            RuntimeUIFactory.CreatePanel(canvas.transform, "Backdrop", MobilityLabPalette.DeepNavy,
                Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RuntimeUIFactory.CreatePanel(canvas.transform, "Accent", MobilityLabPalette.Teal,
                new Vector2(0f, 0f), new Vector2(0.012f, 1f), Vector2.zero, Vector2.zero);

            RuntimeUIFactory.CreateText(canvas.transform, "Brand", "ML // VR", 24, MobilityLabPalette.Teal,
                TextAnchor.MiddleLeft, new Vector2(0.06f, 0.88f), new Vector2(0.28f, 0.94f), Vector2.zero, Vector2.zero);
            RuntimeUIFactory.CreateText(canvas.transform, "Title", "MOBILITYLAB VR", 64, MobilityLabPalette.OffWhite,
                TextAnchor.MiddleLeft, new Vector2(0.06f, 0.72f), new Vector2(0.7f, 0.88f), Vector2.zero, Vector2.zero);
            RuntimeUIFactory.CreateText(canvas.transform, "Subtitle", "VR Micromobility Safety and Transportation Simulator", 27,
                MobilityLabPalette.Sky, TextAnchor.UpperLeft, new Vector2(0.062f, 0.64f), new Vector2(0.7f, 0.73f),
                Vector2.zero, Vector2.zero);
            RuntimeUIFactory.CreateText(canvas.transform, "ResearchQuestion",
                "RESEARCH PROTOTYPE  •  CONTROLLED CAMPUS INTERSECTION  •  REPEATABLE TRIALS", 17,
                new Color(0.68f, 0.75f, 0.78f), TextAnchor.MiddleLeft,
                new Vector2(0.063f, 0.59f), new Vector2(0.75f, 0.64f), Vector2.zero, Vector2.zero);

            GameObject card = RuntimeUIFactory.CreatePanel(canvas.transform, "Experiment Card", new Color(0.07f, 0.13f, 0.18f, 0.98f),
                new Vector2(0.06f, 0.12f), new Vector2(0.56f, 0.57f), Vector2.zero, Vector2.zero);
            RuntimeUIFactory.CreateText(card.transform, "SessionLabel", "ANONYMOUS SESSION ID", 18, MobilityLabPalette.Teal,
                TextAnchor.MiddleLeft, new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.91f), Vector2.zero, Vector2.zero);
            participantInput = RuntimeUIFactory.CreateInputField(card.transform, "ParticipantId", "e.g. P001 (no names)",
                new Vector2(0.06f, 0.60f), new Vector2(0.94f, 0.76f), Vector2.zero, Vector2.zero);
            participantInput.text = SessionContext.ParticipantId == "anonymous" ? string.Empty : SessionContext.ParticipantId;

            selectedScenarioText = RuntimeUIFactory.CreateText(card.transform, "SelectedScenario", "Scenario 01  /  Baseline", 23,
                MobilityLabPalette.OffWhite, TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.39f), new Vector2(0.65f, 0.56f), Vector2.zero, Vector2.zero);
            Button choose = RuntimeUIFactory.CreateButton(card.transform, "ChooseScenario", "CHANGE",
                new Color(1f, 1f, 1f, 0.08f), MobilityLabPalette.Sky,
                new Vector2(0.70f, 0.41f), new Vector2(0.94f, 0.54f), Vector2.zero, Vector2.zero);
            choose.onClick.AddListener(() => SetModal(scenarioPanel));
            Button start = RuntimeUIFactory.CreateButton(card.transform, "StartExperiment", "START EXPERIMENT  →",
                MobilityLabPalette.Teal, MobilityLabPalette.DeepNavy,
                new Vector2(0.06f, 0.10f), new Vector2(0.94f, 0.31f), Vector2.zero, Vector2.zero);
            start.onClick.AddListener(StartExperiment);

            CreateSideButton(canvas.transform, "Scenario Selection", 0.49f, () => SetModal(scenarioPanel));
            CreateSideButton(canvas.transform, "Instructions", 0.40f, () => SetModal(instructionsPanel));
            CreateSideButton(canvas.transform, "Settings", 0.31f, () => SetModal(settingsPanel));
            CreateSideButton(canvas.transform, "Quit", 0.22f, Quit);

            RuntimeUIFactory.CreateText(canvas.transform, "Disclosure",
                "Portfolio research prototype • No human-participant validation • No sensitive data collected", 16,
                new Color(0.55f, 0.62f, 0.65f), TextAnchor.MiddleLeft,
                new Vector2(0.06f, 0.035f), new Vector2(0.9f, 0.08f), Vector2.zero, Vector2.zero);

            scenarioPanel = BuildScenarioPanel(canvas.transform);
            instructionsPanel = BuildInstructionsPanel(canvas.transform);
            settingsPanel = BuildSettingsPanel(canvas.transform);
            scenarioPanel.SetActive(false);
            instructionsPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }

        private void CreateSideButton(Transform parent, string label, float y, UnityEngine.Events.UnityAction action)
        {
            Button button = RuntimeUIFactory.CreateButton(parent, label.Replace(" ", string.Empty), label,
                new Color(0.06f, 0.12f, 0.17f, 0.95f), MobilityLabPalette.OffWhite,
                new Vector2(0.63f, y), new Vector2(0.91f, y + 0.07f), Vector2.zero, Vector2.zero);
            button.onClick.AddListener(action);
        }

        private GameObject BuildScenarioPanel(Transform parent)
        {
            GameObject modal = CreateModal(parent, "Scenario Selection", "SELECT A REPEATABLE TRIAL");
            string[] names = { "01  BASELINE", "02  SUDDEN PEDESTRIAN", "03  VEHICLE FAILS TO YIELD", "04  LOW VISIBILITY" };
            for (int index = 0; index < names.Length; index++)
            {
                int captured = index;
                float top = 0.70f - (index * 0.14f);
                Button button = RuntimeUIFactory.CreateButton(modal.transform, "Scenario" + index, names[index],
                    new Color(1f, 1f, 1f, 0.08f), MobilityLabPalette.OffWhite,
                    new Vector2(0.08f, top), new Vector2(0.92f, top + 0.10f), Vector2.zero, Vector2.zero);
                button.onClick.AddListener(() => SelectScenario((ScenarioKind)captured));
            }
            AddCloseButton(modal);
            return modal;
        }

        private GameObject BuildInstructionsPanel(Transform parent)
        {
            GameObject modal = CreateModal(parent, "Instructions", "RIDER CONTROLS");
            RuntimeUIFactory.CreateText(modal.transform, "Body",
                "W / ↑    Accelerate\nS / ↓    Brake or reverse\nA D / ← →    Steer\nMouse    Look around\nSpace    Emergency brake\nR    Reset repeatable trial\nEsc    Pause\n\nReach the teal destination gate. Obey the signal and respond safely to conflicts.",
                25, MobilityLabPalette.OffWhite, TextAnchor.UpperLeft,
                new Vector2(0.09f, 0.20f), new Vector2(0.91f, 0.80f), Vector2.zero, Vector2.zero);
            AddCloseButton(modal);
            return modal;
        }

        private GameObject BuildSettingsPanel(Transform parent)
        {
            GameObject modal = CreateModal(parent, "Settings", "COMFORT & INPUT");
            RuntimeUIFactory.CreateText(modal.transform, "VolumeLabel", "MASTER VOLUME", 20, MobilityLabPalette.OffWhite,
                TextAnchor.MiddleLeft, new Vector2(0.09f, 0.65f), new Vector2(0.91f, 0.72f), Vector2.zero, Vector2.zero);
            volumeSlider = RuntimeUIFactory.CreateSlider(modal.transform, "Volume", SessionContext.MasterVolume,
                new Vector2(0.09f, 0.57f), new Vector2(0.91f, 0.64f), Vector2.zero, Vector2.zero);
            volumeSlider.onValueChanged.AddListener(value => { SessionContext.MasterVolume = value; AudioListener.volume = value; });

            RuntimeUIFactory.CreateText(modal.transform, "SensitivityLabel", "LOOK SENSITIVITY", 20, MobilityLabPalette.OffWhite,
                TextAnchor.MiddleLeft, new Vector2(0.09f, 0.43f), new Vector2(0.91f, 0.50f), Vector2.zero, Vector2.zero);
            sensitivitySlider = RuntimeUIFactory.CreateSlider(modal.transform, "Sensitivity", SessionContext.LookSensitivity / 0.24f,
                new Vector2(0.09f, 0.35f), new Vector2(0.91f, 0.42f), Vector2.zero, Vector2.zero);
            sensitivitySlider.onValueChanged.AddListener(value => SessionContext.LookSensitivity = Mathf.Lerp(0.04f, 0.24f, value));
            RuntimeUIFactory.CreateText(modal.transform, "XRNote",
                "OpenXR devices are detected automatically. Desktop mode remains available at all times.", 19,
                MobilityLabPalette.Sky, TextAnchor.UpperLeft,
                new Vector2(0.09f, 0.20f), new Vector2(0.91f, 0.31f), Vector2.zero, Vector2.zero);
            AddCloseButton(modal);
            return modal;
        }

        private static GameObject CreateModal(Transform parent, string name, string title)
        {
            GameObject modal = RuntimeUIFactory.CreatePanel(parent, name, new Color(0.035f, 0.075f, 0.11f, 0.995f),
                new Vector2(0.25f, 0.12f), new Vector2(0.75f, 0.88f), Vector2.zero, Vector2.zero);
            RuntimeUIFactory.CreateText(modal.transform, "Title", title, 32, MobilityLabPalette.Teal,
                TextAnchor.MiddleLeft, new Vector2(0.08f, 0.84f), new Vector2(0.92f, 0.95f), Vector2.zero, Vector2.zero);
            return modal;
        }

        private static void AddCloseButton(GameObject modal)
        {
            Button close = RuntimeUIFactory.CreateButton(modal.transform, "Close", "CLOSE", MobilityLabPalette.Coral,
                Color.white, new Vector2(0.68f, 0.05f), new Vector2(0.92f, 0.13f), Vector2.zero, Vector2.zero);
            close.onClick.AddListener(() => modal.SetActive(false));
        }

        private void SetModal(GameObject target)
        {
            scenarioPanel.SetActive(target == scenarioPanel);
            instructionsPanel.SetActive(target == instructionsPanel);
            settingsPanel.SetActive(target == settingsPanel);
        }

        private void SelectScenario(ScenarioKind scenario)
        {
            selectedScenario = scenario;
            string display = scenario == ScenarioKind.VehicleFailsToYield ? "Vehicle Fails to Yield" : SplitCamelCase(scenario.ToString());
            selectedScenarioText.text = $"Scenario {(int)scenario + 1:D2}  /  {display}";
            scenarioPanel.SetActive(false);
        }

        private void StartExperiment()
        {
            SessionContext.SetParticipantId(participantInput.text);
            SessionContext.SelectedScenario = selectedScenario;
            SessionContext.Seed = SessionContext.StableSeed(SessionContext.ParticipantId, selectedScenario, 1);
            SceneManager.LoadScene("Simulation");
        }

        private static string SplitCamelCase(string value)
        {
            return System.Text.RegularExpressions.Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
        }

        private static void Quit()
        {
            Debug.Log("Quit requested from MobilityLab VR main menu.");
            Application.Quit();
        }
    }
}
