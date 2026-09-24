using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MobilityLabVR.Tests
{
    public sealed class BootstrapSceneSmokeTests
    {
        [UnityTest]
        public IEnumerator SavedScenes_BootstrapMenuAndDesktopSimulation()
        {
            SceneManager.LoadScene("MainMenu");
            yield return null;
            Assert.That(Object.FindAnyObjectByType<MainMenuController>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<Canvas>(), Is.Not.Null);

            SceneManager.LoadScene("Simulation");
            yield return null;
            yield return null;
            Assert.That(Object.FindAnyObjectByType<ScooterController>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<TrafficSignalController>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<ScenarioManager>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<ExperimentManager>(), Is.Not.Null);
            Assert.That(Object.FindAnyObjectByType<Camera>(), Is.Not.Null);

            TelemetryRecorder recorder = Object.FindAnyObjectByType<TelemetryRecorder>();
            string csvPath = recorder != null ? recorder.CsvPath : string.Empty;
            string eventPath = recorder != null ? recorder.EventLogPath : string.Empty;
            ExperimentManager experiment = Object.FindAnyObjectByType<ExperimentManager>();
            experiment.ReturnToMenu();
            yield return null;
            DeleteTestOutput(csvPath);
            DeleteTestOutput(eventPath);
        }

        private static void DeleteTestOutput(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
        }
    }
}
