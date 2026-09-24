using UnityEngine;

namespace MobilityLabVR
{
    [DefaultExecutionOrder(-1000)]
    public sealed class MainMenuBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (GetComponent<MainMenuController>() == null) gameObject.AddComponent<MainMenuController>();
            if (FindAnyObjectByType<Camera>() == null)
            {
                GameObject cameraObject = new GameObject("Menu Camera", typeof(Camera), typeof(AudioListener));
                Camera camera = cameraObject.GetComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = MobilityLabPalette.DeepNavy;
            }
        }
    }
}
