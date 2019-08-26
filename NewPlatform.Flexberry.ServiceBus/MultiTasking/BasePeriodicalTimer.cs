namespace NewPlatform.Flexberry.ServiceBus.MultiTasking
{
    using System;
    using System.Threading;

    /// <summary>
    /// Base abstract implementation of <see cref="IPeriodicalTimer"/>.
    /// </summary>
    public abstract class BasePeriodicalTimer : IPeriodicalTimer
    {
        /// <summary>
        /// Shutdown event.
        /// </summary>
        protected AutoResetEvent CloseEvent = new AutoResetEvent(false);

        /// <summary>
        /// Current timer state.
        /// </summary>
        public PeriodicalTimerState State { get; protected set; }

        /// <summary>
        /// Start periodical processing.
        /// </summary>
        /// <param name="milliseconds">Period of timer's callback calls in milliseconds.</param>
        public virtual void Start(int milliseconds)
        {
            var thread = new Thread(DoCicling);
            thread.Start(milliseconds);
            State = PeriodicalTimerState.Working;
        }

        /// <summary>
        /// Check the current state and start periodical processing if needed.
        /// </summary>
        /// <param name="milliseconds">Period of timer's callback calls in milliseconds.</param>
        public virtual void TryStart(int milliseconds)
        {
            if (State != PeriodicalTimerState.Working)
                Start(milliseconds);
        }

        /// <summary>
        /// Stop periodical processing.
        /// </summary>
        public virtual void Stop()
        {
            State = PeriodicalTimerState.Stoping;
            CloseEvent.Set();
        }

        /// <summary>
        /// Check the current state and stop periodical processing if needed.
        /// </summary>
        public virtual void TryStop()
        {
            if (State == PeriodicalTimerState.Working)
                Stop();
        }

        /// <summary>
        /// Calls <see cref="TimerAction"/> method periodicaly.
        /// </summary>
        /// <param name="param">Interval in milliseconds.</param>
        protected virtual void DoCicling(object param)
        {
            var milliseconds = (int)param;
            do
            {
                TimerAction();
            } while (!CloseEvent.WaitOne(TimeSpan.FromMilliseconds(milliseconds)));
            State = PeriodicalTimerState.Stopped;
        }

        /// <summary>
        /// Abstract method containing a periodic action.
        /// </summary>
        public abstract void TimerAction();
    }
}
