namespace NewPlatform.Flexberry.ServiceBus.Components.ObjectRepository
{
    using System;
    using System.Diagnostics;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.Windows.Forms;

    public static class CommonMetodsObjectRepository
    {
        /// <summary>
        /// Create sending permission.
        /// </summary>
        /// <param name="clientId">Client's ID.</param>
        /// <param name="messageTypeId">Message type's ID.</param>
        /// <param name="dataService">The data service for loading objects.</param>
        /// <param name="statisticsService">Statistics service.</param>
        public static void CreateSendingPermission(string clientId, string messageTypeId, IDataService dataService, IStatisticsService statisticsService)
        {
            Guid primaryKeyClient = ServiceHelper.ConvertClientIdToPrimaryKey(clientId, dataService, statisticsService);
            Client currentClient = ServiceHelper.GetClient(primaryKeyClient, dataService, statisticsService);

            Guid primaryKeyMessageType = ServiceHelper.ConvertMessageTypeIdToPrimaryKey(messageTypeId, dataService, statisticsService);
            MessageType currentMessageType = ServiceHelper.GetMessageType(primaryKeyMessageType, dataService, statisticsService);

            SendingPermission currentSendingPermission = new SendingPermission { Client = currentClient, MessageType = currentMessageType };
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            dataService.UpdateObject(currentSendingPermission);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSql(null, (int)time, "CommonMetodsObjectRepository.CreateSendingPermission() update sendingPermission.");
        }

        /// <summary>
        /// Delete sending permission.
        /// </summary>
        /// <param name="clientId">Client's ID.</param>
        /// <param name="messageTypeId">Message type's ID.</param>
        /// <param name="dataService">The data service for loading objects.</param>
        /// <param name="statisticsService">Statistics service.</param>
        public static void DeleteSendingPermission(string clientId, string messageTypeId, IDataService dataService, IStatisticsService statisticsService)
        {
            ExternalLangDef langDef = ExternalLangDef.LanguageDef;
            LoadingCustomizationStruct lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(SendingPermission), SendingPermission.Views.ServiceBusView);
            lcs.LimitFunction = langDef.GetFunction(
                langDef.funcAND,
                langDef.GetFunction(
                    langDef.funcEQ,
                    new VariableDef(langDef.StringType, Information.ExtractPropertyPath<SendingPermission>(x => x.Client.ID)),
                    clientId),
                langDef.GetFunction(
                    langDef.funcEQ,
                    new VariableDef(langDef.StringType, Information.ExtractPropertyPath<SendingPermission>(x => x.MessageType.ID)),
                    messageTypeId));
            lcs.ReturnTop = 1;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            DataObject[] currentSendingPermission = dataService.LoadObjects(lcs);

            stopwatch.Stop();
            long time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSql(null, (int)time, "CommonMetodsObjectRepository.DeleteSendingPermission() load sendingPermission.");

            currentSendingPermission[0].SetStatus(ObjectStatus.Deleted);

            stopwatch = new Stopwatch();
            stopwatch.Start();

            dataService.UpdateObjects(ref currentSendingPermission);

            stopwatch.Stop();
            time = stopwatch.ElapsedMilliseconds;
            statisticsService.NotifyAvgTimeSql(null, (int)time, "CommonMetodsObjectRepository.DeleteSendingPermission() update sendingPermission.");
        }
    }
}
