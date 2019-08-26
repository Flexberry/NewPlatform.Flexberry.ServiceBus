namespace NewPlatform.Flexberry.ServiceBus.WinServiceHost
{
    using System.ServiceProcess;

    public partial class WinService : ServiceBase
    {
        private readonly IServiceBus _serviceBus;

        public WinService()
        {
            InitializeComponent();

            _serviceBus = ServiceBusCreator.CreateServiceBus();
        }

        protected override void OnStart(string[] args)
        {
            _serviceBus.Start();
        }

        protected override void OnStop()
        {
            _serviceBus.Stop();
        }
    }
}
