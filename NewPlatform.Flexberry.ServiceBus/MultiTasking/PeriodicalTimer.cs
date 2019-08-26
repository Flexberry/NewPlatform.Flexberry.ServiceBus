namespace NewPlatform.Flexberry.ServiceBus.MultiTasking
{
    using System;

    /// <summary>
    /// Thread-safe and nonblocking timer for repeatative event.
    /// </summary>
    internal class PeriodicalTimer : BasePeriodicalTimer
    {
        /// <summary>
        /// Callback function. This function will be called periodicaly when component is started.
        /// </summary>
        private Action callback;

        /// <summary>
        /// Start periodical processing.
        /// </summary>
        /// <param name="callback">Callback function. This function will be called periodicaly when component is started.</param>
        /// <param name="milliseconds">Period of timer's callback calls in milliseconds.</param>
        public void Start(Action callback, int milliseconds)
        {
            this.callback = callback;
            base.Start(milliseconds);
        }

        /// <summary>
        /// Check the current state and start periodical processing if needed.
        /// </summary>
        /// <param name="callback">Callback function. This function will be called periodicaly when component is started.</param>
        /// <param name="milliseconds">Period of timer's callback calls in milliseconds.</param>
        public void TryStart(Action callback, int milliseconds)
        {
            this.callback = callback;
            base.TryStart(milliseconds);
        }

        public override void TimerAction()
        {
            if (callback == null) return;

            callback();
        }
    }
}
