namespace NewPlatform.Flexberry.ServiceBus.ConsoleHost
{
    using System;
    using System.Linq;

    using Microsoft.Practices.Unity;
    using Microsoft.Practices.Unity.Configuration;

    using NewPlatform.Flexberry.ServiceBus.Components;

    /// <summary>
    /// Консольное приложение для запуска сервисов ServiceBus.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The entry point of the application.
        /// </summary>
        public static void Main()
        {
            var unityContainer = new UnityContainer();
            unityContainer.LoadConfiguration();

            var components = 
                from registration in unityContainer.Registrations
                where typeof(IServiceBusComponent).IsAssignableFrom(registration.MappedToType)
                select (IServiceBusComponent)unityContainer.Resolve(registration.RegisteredType, registration.Name);
            
            var serviceBusSettings = new ServiceBusSettings
            {
                Components = components.ToList()
            };

            var serviceBus = new ServiceBus(serviceBusSettings, unityContainer.Resolve<ILogger>());
            serviceBus.Start();

            Console.WriteLine("Service Bus started.\nPress any key to shutdown...");
            Console.ReadKey();

            serviceBus.Stop();
            Environment.Exit(0);
        }
    }
}
