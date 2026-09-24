using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobilityLabVR
{
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class PedestrianAgent : MonoBehaviour, ITrialResettable, IHazardTarget
    {
        [SerializeField, Range(0.5f, 3f)] private float walkingSpeed = 1.35f;
        [SerializeField, Range(0.1f, 1f)] private float waypointTolerance = 0.25f;

        private Vector3[] waypoints = Array.Empty<Vector3>();
        private TrafficSignalController signals;
        private RoadAxis roadBeingCrossed;
        private int waypointIndex;
        private bool scenarioHazard;
        private bool hazardActivated;
        private bool crossingStarted;
        private bool startsEnabled;
        private Vector3 previousPosition;
        private Vector3 velocity;
        private Transform leftArm;
        private Transform rightArm;
        private Transform leftLeg;
        private Transform rightLeg;
        private float gaitTime;
        private Renderer[] visualRenderers = Array.Empty<Renderer>();
        private Collider bodyCollider;

        public string HazardId { get; private set; }
        public Transform HazardTransform => transform;
        public Vector3 Velocity => velocity;
        public bool IsActiveHazard => isActiveAndEnabled && (!scenarioHazard || hazardActivated);
        public float CollisionRadius => 0.42f;

        public void Configure(
            string id,
            IReadOnlyList<Vector3> route,
            TrafficSignalController signalController,
            RoadAxis crossingRoad,
            float speed,
            bool isScenarioHazard,
            bool enabledAtStart,
            Transform[] limbs = null)
        {
            HazardId = string.IsNullOrWhiteSpace(id) ? name : id;
            signals = signalController;
            roadBeingCrossed = crossingRoad;
            walkingSpeed = Mathf.Max(0.5f, speed);
            scenarioHazard = isScenarioHazard;
            startsEnabled = enabledAtStart;
            visualRenderers = GetComponentsInChildren<Renderer>(true);
            bodyCollider = GetComponent<Collider>();
            waypoints = new Vector3[route.Count];
            for (int index = 0; index < route.Count; index++) waypoints[index] = route[index];
            if (limbs != null && limbs.Length >= 4)
            {
                leftArm = limbs[0];
                rightArm = limbs[1];
                leftLeg = limbs[2];
                rightLeg = limbs[3];
            }

            ResetForTrial(SessionContext.Seed);
        }

        private void Update()
        {
            if (waypoints.Length < 2 || (!startsEnabled && !hazardActivated))
            {
                velocity = Vector3.zero;
                Animate(false);
                return;
            }

            bool mayCross = crossingStarted || hazardActivated || signals == null || signals.PedestriansMayCross(roadBeingCrossed);
            if (!mayCross)
            {
                velocity = Vector3.zero;
                Animate(false);
                return;
            }

            crossingStarted = true;
            Vector3 target = waypoints[waypointIndex];
            Vector3 offset = target - transform.position;
            offset.y = 0f;
            if (offset.magnitude <= waypointTolerance)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
                if (waypointIndex == 0) crossingStarted = false;
                target = waypoints[waypointIndex];
                offset = target - transform.position;
                offset.y = 0f;
            }

            if (offset.sqrMagnitude > 0.001f)
            {
                Vector3 direction = offset.normalized;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction), 260f * Time.deltaTime);
                transform.position += direction * (walkingSpeed * Time.deltaTime);
            }

            velocity = Time.deltaTime > 0f ? (transform.position - previousPosition) / Time.deltaTime : Vector3.zero;
            previousPosition = transform.position;
            Animate(currentSpeed: velocity.magnitude > 0.05f);
        }

        public void ActivateHazard()
        {
            hazardActivated = true;
            crossingStarted = true;
            SetScenarioPresence(true);
        }

        public void SetWalkingSpeed(float metersPerSecond)
        {
            walkingSpeed = Mathf.Max(0.5f, metersPerSecond);
        }

        public void ResetForTrial(int seed)
        {
            waypointIndex = waypoints.Length > 1 ? 1 : 0;
            if (waypoints.Length > 0)
            {
                transform.position = waypoints[0];
                Vector3 direction = waypoints.Length > 1 ? waypoints[1] - waypoints[0] : Vector3.forward;
                if (direction.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(direction.normalized);
            }

            crossingStarted = false;
            hazardActivated = false;
            SetScenarioPresence(!scenarioHazard);
            previousPosition = transform.position;
            velocity = Vector3.zero;
            gaitTime = 0f;
            Animate(false);
        }

        private void Animate(bool currentSpeed)
        {
            if (leftArm == null) return;
            gaitTime = currentSpeed ? gaitTime + Time.deltaTime * 7f : 0f;
            float swing = currentSpeed ? Mathf.Sin(gaitTime) * 24f : 0f;
            leftArm.localRotation = Quaternion.Euler(swing, 0f, 0f);
            rightArm.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            leftLeg.localRotation = Quaternion.Euler(-swing, 0f, 0f);
            rightLeg.localRotation = Quaternion.Euler(swing, 0f, 0f);
        }

        private void SetScenarioPresence(bool present)
        {
            if (!scenarioHazard) return;
            if (bodyCollider != null) bodyCollider.enabled = present;
            for (int index = 0; index < visualRenderers.Length; index++)
            {
                if (visualRenderers[index] != null) visualRenderers[index].enabled = present;
            }
        }
    }
}
