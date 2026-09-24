using UnityEngine;

namespace MobilityLabVR
{
    public sealed class TrafficLightVisual : MonoBehaviour
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private TrafficSignalController controller;
        private RoadAxis axis;
        private Renderer redLens;
        private Renderer yellowLens;
        private Renderer greenLens;

        public void Configure(
            TrafficSignalController signalController,
            RoadAxis controlledAxis,
            Renderer red,
            Renderer yellow,
            Renderer green)
        {
            controller = signalController;
            axis = controlledAxis;
            redLens = red;
            yellowLens = yellow;
            greenLens = green;
            controller.PhaseChanged += OnPhaseChanged;
            Refresh();
        }

        private void OnDestroy()
        {
            if (controller != null) controller.PhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(SignalPhase phase)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (controller == null) return;
            RoadSignalState state = controller.GetVehicleState(axis);
            SetLens(redLens, new Color(1f, 0.08f, 0.05f), state == RoadSignalState.Red);
            SetLens(yellowLens, new Color(1f, 0.65f, 0.04f), state == RoadSignalState.Yellow);
            SetLens(greenLens, new Color(0.05f, 1f, 0.38f), state == RoadSignalState.Green);
        }

        private static void SetLens(Renderer lens, Color color, bool active)
        {
            if (lens == null) return;
            Color visible = active ? color : color * 0.12f;
            Material material = lens.material;
            material.color = visible;
            if (material.HasProperty(EmissionColorId))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor(EmissionColorId, active ? color * 3.2f : Color.black);
            }
        }
    }
}
