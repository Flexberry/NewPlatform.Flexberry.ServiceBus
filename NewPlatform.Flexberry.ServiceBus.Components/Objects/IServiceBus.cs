using System;

namespace NewPlatform.Flexberry.ServiceBus
{
    /// <summary>
    /// Интерфейс, описывающий методы и свойства шины.
    /// </summary>
    public interface IServiceBus : IDisposable
    {
        ServiceBusState State { get; }

        /// <summary>
        /// Метод инициализации шины перед запуском.
        /// </summary>
        /// <param name="settings">Настройки шины.</param>
        void Init(IServiceBusSettings settings);

        /// <summary>
        /// Запуск.
        /// </summary>
        void Start();

        /// <summary>
        /// Остановка.
        /// </summary>
        void Stop();
    }
}
