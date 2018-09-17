using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NewPlatform.Flexberry.ServiceBus.Components.SubscriptionsSynchronizer
{
    /// <summary>
    /// Класс для синхронизации подписок в MQ и шине.
    /// </summary>
    internal class RmqSubscriptionsSynchronizer : BaseServiceBusComponent, ISubscriptionSynchronizer
    {
        private readonly ILogger _logger;
        private readonly ISubscriptionsManager _esbSubscriptionsManager;
        private readonly ISubscriptionsManager _mqSubscriptionsManager;

        /// <summary>
        /// Частота запуска синхронизации подписок.
        /// </summary>
        public int UpdatePeriodMilliseconds { get; set; } = 30 * 1000;

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="RmqSubscriptionsSynchronizer"/>.
        /// </summary>
        /// <param name="logger">Используемый компонент логирования.</param>
        /// <param name="esbSubscriptionsManager">Менеджер подписок шины.</param>
        /// <param name="mqSubscriptionsManager">Менеджер подписок </param>
        public RmqSubscriptionsSynchronizer(ILogger logger, ISubscriptionsManager esbSubscriptionsManager, ISubscriptionsManager mqSubscriptionsManager)
        {
            this._logger = logger;
            this._esbSubscriptionsManager = esbSubscriptionsManager;
            this._mqSubscriptionsManager = mqSubscriptionsManager;
        }

        private Timer _syncTimer;

        public void Start()
        {
            this._syncTimer = new Timer(x => this.Sync(), null, this.UpdatePeriodMilliseconds, this.UpdatePeriodMilliseconds);
        }

        public void Stop()
        {
            this._syncTimer.Dispose();
        }

        /// <summary>
        /// Цикл синхронизации подписок.
        /// </summary>
        private void Sync()
        {
            try
            {
                var mqSubscriptions = this._mqSubscriptionsManager.GetSubscriptions().ToList();
                var esbSubscriptions = this._esbSubscriptionsManager.GetSubscriptions().ToList();

                // Сначала актуализируем подписки в брокере, считаем его ведущим по данным
                this.UpdateMqSubscriptions(mqSubscriptions, esbSubscriptions);
                this.UpdateEsbSubscriptions(mqSubscriptions, esbSubscriptions);
            }
            catch (Exception e)
            {
                this._logger.LogError("Ошибка при синхронизации подписок шины и RabbitMQ", e.ToString());
            }

        }

        /// <summary>
        /// Актуализация подписок в RabbitMQ.
        /// На данный момент реализовано только копирование подписок из шины в RabbitMQ.
        /// </summary>
        /// <param name="mqSubscriptions">Список текущих подписок в RabbitMQ.</param>
        /// <param name="esbSubscriptions">Список текущих подписок в шине.</param>
        public void UpdateMqSubscriptions(List<Subscription> mqSubscriptions, List<Subscription> esbSubscriptions)
        {
            foreach (var esbSubscription in esbSubscriptions)
            {
                // Если подписки нет, создаём
                if (!mqSubscriptions.Any(x => this.IsSubscriptionEquals(esbSubscription, x)))
                {
                    this._mqSubscriptionsManager.SubscribeOrUpdate(esbSubscription.Client.ID, esbSubscription.MessageType.ID, false, null);
                }

                // TODO: подумать об изменении и удалении подписок
            }
        }

        /// <summary>
        /// Актуализация подпсок в шине.
        /// На данный момент реализовано только копирование подписок из RabbitMQ в шину.
        /// </summary>
        /// <param name="mqSubscriptions">Подписки RabbitMQ.</param>
        /// <param name="esbSubscriptions">Подписки шины.</param>
        public void UpdateEsbSubscriptions(List<Subscription> mqSubscriptions, List<Subscription> esbSubscriptions)
        {
            foreach (var mqSubscription in mqSubscriptions)
            {
                if (!esbSubscriptions.Any(x => this.IsSubscriptionEquals(mqSubscription, x)))
                {
                    this._esbSubscriptionsManager.CreateMessageType(new NameCommentStruct()
                    {
                        Name = mqSubscription.Client.ID,
                        Id = mqSubscription.Client.ID,
                        Comment = "Подписка создана автоматически при синхронизации подписок"
                    });
                    this._esbSubscriptionsManager.CreateClient(mqSubscription.Client.ID, mqSubscription.Client.Name);

                    this._esbSubscriptionsManager.SubscribeOrUpdate(mqSubscription.Client.ID, mqSubscription.MessageType.ID, false, null, DateTime.MaxValue);
                }

                // TODO: подумать об изменении и удалении подписок
            }
        }

        private bool IsSubscriptionEquals(Subscription sub1, Subscription sub2)
        {
            return sub1.Client.ID == sub2.Client.ID && sub1.MessageType.ID == sub2.MessageType.ID;
        }
    }
}
