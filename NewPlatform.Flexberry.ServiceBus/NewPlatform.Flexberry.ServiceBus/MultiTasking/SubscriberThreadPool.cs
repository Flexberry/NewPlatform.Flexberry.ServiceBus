namespace NewPlatform.Flexberry.ServiceBus.MultiTasking
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Components;
    using ICSSoft.STORMNET.Business;

    /// <summary>
    /// Класс для работы с потоками подписчиков
    /// </summary>
    public static class SubscriberThreadPool
    {
        #region Static Fields

        /// <summary>
        /// The thread pool.
        /// </summary>
        private static readonly ConcurrentDictionary<Guid, SubscriptionThread> ThreadPool = new ConcurrentDictionary<Guid, SubscriptionThread>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Прерывает поток подписчика, а также удаляет его из пула.
        /// </summary>
        /// <param name="subscriptionGuidObj">
        /// Guid подписки.
        /// </param>
        /// <returns>
        /// Возвращает <see cref="bool"/>: true, если успешно завершена работа с данным подписчиком
        /// </returns>
        public static bool ReleaseSubscriber(object subscriptionGuidObj)
        {
            var subscriptionGuid = (Guid)subscriptionGuidObj;

            if (!ThreadPool.ContainsKey(subscriptionGuid))
            {
                return false;
            }

            var subcsrThread = ThreadPool[subscriptionGuid];
            if (!subcsrThread.IsExecuting)
            {
                return false;
            }

            subcsrThread.WaitForHandlingAndStop();

            return true;
        }

        /// <summary>
        /// Завершить работу всех потоков подписчиков
        /// </summary>
        public static void ReleaseAllSubscriptionThreads()
        {
            List<Task> tasks = new List<Task>();

            foreach (var subscriptionThread in ThreadPool.Keys)
            {
                tasks.Add(Task<bool>.Factory.StartNew(ReleaseSubscriber, subscriptionThread));
            }

            Task.WaitAll(tasks.ToArray());
        }

        /// <summary>
        /// Выделяет и исполняет новый поток для подписчика в случае, если данный поток ещё не создан или не выполняется.
        /// </summary>
        /// <param name="subscription">Подписка, для которой создается поток.</param>
        /// <param name="waitScanDB4CallBackHandle">Элемент, который разбудит создаваемый поток, если нужно будет закончить работу.</param>
        /// <returns>Успешно ли запущен поток.</returns>
        public static bool QueueUserWorkItem(Subscription subscription, ManualResetEvent waitScanDB4CallBackHandle, IStatisticsService statistics, IDataService dataService, ILogger logger)
        {
            var subscriptionGuid = new Guid(subscription.__PrimaryKey.ToString());

            if (!ThreadPool.ContainsKey(subscriptionGuid) || !ThreadPool[subscriptionGuid].IsExecuting)
            {
                var subscriptionThread = new SubscriptionThread(waitScanDB4CallBackHandle, statistics, dataService, logger)
                    {
                        Subscription = subscription
                    };
                ThreadPool[subscriptionGuid] = subscriptionThread;
                subscriptionThread.StartExecution();
                return true;
            }

            return false;
        }

        /// <summary>
        /// The subscriber thread exists.
        /// </summary>
        /// <param name="subscribe">
        /// The subscription.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool SubscriberThreadExists(Subscription subscribe)
        {
            var subscriptionGuid = new Guid(subscribe.__PrimaryKey.ToString());
            return ThreadPool.ContainsKey(subscriptionGuid);
        }

        /// <summary>
        /// Удаляет неактивные потоки подписок.
        /// </summary>
        public static void CheckForUnActiveSubscribers()
        {
            IEnumerable<Guid> subscribersToStop = ThreadPool.Where(x => !x.Value.IsExecuting).Select(x => x.Key);

            // Останавливаем и удаляем все неактивные подписки.
            foreach (Guid subscriber in subscribersToStop)
            {
                SubscriptionThread outVal;
                ThreadPool.TryRemove(subscriber, out outVal);
            }
        }

        /// <summary>
        /// Удаление из пула потоков-подписчиков потока определённого подписчика
        /// </summary>
        /// <param name="subscriptions">
        /// Подписка, которую необходимо удалить
        /// </param>
        public static void RemoveSubscriberFromPool(Subscription subscriptions)
        {
            SubscriptionThread outVal;
            ThreadPool.TryRemove(new Guid(subscriptions.__PrimaryKey.ToString()), out outVal);
        }

        #endregion
    }
}