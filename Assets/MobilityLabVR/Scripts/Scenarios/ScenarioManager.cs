using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MobilityLabVR
{
    public sealed class ScenarioManager : MonoBehaviour, ITrialResettable
    {
        private IReadOnlyList<ScenarioDefinition> definitions;
        private PedestrianAgent suddenPedestrian;
        private VehicleAgent failingVehicle;
        private ScenarioTriggerZone triggerZone;
        private Light sun;
        private SafetyEventMonitor safety;
        private Color normalAmbient;
        private float normalAmbientIntensity;
        private Color normalFogColor;
        private bool normalFog;
        private float normalFogDensity;

        public ScenarioDefinition Current { get; private set; }
        public IHazardTarget ActiveHazard { get; private set; }
        public bool HazardActivated { get; private set; }
        public float HazardActivationElapsedSeconds { get; private set; } = -1f;
        public int CurrentSeed { get; private set; }

        public event Action<ScenarioDefinition> ScenarioConfigured;
        public event Action<IHazardTarget> HazardActivatedEvent;

        public void Configure(
            IReadOnlyList<ScenarioDefinition> scenarioDefinitions,
            PedestrianAgent scenarioPedestrian,
            VehicleAgent scenarioVehicle,
            ScenarioTriggerZone activationZone,
            Light directionalLight,
            SafetyEventMonitor safetyMonitor)
        {
            definitions = scenarioDefinitions;
            suddenPedestrian = scenarioPedestrian;
            failingVehicle = scenarioVehicle;
            triggerZone = activationZone;
            sun = directionalLight;
            safety = safetyMonitor;

            normalAmbient = RenderSettings.ambientLight;
            normalAmbientIntensity = RenderSettings.ambientIntensity;
            normalFog = RenderSettings.fog;
            normalFogColor = RenderSettings.fogColor;
            normalFogDensity = RenderSettings.fogDensity;
        }

        public void Select(ScenarioKind kind)
        {
            if (definitions == null || definitions.Count == 0)
            {
                definitions = DefaultScenarioFactory.CreateRuntimeDefinitions();
            }

            Current = definitions.FirstOrDefault(definition => definition.Kind == kind) ?? definitions[0];
            SessionContext.SelectedScenario = Current.Kind;
            if (Current.Kind == ScenarioKind.SuddenPedestrian)
            {
                suddenPedestrian?.SetWalkingSpeed(Current.HazardSpeedMetersPerSecond);
            }
            else if (Current.Kind == ScenarioKind.VehicleFailsToYield)
            {
                failingVehicle?.SetCruiseSpeed(Current.HazardSpeedMetersPerSecond);
            }
            if (triggerZone != null)
            {
                Vector3 position = triggerZone.transform.position;
                position.z = -Current.TriggerDistanceFromIntersection;
                triggerZone.transform.position = position;
            }
            ApplyVisibility();
            ScenarioConfigured?.Invoke(Current);
        }

        public void ResetForTrial(int seed)
        {
            Select(SessionContext.SelectedScenario);
            CurrentSeed = unchecked(seed + Current.SeedOffset);
            UnityEngine.Random.InitState(CurrentSeed);
            HazardActivated = false;
            HazardActivationElapsedSeconds = -1f;
            ActiveHazard = null;
            triggerZone?.ResetZone();
            suddenPedestrian?.ResetForTrial(CurrentSeed);
            failingVehicle?.ResetForTrial(CurrentSeed);
            ApplyVisibility();
        }

        public void ActivateScenarioHazard()
        {
            if (HazardActivated || Current == null)
            {
                return;
            }

            switch (Current.Kind)
            {
                case ScenarioKind.SuddenPedestrian:
                    suddenPedestrian?.ActivateHazard();
                    ActiveHazard = suddenPedestrian;
                    break;
                case ScenarioKind.VehicleFailsToYield:
                    failingVehicle?.ActivateHazard(true);
                    ActiveHazard = failingVehicle;
                    break;
                default:
                    return;
            }

            HazardActivated = true;
            HazardActivationElapsedSeconds = safety != null ? safety.ElapsedSeconds : 0f;
            HazardActivatedEvent?.Invoke(ActiveHazard);
            safety?.RegisterHazardActivation(ActiveHazard != null ? ActiveHazard.HazardId : "hazard");
        }

        private void ApplyVisibility()
        {
            bool lowVisibility = Current != null && Current.Kind == ScenarioKind.LowVisibility;
            RenderSettings.fog = lowVisibility || normalFog;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = lowVisibility ? Current.FogDensity : normalFogDensity;
            RenderSettings.fogColor = lowVisibility ? new Color(0.34f, 0.39f, 0.43f) : normalFogColor;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = lowVisibility ? new Color(0.16f, 0.2f, 0.25f) : normalAmbient;
            RenderSettings.ambientIntensity = lowVisibility ? Current.AmbientIntensity : normalAmbientIntensity;

            if (sun != null)
            {
                sun.intensity = lowVisibility ? 0.35f : 1.15f;
                sun.color = lowVisibility ? new Color(0.67f, 0.72f, 0.82f) : new Color(1f, 0.94f, 0.82f);
                sun.transform.rotation = Quaternion.Euler(lowVisibility ? 12f : 42f, -32f, 0f);
            }
        }
    }
}
