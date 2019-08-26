namespace NewPlatform.Flexberry.ServiceBus.MultiTasking
{
    using System;
    using System.Threading.Tasks;

    public class AsyncPeriodicalTimer : BasePeriodicalTimer
    {
        private Func<Task> _callback;

        protected override async void DoCicling(object param)
        {
            var milliseconds = (int)param;
            do
            {
                await _callback();
            } while (!CloseEvent.WaitOne(TimeSpan.FromMilliseconds(milliseconds)));
            State = PeriodicalTimerState.Stopped;
        }

        public override void TimerAction()
        {
        }

        public void Start(Func<Task> callback, int milliseconds)
        {
            _callback = callback;
            base.Start(milliseconds);
        }

        public void TryStart(Func<Task> callback, int milliseconds)
        {
            _callback = callback;
            base.TryStart(milliseconds);
        }
    }
}
