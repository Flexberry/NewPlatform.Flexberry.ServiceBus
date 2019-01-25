namespace NewPlatform.Flexberry.ServiceBus
{
    using System;
    using System.Linq;
    using Components;

    public sealed class ServiceBus : IDisposable
    {
        private enum State
        {
            NotStarted,

            Started,

            Stopped
        }

        private readonly ServiceBusSettings _settings;

        private readonly ILogger _logger;

        private State _state = State.NotStarted;

        public ServiceBus(ServiceBusSettings settings, ILogger logger)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.Components == null)
                throw new ArgumentException(nameof(settings));

            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            _settings = settings;
            _logger = logger;
        }

        public void Start()
        {
            if (_state != State.NotStarted)
                throw new InvalidOperationException("Wrong state");

            _logger.LogDebugMessage(nameof(ServiceBus), "Starting service bus");

            try
            {
                foreach (IServiceBusComponent component in _settings.Components)
                {
                    _logger.LogDebugMessage(nameof(ServiceBus), $"Preparing module {component.GetType().FullName}");
                    component.Prepare();
                }

                foreach (IServiceBusComponent component in _settings.Components)
                {
                    _logger.LogDebugMessage(nameof(ServiceBus), $"Starting module {component.GetType().FullName}");
                    component.Start();
                }

                _logger.LogDebugMessage(nameof(ServiceBus), "Started successfully");

                _state = State.Started;
            }
            catch (Exception ex)
            {
                _logger.LogUnhandledException(ex);
                throw;
            }
        }

        public void Stop()
        {
            if (_state != State.Started)
                throw new InvalidOperationException("Wrong state");

            _logger.LogDebugMessage(nameof(ServiceBus), "Stopping service bus");

            foreach (var component in _settings.Components)
            {
                try
                {
                    _logger.LogDebugMessage(nameof(ServiceBus), $"Stopping module {component.GetType().FullName}");
                    component.Stop();
                }
                catch (Exception ex)
                {
                    _logger.LogUnhandledException(ex);
                }
            }

            foreach (var component in _settings.Components)
            {
                try
                {
                    _logger.LogDebugMessage(nameof(ServiceBus), $"Executing \"after stop\" action for module {component.GetType().FullName}");
                    component.AfterStop();
                }
                catch (Exception ex)
                {
                    _logger.LogUnhandledException(ex);
                }
            }

            foreach (var component in _settings.Components.OfType<IDisposable>())
            {
                try
                {
                    component.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogUnhandledException(ex);
                }
            }

            _state = State.Stopped;
        }

        public void Dispose()
        {
            if (_state == State.Started)
            {
                Stop();
            }
        }
    }
}
