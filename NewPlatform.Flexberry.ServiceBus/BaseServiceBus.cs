using System;
using NewPlatform.Flexberry.ServiceBus.Components;
using NewPlatform.Flexberry.ServiceBus.Exceptions;

namespace NewPlatform.Flexberry.ServiceBus
{
    public abstract class BaseServiceBus<T> : IServiceBus
        where T : IServiceBusSettings
    {
        protected BaseServiceBus(ILogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected T Settings { get; private set; }

        protected ILogger Logger { get; }

        public ServiceBusState State { get; protected set; } = ServiceBusState.NotStarted;

        public void Init(IServiceBusSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.Components == null)
                throw new ServiceBusComponentsNotSetException();

            if (settings is T typedSettings)
            {
                Settings = typedSettings;
                InitSettings(typedSettings);
            }
            else
                throw new ServiceBusSettingsInvalidTypeException(typeof(T), Settings.GetType());

        }

        protected abstract void InitSettings(T settings);

        public void Start()
        {
            Logger.LogDebugMessage(nameof(ServiceBus), "Starting service bus");

            try
            {
                StartService();

                State = ServiceBusState.Started;
                Logger.LogDebugMessage(nameof(ServiceBus), "Started successfully");
            }
            catch (Exception ex)
            {
                Logger.LogUnhandledException(ex);
                throw;
            }
        }

        public void Stop()
        {
            if (State != ServiceBusState.Started)
                throw new InvalidOperationException("Wrong state");

            Logger.LogDebugMessage(nameof(ServiceBus), "Stopping service bus");

            StopService();

            State = ServiceBusState.Stopped;
        }

        void IDisposable.Dispose()
        {
            if (State == ServiceBusState.Started)
            {
                Stop();
            }
        }

        protected abstract void StartService();

        protected abstract void StopService();
    }
}
