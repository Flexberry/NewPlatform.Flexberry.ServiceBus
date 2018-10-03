namespace NewPlatform.Flexberry.ServiceBus.MultiTasking
{
    using System;
    using System.Threading;

    /// <summary>
    /// Thread-safe and nonblocking timer for repeatative event.
    /// </summary>
    internal class PeriodicalTimer
    {
        /// <summary>
        /// States of current timer.
        /// </summary>
        public enum TimerState
        {
            /// <summary>
            /// Periodical processing stopped.
            /// </summary>
            Stopped,

            /// <summary>
            /// Stoping periodical processing.
            /// </summary>
            Stoping,

            /// <summary>
            /// Periodical processing in progress.
            /// </summary>
            Working,
        }

        /// <summary>
        /// Shutdown event.
        /// </summary>
        private AutoResetEvent closeEvent = new AutoResetEvent(false);

        /// <summary>
        /// Callback function. This function will be called periodicaly when component is started.
        /// </summary>
        private Action callback;

        /// <summary>
        /// Current timer state.
        /// </summary>
        public TimerState State { get; private set; }

        /// <summary>
        /// Start periodical processing.
        /// </summary>
        /// <param name="callback">Callback function. This function will be called periodicaly when component is started.</param>
        /// <param name="milliseconds">Period of timer's callback calls in milliseconds.</param>
        public void Start(Action callback, int milliseconds)
        {
            this.callback = callback;
            var thread = new Thread(DoCicling);
            thread.Start(milliseconds);
            State = TimerState.Working;
        }

        /// <summary>
        /// Stop periodical processing.
        /// </summary>
        public void Stop()
        {
            closeEvent.Set();
            State = TimerState.Stoping;
        }

        /// <summary>
        /// Calls callback method periodicaly.
        /// </summary>
        /// <param name="param">Interval in milliseconds.</param>
        private void DoCicling(object param)
        {
            var milliseconds = (int)param;
            do
            {
                callback();
            } while (!closeEvent.WaitOne(TimeSpan.FromMilliseconds(milliseconds)));
            State = TimerState.Stopped;
        }
    }
}
