using System.Collections.Generic;
using UnityEngine;

namespace MobilityLabVR
{
    [DefaultExecutionOrder(-1000)]
    public sealed class SimulationSceneBootstrap : MonoBehaviour
    {
        private ExperimentManager experiment;

        private void Awake()
        {
            BuildSimulation();
        }

        private void Start()
        {
            experiment.StartTrial();
        }

        private void BuildSimulation()
        {
            GameObject worldRoot = new GameObject("Generated Campus Environment");
            GameObject systemRoot = new GameObject("Simulation Systems");
            GameObject agentRoot = new GameObject("Traffic and Pedestrian Agents");

            TrafficSignalController signals = new GameObject("Traffic Signal Controller", typeof(TrafficSignalController))
                .GetComponent<TrafficSignalController>();
            signals.transform.SetParent(systemRoot.transform, false);
            signals.Configure(TrafficSignalTiming.Default);

            WorldBuildResult world = CampusEnvironmentBuilder.Build(worldRoot.transform, signals);
            SimulationClock clock = new SimulationClock();
            SafetyEventMonitor safety = new GameObject("Safety Event Monitor", typeof(SafetyEventMonitor))
                .GetComponent<SafetyEventMonitor>();
            safety.transform.SetParent(systemRoot.transform, false);
            ScenarioManager scenarios = new GameObject("Scenario Manager", typeof(ScenarioManager))
                .GetComponent<ScenarioManager>();
            scenarios.transform.SetParent(systemRoot.transform, false);

            CompositeInputSource input = CreateInput(systemRoot.transform);
            ScooterController scooter = CreateScooter(input, safety);
            scooter.transform.SetParent(agentRoot.transform, true);

            List<ITrialResettable> agents = new List<ITrialResettable>();
            List<IHazardTarget> hazards = new List<IHazardTarget>();
            CreateAgents(agentRoot.transform, signals, agents, hazards,
                out PedestrianAgent scenarioPedestrian, out VehicleAgent scenarioVehicle);

            ScenarioTriggerZone trigger = CreateScenarioTrigger(worldRoot.transform);
            DestinationZone destination = CreateDestination(worldRoot.transform);
            scenarios.Configure(DefaultScenarioFactory.CreateRuntimeDefinitions(), scenarioPedestrian, scenarioVehicle,
                trigger, world.Sun, safety);
            trigger.Configure(scenarios);
            safety.Configure(scooter, scenarios, clock);
            for (int index = 0; index < hazards.Count; index++) safety.RegisterHazard(hazards[index]);

            TelemetryRecorder telemetry = new GameObject("Telemetry Recorder", typeof(TelemetryRecorder))
                .GetComponent<TelemetryRecorder>();
            telemetry.transform.SetParent(systemRoot.transform, false);
            telemetry.Configure(scooter, safety, signals, scenarios, clock);

            SimulationUIController ui = new GameObject("Simulation UI Controller", typeof(SimulationUIController))
                .GetComponent<SimulationUIController>();
            ui.transform.SetParent(systemRoot.transform, false);
            experiment = new GameObject("Experiment Manager", typeof(ExperimentManager)).GetComponent<ExperimentManager>();
            experiment.transform.SetParent(systemRoot.transform, false);
            experiment.Configure(clock, scooter, signals, scenarios, safety, telemetry, destination, ui, agents);
            destination.Configure(experiment);
            ui.Configure(experiment, scooter, safety, scenarios, signals);
        }

        private static CompositeInputSource CreateInput(Transform parent)
        {
            GameObject inputObject = new GameObject("Input Adapters", typeof(DesktopInputSource), typeof(XRInputSource),
                typeof(CompositeInputSource));
            inputObject.transform.SetParent(parent, false);
            DesktopInputSource desktop = inputObject.GetComponent<DesktopInputSource>();
            XRInputSource xr = inputObject.GetComponent<XRInputSource>();
            CompositeInputSource composite = inputObject.GetComponent<CompositeInputSource>();
            composite.Configure(desktop, xr);
            return composite;
        }

