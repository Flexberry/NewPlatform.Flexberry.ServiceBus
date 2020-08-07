namespace NewPlatform.Flexberry.ServiceBus.WinServiceHost
{
    using System.Linq;
    using System.ServiceProcess;

    using ICSSoft.Services;

    using NewPlatform.Flexberry.ServiceBus.Components;

    using Unity;

    public partial class WinService : ServiceBase
    {
        private readonly ServiceBus _serviceBus;

        public WinService()
        {
            InitializeComponent();

            var unityContainer = UnityFactory.GetContainer();

            var sbComponents =
                from registration in unityContainer.Registrations
                where typeof(IServiceBusComponent).IsAssignableFrom(registration.MappedToType)
                select (IServiceBusComponent)unityContainer.Resolve(registration.RegisteredType, registration.Name);

            var serviceBusSettings = new ServiceBusSettings
            {
                Components = sbComponents.ToList()
            };

            _serviceBus = new ServiceBus(serviceBusSettings, unityContainer.Resolve<ILogger>());
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
