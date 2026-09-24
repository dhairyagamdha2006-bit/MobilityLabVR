using System;
using System.Collections.Generic;
using UnityEngine;

namespace MobilityLabVR
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class VehicleAgent : MonoBehaviour, ITrialResettable, IHazardTarget
    {
        [SerializeField, Min(0.5f), Tooltip("Comfort cruising speed in metres per second.")]
        private float cruiseSpeed = 5.5f;
        [SerializeField, Min(0.1f)] private float acceleration = 2.5f;
        [SerializeField, Min(0.1f)] private float braking = 5f;
        [SerializeField, Min(0.5f)] private float waypointTolerance = 0.75f;
        [SerializeField, Min(1f)] private float obstacleLookAhead = 6f;

        private Vector3[] waypoints = Array.Empty<Vector3>();
        private TrafficSignalController signals;
        private RoadAxis routeAxis;
        private int waypointIndex;
        private float currentSpeed;
        private bool scenarioHazard;
        private bool hazardActivated;
        private bool ignoreSignal;
        private bool startsEnabled;
        private Vector3 previousPosition;
        private Vector3 velocity;
        private readonly RaycastHit[] obstacleHits = new RaycastHit[8];
        private Renderer[] visualRenderers = Array.Empty<Renderer>();
        private Collider bodyCollider;

        public string HazardId { get; private set; }
        public Transform HazardTransform => transform;
        public Vector3 Velocity => velocity;
        public bool IsActiveHazard => isActiveAndEnabled && (!scenarioHazard || hazardActivated);
        public float CollisionRadius => 1.25f;

        public void Configure(
            string id,
            IReadOnlyList<Vector3> route,
            TrafficSignalController signalController,
            RoadAxis axis,
            float configuredSpeed,
            bool isScenarioHazard,
            bool movementEnabledAtStart)
        {
            HazardId = string.IsNullOrWhiteSpace(id) ? name : id;
            signals = signalController;
            routeAxis = axis;
            cruiseSpeed = Mathf.Max(0.5f, configuredSpeed);
            scenarioHazard = isScenarioHazard;
            startsEnabled = movementEnabledAtStart;
            visualRenderers = GetComponentsInChildren<Renderer>(true);
            bodyCollider = GetComponent<Collider>();
            waypoints = new Vector3[route.Count];
            for (int index = 0; index < route.Count; index++) waypoints[index] = route[index];
            ResetForTrial(SessionContext.Seed);
        }

        private void Update()
        {
            if (waypoints.Length < 2)
            {
                velocity = Vector3.zero;
                return;
            }

            bool mayMove = startsEnabled || hazardActivated;
            float targetSpeed = mayMove && !MustStop() ? cruiseSpeed : 0f;
            float rate = targetSpeed < currentSpeed ? braking : acceleration;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

            Vector3 target = waypoints[waypointIndex];
            Vector3 offset = target - transform.position;
            offset.y = 0f;
            if (offset.magnitude <= waypointTolerance)
            {
                waypointIndex = (waypointIndex + 1) % waypoints.Length;
                target = waypoints[waypointIndex];
                offset = target - transform.position;
                offset.y = 0f;
            }

            if (offset.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(offset.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, 90f * Time.deltaTime);
                transform.position += transform.forward * (currentSpeed * Time.deltaTime);
            }

            velocity = Time.deltaTime > 0f ? (transform.position - previousPosition) / Time.deltaTime : Vector3.zero;
            previousPosition = transform.position;
        }

        public void ActivateHazard(bool shouldIgnoreSignal)
        {
            hazardActivated = true;
            ignoreSignal = shouldIgnoreSignal;
            SetScenarioPresence(true);
        }

        public void SetCruiseSpeed(float metersPerSecond)
        {
            cruiseSpeed = Mathf.Max(0.5f, metersPerSecond);
        }

        public void ResetForTrial(int seed)
        {
            waypointIndex = waypoints.Length > 1 ? 1 : 0;
            if (waypoints.Length > 0)
            {
                transform.position = waypoints[0];
                Vector3 direction = waypoints.Length > 1 ? waypoints[1] - waypoints[0] : Vector3.forward;
                if (direction.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }

            float speedVariation = DeterministicVariation(seed, HazardId) * 0.35f;
            currentSpeed = startsEnabled ? Mathf.Max(0f, cruiseSpeed - speedVariation) : 0f;
            hazardActivated = false;
            ignoreSignal = false;
            SetScenarioPresence(!scenarioHazard);
            previousPosition = transform.position;
            velocity = Vector3.zero;
        }

        private bool MustStop()
        {
            if (ObstacleAhead()) return true;
            if (ignoreSignal || signals == null) return false;

            RoadSignalState state = signals.GetVehicleState(routeAxis);
            if (state == RoadSignalState.Green) return false;

            Vector3 toIntersection = -transform.position;
            toIntersection.y = 0f;
            float distance = toIntersection.magnitude;
            bool approaching = Vector3.Dot(transform.forward, toIntersection.normalized) > 0.6f;
            return approaching && distance > 6f && distance < 15f;
        }

        private bool ObstacleAhead()
        {
            Vector3 origin = transform.position + (Vector3.up * 0.65f) + (transform.forward * 1.4f);
            int hitCount = Physics.SphereCastNonAlloc(origin, 0.45f, transform.forward, obstacleHits,
                obstacleLookAhead, 1 << 8, QueryTriggerInteraction.Ignore);
            for (int index = 0; index < hitCount; index++)
            {
                if (obstacleHits[index].transform != null && obstacleHits[index].transform.root != transform.root)
                {
                    return true;
                }
            }
            return false;
        }

        private static float DeterministicVariation(int seed, string id)
        {
            unchecked
            {
                int hash = seed;
                if (id != null)
                {
                    for (int index = 0; index < id.Length; index++) hash = (hash * 31) + id[index];
                }

                return Mathf.Abs(hash % 1000) / 1000f;
            }
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