        private static ScooterController CreateScooter(IPlayerInputSource input, SafetyEventMonitor safety)
        {
            Vector3 spawnPosition = new Vector3(5.5f, 0.15f, -38f);
            GameObject scooterObject = new GameObject("Rider Scooter", typeof(Rigidbody), typeof(CapsuleCollider),
                typeof(ScooterController));
            scooterObject.layer = 8;
            scooterObject.transform.position = spawnPosition;
            CapsuleCollider collider = scooterObject.GetComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.95f, 0f);
            collider.radius = 0.52f;
            collider.height = 1.9f;

            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Deck", scooterObject.transform,
                new Vector3(0f, 0.14f, 0f), new Vector3(0.55f, 0.16f, 1.55f), MobilityLabPalette.Teal);
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cylinder, "Front Wheel", scooterObject.transform,
                new Vector3(0f, 0.22f, 0.82f), new Vector3(0.28f, 0.12f, 0.28f), new Color32(25, 27, 29, 255),
                false, Quaternion.Euler(0f, 0f, 90f));
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cylinder, "Rear Wheel", scooterObject.transform,
                new Vector3(0f, 0.22f, -0.72f), new Vector3(0.28f, 0.12f, 0.28f), new Color32(25, 27, 29, 255),
                false, Quaternion.Euler(0f, 0f, 90f));
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cylinder, "Stem", scooterObject.transform,
                new Vector3(0f, 0.92f, 0.67f), new Vector3(0.055f, 0.75f, 0.055f), MobilityLabPalette.Navy);
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Handlebar", scooterObject.transform,
                new Vector3(0f, 1.65f, 0.67f), new Vector3(0.75f, 0.07f, 0.07f), MobilityLabPalette.Navy);

            GameObject pivot = new GameObject("View Pivot");
            pivot.transform.SetParent(scooterObject.transform, false);
            pivot.transform.localPosition = new Vector3(0f, 1.62f, 0.05f);
            GameObject cameraObject = new GameObject("Rider Camera", typeof(Camera), typeof(AudioListener), typeof(XRHeadPoseDriver));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(pivot.transform, false);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.fieldOfView = 72f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 180f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.48f, 0.69f, 0.8f);
            camera.allowHDR = true;

            ScooterController controller = scooterObject.GetComponent<ScooterController>();
            controller.Configure(input, pivot.transform, safety, spawnPosition, Quaternion.identity);
            return controller;
        }

        private static void CreateAgents(
            Transform parent,
            TrafficSignalController signals,
            ICollection<ITrialResettable> resettableAgents,
            ICollection<IHazardTarget> hazards,
            out PedestrianAgent scenarioPedestrian,
            out VehicleAgent scenarioVehicle)
        {
            VehicleAgent northSouthCar = AgentFactory.CreateVehicle(parent, "Vehicle-NS-01", new[]
            {
                new Vector3(-2.5f, 0.02f, -48f), new Vector3(-2.5f, 0.02f, 48f),
                new Vector3(-28f, 0.02f, 48f), new Vector3(-28f, 0.02f, -48f)
            }, signals, RoadAxis.NorthSouth, 5.2f, new Color32(66, 132, 184, 255));
            VehicleAgent eastWestCar = AgentFactory.CreateVehicle(parent, "Vehicle-EW-01", new[]
            {
                new Vector3(48f, 0.02f, -2.6f), new Vector3(-48f, 0.02f, -2.6f),
                new Vector3(-48f, 0.02f, -27f), new Vector3(48f, 0.02f, -27f)
            }, signals, RoadAxis.EastWest, 5.7f, new Color32(221, 132, 64, 255));
            VehicleAgent followingCar = AgentFactory.CreateVehicle(parent, "Vehicle-EW-02", new[]
            {
                new Vector3(33f, 0.02f, 2.7f), new Vector3(-48f, 0.02f, 2.7f),
                new Vector3(-48f, 0.02f, 28f), new Vector3(48f, 0.02f, 28f), new Vector3(48f, 0.02f, 2.7f)
            }, signals, RoadAxis.EastWest, 4.9f, new Color32(163, 83, 110, 255));

            PedestrianAgent normalPedestrian = AgentFactory.CreatePedestrian(parent, "Pedestrian-01", new[]
            {
                new Vector3(-12f, 0.16f, 8.8f), new Vector3(12f, 0.16f, 8.8f)
            }, signals, RoadAxis.NorthSouth, 1.25f, new Color32(78, 145, 188, 255));
            PedestrianAgent secondPedestrian = AgentFactory.CreatePedestrian(parent, "Pedestrian-02", new[]
            {
                new Vector3(-8.8f, 0.16f, -12f), new Vector3(-8.8f, 0.16f, 12f)
            }, signals, RoadAxis.EastWest, 1.4f, new Color32(208, 114, 82, 255));

            scenarioPedestrian = AgentFactory.CreatePedestrian(parent, "Scenario-Sudden-Pedestrian", new[]
            {
                new Vector3(11f, 0.16f, -2.5f), new Vector3(-11f, 0.16f, -2.5f)
            }, signals, RoadAxis.NorthSouth, 2.1f, MobilityLabPalette.Amber, true, false);
            scenarioVehicle = AgentFactory.CreateVehicle(parent, "Scenario-Failing-Vehicle", new[]
            {
                new Vector3(16f, 0.02f, 2.8f), new Vector3(8f, 0.02f, 2.8f),
                new Vector3(5.2f, 0.02f, 6f), new Vector3(5.2f, 0.02f, 42f),
                new Vector3(34f, 0.02f, 42f), new Vector3(34f, 0.02f, 2.8f)
            }, signals, RoadAxis.EastWest, 4.4f, MobilityLabPalette.Coral, true, false);

            resettableAgents.Add(northSouthCar);
            resettableAgents.Add(eastWestCar);
            resettableAgents.Add(followingCar);
            resettableAgents.Add(normalPedestrian);
            resettableAgents.Add(secondPedestrian);
            hazards.Add(northSouthCar);
            hazards.Add(eastWestCar);
            hazards.Add(followingCar);
            hazards.Add(normalPedestrian);
            hazards.Add(secondPedestrian);
            hazards.Add(scenarioPedestrian);
            hazards.Add(scenarioVehicle);
        }

        private static ScenarioTriggerZone CreateScenarioTrigger(Transform parent)
        {
            GameObject triggerObject = new GameObject("Scenario Activation Zone", typeof(BoxCollider), typeof(ScenarioTriggerZone));
            triggerObject.transform.SetParent(parent, false);
            triggerObject.transform.localPosition = new Vector3(5.5f, 0f, -14f);
            BoxCollider collider = triggerObject.GetComponent<BoxCollider>();
            collider.center = new Vector3(0f, 1.2f, 0f);
            collider.size = new Vector3(3.8f, 2.4f, 1.2f);
            collider.isTrigger = true;
            return triggerObject.GetComponent<ScenarioTriggerZone>();
        }

        private static DestinationZone CreateDestination(Transform parent)
        {
            GameObject destinationObject = new GameObject("Destination Gate", typeof(BoxCollider), typeof(DestinationZone));
            destinationObject.transform.SetParent(parent, false);
            destinationObject.transform.localPosition = new Vector3(5.5f, 0f, 38f);
            BoxCollider collider = destinationObject.GetComponent<BoxCollider>();
            collider.center = new Vector3(0f, 1.5f, 0f);
            collider.size = new Vector3(4f, 3f, 1.2f);
            collider.isTrigger = true;
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Left Post", destinationObject.transform,
                new Vector3(-2.2f, 1.6f, 0f), new Vector3(0.28f, 3.2f, 0.28f), MobilityLabPalette.Teal);
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Right Post", destinationObject.transform,
                new Vector3(2.2f, 1.6f, 0f), new Vector3(0.28f, 3.2f, 0.28f), MobilityLabPalette.Teal);
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Top Bar", destinationObject.transform,
                new Vector3(0f, 3.2f, 0f), new Vector3(4.7f, 0.28f, 0.28f), MobilityLabPalette.Teal);
            RuntimePrimitiveFactory.WorldText("Destination Text", destinationObject.transform, "DESTINATION",
                new Vector3(0f, 3.55f, -0.05f), Quaternion.identity, 0.18f, MobilityLabPalette.OffWhite);
            return destinationObject.GetComponent<DestinationZone>();
        }
    }
}
