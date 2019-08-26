namespace NewPlatform.Flexberry.ServiceBus
{
    using System.Linq;

    using Components;
    using Microsoft.Practices.Unity.Configuration;
    using Unity;
    using Unity.Exceptions;

    public static class ServiceBusCreator
    {
        /// <summary>
        /// Создание запускаемого экземпляра сервиса шины.
        /// </summary>
        /// <param name="unityContainer">Контейнер unity для инициализации компонентов.</param>
        /// <returns>Экземпляр сервиса шины.</returns>
        public static IServiceBus CreateServiceBus(IUnityContainer unityContainer = null)
        {
            if (unityContainer == null)
            {
                unityContainer = new UnityContainer();
                unityContainer.LoadConfiguration();
            }

            var components =
                from registration in unityContainer.Registrations
                where typeof(IServiceBusComponent).IsAssignableFrom(registration.MappedToType)
                select (IServiceBusComponent)unityContainer.Resolve(registration.RegisteredType, registration.Name);

            var serviceBusSettings = new ServiceBusSettings
            {
                Components = components.ToList()
            };

            IServiceBus serviceBus = null;
            try
            {
                serviceBus = unityContainer.Resolve<IServiceBus>();
            }
            catch (ResolutionFailedException)
            {
                // Если в Unity не объявлена зависимость для IServiceBus, то действуем по старой схеме
                // для совместимости с конфигурациями предыдущих версий.
            }

            if (serviceBus == null)
            {
                serviceBus = new ServiceBus(unityContainer.Resolve<ILogger>());
            }

            serviceBus.Init(serviceBusSettings);

            return serviceBus;
        }
    }
}
