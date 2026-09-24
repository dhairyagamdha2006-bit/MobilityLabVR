using System.Collections.Generic;
using UnityEngine;

namespace MobilityLabVR
{
    public static class AgentFactory
    {
        public static VehicleAgent CreateVehicle(
            Transform parent,
            string id,
            IReadOnlyList<Vector3> route,
            TrafficSignalController signals,
            RoadAxis axis,
            float speed,
            Color color,
            bool scenarioHazard = false,
            bool enabledAtStart = true)
        {
            GameObject root = new GameObject(id, typeof(Rigidbody), typeof(BoxCollider), typeof(VehicleAgent));
            root.transform.SetParent(parent, false);
            root.layer = 8;
            Rigidbody body = root.GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            BoxCollider collider = root.GetComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.65f, 0f);
            collider.size = new Vector3(1.9f, 1.3f, 4f);

            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Body", root.transform,
                new Vector3(0f, 0.65f, 0f), new Vector3(1.9f, 0.75f, 3.9f), color);
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Cabin", root.transform,
                new Vector3(0f, 1.25f, -0.25f), new Vector3(1.55f, 0.7f, 2f),
                new Color32(81, 126, 143, 255));
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Front Light Left", root.transform,
                new Vector3(-0.55f, 0.65f, 1.98f), new Vector3(0.35f, 0.25f, 0.08f), MobilityLabPalette.OffWhite, false, null, true);
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, "Front Light Right", root.transform,
                new Vector3(0.55f, 0.65f, 1.98f), new Vector3(0.35f, 0.25f, 0.08f), MobilityLabPalette.OffWhite, false, null, true);
            CreateWheel(root.transform, new Vector3(-1f, 0.38f, 1.25f));
            CreateWheel(root.transform, new Vector3(1f, 0.38f, 1.25f));
            CreateWheel(root.transform, new Vector3(-1f, 0.38f, -1.25f));
            CreateWheel(root.transform, new Vector3(1f, 0.38f, -1.25f));

            VehicleAgent agent = root.GetComponent<VehicleAgent>();
            agent.Configure(id, route, signals, axis, speed, scenarioHazard, enabledAtStart);
            return agent;
        }

        public static PedestrianAgent CreatePedestrian(
            Transform parent,
            string id,
            IReadOnlyList<Vector3> route,
            TrafficSignalController signals,
            RoadAxis crossingRoad,
            float speed,
            Color shirtColor,
            bool scenarioHazard = false,
            bool enabledAtStart = true)
        {
            GameObject root = new GameObject(id, typeof(Rigidbody), typeof(CapsuleCollider), typeof(PedestrianAgent));
            root.transform.SetParent(parent, false);
            root.layer = 8;
            Rigidbody body = root.GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            CapsuleCollider collider = root.GetComponent<CapsuleCollider>();
            collider.center = new Vector3(0f, 0.95f, 0f);
            collider.height = 1.9f;
            collider.radius = 0.35f;

            RuntimePrimitiveFactory.Primitive(PrimitiveType.Capsule, "Torso", root.transform,
                new Vector3(0f, 1.15f, 0f), new Vector3(0.55f, 0.65f, 0.38f), shirtColor);
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0f, 1.95f, 0f), new Vector3(0.48f, 0.48f, 0.48f), new Color32(190, 145, 111, 255));
            Transform leftArm = Limb(root.transform, "Left Arm", new Vector3(-0.37f, 1.55f, 0f), new Vector3(0.16f, 0.65f, 0.16f), shirtColor);
            Transform rightArm = Limb(root.transform, "Right Arm", new Vector3(0.37f, 1.55f, 0f), new Vector3(0.16f, 0.65f, 0.16f), shirtColor);
            Transform leftLeg = Limb(root.transform, "Left Leg", new Vector3(-0.18f, 0.75f, 0f), new Vector3(0.19f, 0.75f, 0.19f), MobilityLabPalette.Navy);
            Transform rightLeg = Limb(root.transform, "Right Leg", new Vector3(0.18f, 0.75f, 0f), new Vector3(0.19f, 0.75f, 0.19f), MobilityLabPalette.Navy);

            PedestrianAgent agent = root.GetComponent<PedestrianAgent>();
            agent.Configure(id, route, signals, crossingRoad, speed, scenarioHazard, enabledAtStart,
                new[] { leftArm, rightArm, leftLeg, rightLeg });
            return agent;
        }

        private static Transform Limb(Transform parent, string name, Vector3 pivotPosition, Vector3 scale, Color color)
        {
            GameObject pivot = new GameObject(name + " Pivot");
            pivot.transform.SetParent(parent, false);
            pivot.transform.localPosition = pivotPosition;
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cube, name, pivot.transform,
                new Vector3(0f, -scale.y * 0.45f, 0f), scale, color);
            return pivot.transform;
        }

        private static void CreateWheel(Transform parent, Vector3 position)
        {
            RuntimePrimitiveFactory.Primitive(PrimitiveType.Cylinder, "Wheel", parent, position,
                new Vector3(0.42f, 0.16f, 0.42f), new Color32(25, 27, 29, 255), false,
                Quaternion.Euler(0f, 0f, 90f));
        }
    }
}
