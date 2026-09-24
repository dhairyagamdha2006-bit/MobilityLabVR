using UnityEngine;

namespace MobilityLabVR
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class DestinationZone : MonoBehaviour
    {
        private ExperimentManager experiment;
        private bool reached;

        public void Configure(ExperimentManager experimentManager)
        {
            experiment = experimentManager;
            GetComponent<BoxCollider>().isTrigger = true;
        }

        public void ResetZone()
        {
            reached = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (reached || experiment == null || other.GetComponentInParent<ScooterController>() == null)
            {
                return;
            }

            reached = true;
            experiment.CompleteTrial();
        }
    }
}
