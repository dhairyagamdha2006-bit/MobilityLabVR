using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MobilityLabVR
{
    public sealed class ExperimentManager : MonoBehaviour
    {
        private readonly List<ITrialResettable> resettableAgents = new List<ITrialResettable>();
        private SimulationClock clock;
        private ScooterController scooter;
        private TrafficSignalController signals;
        private ScenarioManager scenarios;
        private SafetyEventMonitor safety;
        private TelemetryRecorder telemetry;
        private DestinationZone destination;
        private SimulationUIController ui;
        private int trialSequence;

        public TrialCompletionStatus Status { get; private set; } = TrialCompletionStatus.NotStarted;
        public bool IsPaused { get; private set; }
        public ExperimentSummary LastSummary { get; private set; }
        public float ElapsedSeconds => clock != null ? clock.ElapsedSeconds : 0f;

        public event Action<ExperimentSummary> TrialFinished;

        public void Configure(
            SimulationClock simulationClock,
            ScooterController rider,
            TrafficSignalController signalController,
            ScenarioManager scenarioManager,
            SafetyEventMonitor safetyMonitor,
            TelemetryRecorder telemetryRecorder,
            DestinationZone destinationZone,
            SimulationUIController interfaceController,
            IEnumerable<ITrialResettable> agents)
        {
            clock = simulationClock;
            scooter = rider;
            signals = signalController;
            scenarios = scenarioManager;
            safety = safetyMonitor;
            telemetry = telemetryRecorder;
            destination = destinationZone;
            ui = interfaceController;
            resettableAgents.Clear();
            if (agents != null) resettableAgents.AddRange(agents);

            scooter.ResetRequested += ResetCurrentTrial;
            scooter.PauseRequested += TogglePause;
        }

        private void OnDestroy()
        {
            if (scooter == null) return;
            scooter.ResetRequested -= ResetCurrentTrial;
            scooter.PauseRequested -= TogglePause;
        }

        private void Update()
        {
            if (Status != TrialCompletionStatus.InProgress || IsPaused)
            {
                return;
            }

            clock.Tick(Time.unscaledDeltaTime);
            if (scenarios.Current != null && clock.ElapsedSeconds >= scenarios.Current.TrialTimeoutSeconds)
            {
                FinishTrial(TrialCompletionStatus.TimedOut);
            }
        }

        public void StartTrial()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            trialSequence++;
            int baseSeed = SessionContext.Seed;
            scenarios.ResetForTrial(baseSeed);
            signals.ResetForTrial(scenarios.CurrentSeed);
            for (int index = 0; index < resettableAgents.Count; index++)
            {
                resettableAgents[index]?.ResetForTrial(scenarios.CurrentSeed);
            }
            scooter.ResetForTrial(scenarios.CurrentSeed);
            safety.ResetForTrial(scenarios.CurrentSeed);
            destination.ResetZone();
            clock.Reset();
            clock.Start();
            Status = TrialCompletionStatus.InProgress;

            string trialId = $"trial-{DateTime.UtcNow:yyyyMMddHHmmss}-{trialSequence:D2}";
            telemetry.StartTrial(SessionContext.ParticipantId, trialId, scenarios.CurrentSeed);
            ui?.ShowExperiment();
            scooter.SetGameplayFocus(true);
        }

        public void CompleteTrial()
        {
            if (Status == TrialCompletionStatus.InProgress) FinishTrial(TrialCompletionStatus.Completed);
        }

        public void ResetCurrentTrial()
        {
            if (Status == TrialCompletionStatus.InProgress)
            {
                FinishTrial(TrialCompletionStatus.Reset, false);
            }
            StartTrial();
        }

        public void TogglePause()
        {
            if (Status != TrialCompletionStatus.InProgress) return;
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            scooter.SetGameplayFocus(!IsPaused);
            ui?.SetPauseVisible(IsPaused);
        }

        public void Resume()
        {
            if (IsPaused) TogglePause();
        }

        public void ReturnToMenu()
        {
            if (Status == TrialCompletionStatus.InProgress)
            {
                FinishTrial(TrialCompletionStatus.Aborted, false);
            }
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void RepeatTrial()
        {
            StartTrial();
        }

        private void FinishTrial(TrialCompletionStatus finalStatus, bool showResults = true)
        {
            if (Status != TrialCompletionStatus.InProgress) return;
            Status = finalStatus;
            clock.Stop();
            if (finalStatus == TrialCompletionStatus.Completed || finalStatus == TrialCompletionStatus.TimedOut)
            {
                safety.RegisterCompletion(finalStatus == TrialCompletionStatus.TimedOut);
            }
            LastSummary = ExperimentSummaryBuilder.Build(
                SessionContext.ParticipantId,
                telemetry.CurrentTrialId,
                scenarios.Current.Kind,
                scenarios.CurrentSeed,
                finalStatus,
                clock.ElapsedSeconds,
                safety,
                telemetry.CsvPath,
                telemetry.EventLogPath);
            telemetry.FinishTrial(LastSummary);
            scooter.SetGameplayFocus(false);
            TrialFinished?.Invoke(LastSummary);
            if (showResults) ui?.ShowResults(LastSummary);
        }

    }
}
