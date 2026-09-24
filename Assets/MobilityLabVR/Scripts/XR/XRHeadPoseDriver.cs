using UnityEngine;
using UnityEngine.XR;

namespace MobilityLabVR
{
    /// <summary>
    /// Applies the connected HMD pose relative to its pose at trial start.
    /// Desktop cameras remain unchanged when no XR head device is present.
    /// </summary>
    public sealed class XRHeadPoseDriver : MonoBehaviour, ITrialResettable
    {
        private InputDevice headDevice;
        private Vector3 originPosition;
        private Quaternion originRotation = Quaternion.identity;
        private bool originCaptured;

        private void LateUpdate()
        {
            if (!headDevice.isValid) headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            if (!headDevice.isValid ||
                !headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position) ||
                !headDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation))
            {
                return;
            }

            if (!originCaptured)
            {
                originPosition = position;
                originRotation = rotation;
                originCaptured = true;
            }

            transform.localPosition = position - originPosition;
            transform.localRotation = Quaternion.Inverse(originRotation) * rotation;
        }

        public void ResetForTrial(int seed)
        {
            originCaptured = false;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
}
