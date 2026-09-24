namespace MobilityLabVR
{
    /// <summary>
    /// Centralized monotonic trial time. Tests can use the pure elapsed value
    /// without depending on wall-clock time.
    /// </summary>
    public sealed class SimulationClock
    {
        public float ElapsedSeconds { get; private set; }
        public bool IsRunning { get; private set; }

        public void Start()
        {
            ElapsedSeconds = 0f;
            IsRunning = true;
        }

        public void Tick(float unscaledDeltaTime)
        {
            if (IsRunning && unscaledDeltaTime > 0f)
            {
                ElapsedSeconds += unscaledDeltaTime;
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Reset()
        {
            ElapsedSeconds = 0f;
            IsRunning = false;
        }
    }
}
