using UnityEngine;

namespace MobilityLabVR
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ScenarioTriggerZone : MonoBehaviour
    {
        private ScenarioManager manager;
        private bool triggered;

        public void Configure(ScenarioManager scenarioManager)
        {
            manager = scenarioManager;
            BoxCollider trigger = GetComponent<BoxCollider>();
            trigger.isTrigger = true;
        }

        public void ResetZone()
        {
            triggered = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggered || manager == null || other.GetComponentInParent<ScooterController>() == null)
            {
                return;
            }

            triggered = true;
            manager.ActivateScenarioHazard();
        }
    }
}
