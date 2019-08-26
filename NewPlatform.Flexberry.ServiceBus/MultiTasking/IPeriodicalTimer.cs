namespace NewPlatform.Flexberry.ServiceBus.MultiTasking
{
    /// <summary>
    /// The interface of thread-safe and nonblocking timer for repeatative event.
    /// </summary>
    public interface IPeriodicalTimer
    {
        /// <summary>
        /// Current timer state.
        /// </summary>
        PeriodicalTimerState State { get; }

        /// <summary>
        /// Start periodical processing.
        /// </summary>
        /// <param name="milliseconds">Period of timer's callback calls in milliseconds.</param>
        void Start(int milliseconds);

        /// <summary>
        /// Check the current state and start periodical processing if needed.
        /// </summary>
        /// <param name="milliseconds">Period of timer's callback calls in milliseconds.</param>
        void TryStart(int milliseconds);

        /// <summary>
        /// Stop periodical processing.
        /// </summary>
        void Stop();

        /// <summary>
        /// Check the current state and stop periodical processing if needed.
        /// </summary>
        void TryStop();
    }
}