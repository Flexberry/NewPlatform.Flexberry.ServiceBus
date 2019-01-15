using EasyNetQ.Management.Client;
using EasyNetQ.Management.Client.Model;
using ICSSoft.STORMNET.Business;
using ICSSoft.STORMNET.Business.LINQProvider;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    /// <summary>
    /// Класс для синхронизации подписок в MQ и шине.
    /// </summary>
    public class RmqSubscriptionsSynchronizer : BaseServiceBusComponent, ISubscriptionSynchronizer
    {
        private readonly ILogger _logger;
        private readonly ISubscriptionsManager _esbSubscriptionsManager;
        private readonly ISubscriptionsManager _mqSubscriptionsManager;
        private readonly IDataService _dataService;
        private readonly IManagementClient _managementClient;
        private readonly AmqpNamingManager _namingManager;
        private readonly string _vhostStr;
        private Vhost _vhost;

        /// <summary>
        /// Gets Vhost RabbitMq.
        /// </summary>
        public Vhost Vhost
        {
            get
            {
                if (_vhost == null)
                {
                    _vhost = this._managementClient.CreateVirtualHostAsync(_vhostStr).Result;
                }
                return _vhost;
            }
        }

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
        public RmqSubscriptionsSynchronizer(ILogger logger, ISubscriptionsManager esbSubscriptionsManager, ISubscriptionsManager mqSubscriptionsManager, IDataService dataService, IManagementClient managementClient, string vhost = "/")
        {
            this._logger = logger;
            this._esbSubscriptionsManager = esbSubscriptionsManager;
            this._mqSubscriptionsManager = mqSubscriptionsManager;
            this._dataService = dataService;
            this._managementClient = managementClient;

            this._namingManager = new AmqpNamingManager();
            this._vhostStr = vhost;
        }

        private Timer _syncTimer;

        public void Start()
        {
            this._syncTimer = new Timer(x => this.Sync(), null, 0, this.UpdatePeriodMilliseconds);
        }

        public void Stop()
        {
            this._syncTimer.Dispose();
        }

        /// <summary>
        /// Цикл синхронизации подписок.
        /// </summary>
        public void Sync()
        {
            try
            {
                var mqSubscriptions = this._mqSubscriptionsManager.GetSubscriptions().ToList();
                var esbSubscriptions = this._esbSubscriptionsManager.GetSubscriptions().ToList();

                // Сначала актуализируем подписки в брокере, считаем его ведомым по данным
                this.UpdateMqSubscriptions(mqSubscriptions, esbSubscriptions);
                this.UpdateEsbSubscriptions(mqSubscriptions, esbSubscriptions);

                this.SynchronizeSendingPermissions();
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
                    _logger.LogDebugMessage("Subscription synchronizatrion",
                        $"Created subscription in broker for {esbSubscription.Client.ID} {esbSubscription.MessageType.ID}");
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
                    this._esbSubscriptionsManager.CreateMessageType(new ServiceBusMessageType()
                    {
                        Name = mqSubscription.Client.ID,
                        ID = mqSubscription.Client.ID,
                        Description = "Подписка создана автоматически при синхронизации подписок"
                    });
                    this._esbSubscriptionsManager.CreateClient(mqSubscription.Client.ID, mqSubscription.Client.Name);

                    this._esbSubscriptionsManager.SubscribeOrUpdate(mqSubscription.Client.ID, mqSubscription.MessageType.ID, false, null, DateTime.MaxValue);
                    _logger.LogDebugMessage("Subscription synchronizatrion",
                        $"Created subscription in esb storage for {mqSubscription.Client.ID} {mqSubscription.MessageType.ID}");
                }

                // TODO: подумать об изменении и удалении подписок
            }
        }

        private bool IsSubscriptionEquals(Subscription sub1, Subscription sub2)
        {
            return sub1.Client.ID == sub2.Client.ID && sub1.MessageType.ID == sub2.MessageType.ID;
        }

        /// <summary>
        /// Актуализация разрешений в RabbitMQ.
        /// </summary>
        /// <param name="clientId">ID клиента для актуализации разрешений.</param>
        public void SynchronizeSendingPermissions(string clientId = null)
        { 
            if (string.IsNullOrEmpty(clientId))
            {
                List<Task> tasks = new List<Task>();
                List<SendingPermission> esbPermissions = _dataService.Query<SendingPermission>(SendingPermission.Views.ServiceBusView).ToList();
                List<string> usersIds = esbPermissions.Select(p => p.Client.ID).Distinct().ToList();
                foreach (string id in usersIds)
                {
                    _mqSubscriptionsManager.CreateClient(id, id);
                    User user = _managementClient.GetUserAsync(id).Result;
                    tasks.Add(SynchronizePermissionsForClient(user, esbPermissions));
                }

                List<Permission> mqPermissions = _managementClient.GetPermissionsAsync().Result.Where(p => !usersIds.Contains(p.User) && p.User != ConfigurationManager.AppSettings["DefaultRmqUserName"]).ToList();

                foreach (Permission mqPermission in mqPermissions)
                {
                    User user = _managementClient.GetUserAsync(mqPermission.User).Result;
                    tasks.Add(_managementClient.CreatePermissionAsync(CreatePermissionInfo(user)));
                }

                Task.WaitAll(tasks.ToArray());
            }
            else
            {
                User user = _managementClient.GetUserAsync(clientId).Result;
                SynchronizePermissionsForClient(user).Wait();
            }
        }

        /// <summary>
        /// Актуализация разрешений в RabbitMQ для конкретного пользователя.
        /// </summary>
        /// <param name="user">Пользователь RabbitMQ.</param>
        /// <param name="esbPermissions">Разрешения из шины.</param>
        /// <returns>Асинхронная операция синхронизации разрешений пользователя.</returns>
        private Task SynchronizePermissionsForClient(User user, List<SendingPermission> esbPermissions = null)
        {
            List<SendingPermission> currentEsbPermissions;
            string clientId = user.Name;
            if (esbPermissions == null)
            {
                currentEsbPermissions = _dataService.Query<SendingPermission>(SendingPermission.Views.ServiceBusView).Where(p => p.Client.ID == clientId).ToList();
            }
            else
            {
                currentEsbPermissions = esbPermissions.Where(p => p.Client.ID == clientId).ToList();
            }

            Permission mqPermission = _managementClient.GetPermissionsAsync().Result.Where(p => p.User == clientId && p.Vhost == Vhost.Name).FirstOrDefault();
            if (currentEsbPermissions.Count > 0)
            {
                List<string> rmqPermissionRegex = new List<string>();
                foreach (SendingPermission esbPermission in currentEsbPermissions)
                {
                    rmqPermissionRegex.Add(_namingManager.GetExchangeName(esbPermission.MessageType.ID));
                }

                if (mqPermission == null)
                {
                    return _managementClient.CreatePermissionAsync(CreatePermissionInfo(user, $"^({string.Join("|", rmqPermissionRegex)})$"));
                }
                else
                {
                    PermissionInfo permissionInfo = CreatePermissionInfo(user, $"^({string.Join("|", rmqPermissionRegex)})$", mqPermission.Read, mqPermission.Configure);
                    return _managementClient.CreatePermissionAsync(permissionInfo);
                }
            }
            else
            {
                if (mqPermission == null)
                {
                    return _managementClient.CreatePermissionAsync(CreatePermissionInfo(user));
                }
                else
                {
                    PermissionInfo permissionInfo = CreatePermissionInfo(user, "^$", mqPermission.Read, mqPermission.Configure);
                    return _managementClient.CreatePermissionAsync(permissionInfo);
                }
            }
        }

        /// <summary>
        /// Создание <see cref="PermissionInfo"/> для разрешения в RabbitMQ.
        /// </summary>
        /// <param name="user">Пользователь RabbitMQ.</param>
        /// <param name="write">Разрешение на отправку сообщений.</param>
        /// <param name="read">Разрешение на чтение сообщений.</param>
        /// <param name="configure">Разрешение возможности конфигурирования.</param>
        /// <returns>Информацию о разрешении.</returns>
        private PermissionInfo CreatePermissionInfo(User user, string write = "^$", string read = "^$", string configure = "^$")
        {
            PermissionInfo permissionInfo = new PermissionInfo(user, Vhost);
            permissionInfo.SetWrite(write);
            permissionInfo.SetRead(read);
            permissionInfo.SetConfigure(configure);

            return permissionInfo;
        }
    }
}
