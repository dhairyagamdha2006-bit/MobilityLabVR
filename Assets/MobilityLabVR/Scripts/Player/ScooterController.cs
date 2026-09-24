using System;
using UnityEngine;

namespace MobilityLabVR
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class ScooterController : MonoBehaviour, ITrialResettable
    {
        [Header("Longitudinal dynamics")]
        [SerializeField, Min(0.5f), Tooltip("Maximum forward speed in metres per second.")]
        private float maximumSpeed = 8.5f;
        [SerializeField, Min(0.1f)] private float maximumReverseSpeed = 2f;
        [SerializeField, Min(0.1f)] private float accelerationRate = 3.2f;
        [SerializeField, Min(0.1f)] private float normalBrakeRate = 5.5f;
        [SerializeField, Min(0.1f)] private float emergencyBrakeRate = 10f;
        [SerializeField, Min(0f)] private float rollingResistance = 0.65f;

        [Header("Steering and view")]
        [SerializeField, Range(5f, 120f)] private float maximumSteerRateDegrees = 72f;
        [SerializeField, Range(0f, 1f)] private float lateralGrip = 0.86f;
        [SerializeField, Range(20f, 89f)] private float verticalLookLimit = 75f;

        private Rigidbody body;
        private Transform cameraPivot;
        private IPlayerInputSource inputSource;
        private SafetyEventMonitor safetyMonitor;
        private XRHeadPoseDriver headPoseDriver;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private PlayerControlState input;
        private float cameraYaw;
        private float cameraPitch;
        private float previousForwardSpeed;

        public event Action ResetRequested;
        public event Action PauseRequested;

        public float SpeedMetersPerSecond => body == null ? 0f : Mathf.Abs(Vector3.Dot(body.linearVelocity, transform.forward));
        public Vector3 Velocity => body == null ? Vector3.zero : body.linearVelocity;
        public float LongitudinalInput => input.Throttle;
        public bool IsEmergencyBraking => input.EmergencyBrakeHeld;
        public bool IsBraking => input.EmergencyBrakeHeld || (input.Throttle < -0.05f && previousForwardSpeed > 0.05f);
        public float LongitudinalAcceleration { get; private set; }
        public float MaximumSpeed => maximumSpeed;

        public void Configure(
            IPlayerInputSource source,
            Transform viewPivot,
            SafetyEventMonitor monitor,
            Vector3 initialPosition,
            Quaternion initialRotation)
        {
            inputSource = source;
            cameraPivot = viewPivot;
            safetyMonitor = monitor;
            headPoseDriver = viewPivot != null ? viewPivot.GetComponentInChildren<XRHeadPoseDriver>() : null;
            spawnPosition = initialPosition;
            spawnRotation = initialRotation;
            EnsureComponents();
        }

        private void Awake()
        {
            EnsureComponents();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
        }

        private void OnEnable()
        {
            inputSource?.SetGameplayFocus(true);
        }

        private void Update()
        {
            if (inputSource == null)
            {
                return;
            }

            input = inputSource.ReadInput();
            if (input.ResetPressed) ResetRequested?.Invoke();
            if (input.PausePressed) PauseRequested?.Invoke();
            UpdateView();
        }

        private void FixedUpdate()
        {
            if (body == null)
            {
                return;
            }

            float deltaTime = Time.fixedDeltaTime;
            float forwardSpeed = Vector3.Dot(body.linearVelocity, transform.forward);
            float speedBefore = forwardSpeed;
            if (input.EmergencyBrakeHeld)
            {
                forwardSpeed = Mathf.MoveTowards(forwardSpeed, 0f, emergencyBrakeRate * deltaTime);
            }
            else if (input.Throttle > 0.01f)
            {
                forwardSpeed = Mathf.MoveTowards(forwardSpeed, maximumSpeed * input.Throttle, accelerationRate * deltaTime);
            }
            else if (input.Throttle < -0.01f)
            {
                float appliedRate = forwardSpeed > 0.05f ? normalBrakeRate : accelerationRate * 0.65f;
                float target = forwardSpeed > 0.05f ? 0f : -maximumReverseSpeed * -input.Throttle;
                forwardSpeed = Mathf.MoveTowards(forwardSpeed, target, appliedRate * deltaTime);
            }
            else
            {
                forwardSpeed = Mathf.MoveTowards(forwardSpeed, 0f, rollingResistance * deltaTime);
            }

            forwardSpeed = Mathf.Clamp(forwardSpeed, -maximumReverseSpeed, maximumSpeed);
            Vector3 lateralVelocity = Vector3.Project(body.linearVelocity, transform.right);
            Vector3 verticalVelocity = Vector3.Project(body.linearVelocity, Vector3.up);
            body.linearVelocity = (transform.forward * forwardSpeed) + (lateralVelocity * (1f - lateralGrip)) + verticalVelocity;

            float steeringScale = Mathf.Lerp(0.25f, 1f, Mathf.InverseLerp(0f, 2f, Mathf.Abs(forwardSpeed)));
            float direction = forwardSpeed < -0.05f ? -1f : 1f;
            float rotation = input.Steering * maximumSteerRateDegrees * steeringScale * direction * deltaTime;
            body.MoveRotation(body.rotation * Quaternion.Euler(0f, rotation, 0f));

            LongitudinalAcceleration = (forwardSpeed - speedBefore) / Mathf.Max(0.001f, deltaTime);
            previousForwardSpeed = forwardSpeed;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.isTrigger)
            {
                return;
            }

            ContactPoint contact = collision.contactCount > 0 ? collision.GetContact(0) : default;
            bool floorLikeContact = collision.contactCount > 0 && Mathf.Abs(contact.normal.y) > 0.7f;
            if (floorLikeContact && collision.collider.gameObject.layer != 8)
            {
                return;
            }

            body.linearVelocity *= 0.32f;
            string objectId = collision.collider.transform.root.name;
            Vector3 contactPoint = collision.contactCount > 0 ? contact.point : transform.position;
            safetyMonitor?.RegisterCollision(objectId, contactPoint, collision.relativeVelocity.magnitude);
        }

        public void ResetForTrial(int seed)
        {
            EnsureComponents();
            body.position = spawnPosition;
            body.rotation = spawnRotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            previousForwardSpeed = 0f;
            LongitudinalAcceleration = 0f;
            cameraYaw = 0f;
            cameraPitch = 0f;
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.identity;
            }
            headPoseDriver?.ResetForTrial(seed);
        }

        public void SetGameplayFocus(bool focused)
        {
            inputSource?.SetGameplayFocus(focused);
        }

        private void UpdateView()
        {
            if (cameraPivot == null)
            {
                return;
            }

            cameraYaw += input.LookDelta.x;
            cameraPitch = Mathf.Clamp(cameraPitch - input.LookDelta.y, -verticalLookLimit, verticalLookLimit);
            cameraPivot.localRotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);
        }

        private void EnsureComponents()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
                body.mass = 95f;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            }
        }
    }
}
