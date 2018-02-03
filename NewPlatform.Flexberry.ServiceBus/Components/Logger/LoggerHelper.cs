namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Business;
    using ICSSoft.STORMNET.FunctionalLanguage;
    using ICSSoft.STORMNET.Windows.Forms;

    /// <summary>
    /// Класс с общими функциями для логгеров
    /// </summary>
    internal class LoggerHelper
    {
        /// <summary>
        /// Язык для создания ограничений.
        /// </summary>
        private static readonly ExternalLangDef LangDef = ExternalLangDef.LanguageDef;

        /// <summary>
        /// Сервис данных для вычитки объектов
        /// </summary>
        private readonly IDataService _dataService;

        public LoggerHelper(IDataService dataService)
        {
            _dataService = dataService;
        }

        /// <summary>
        /// Функция для получения наименования клиента по его идентификатору.
        /// </summary>
        /// <param name="clientId">Идентификатор клиента.</param>
        /// <returns>Наименование клиента, если он был найден в БД, иначе идентификатор, который был передан.</returns>
        internal string GetClientName(string clientId)
        {
            var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(Client), Client.Views.ListView);
            lcs.LimitFunction = LangDef.GetFunction(
                LangDef.funcEQ,
                new VariableDef(LangDef.StringType, Information.ExtractPropertyPath<Client>(x => x.ID)),
                clientId);
            lcs.ReturnTop = 1;

            DataObject[] clients = _dataService.LoadObjects(lcs);
            if (clients.Length > 0)
            {
                var client = (Client)clients[0];
                if (client.ID == clientId)
                {
                    return client.Name;
                }
            }

            return clientId;
        }

        /// <summary>
        /// Функция для получения наименования типа сообщения для лога по его идентификатору.
        /// </summary>
        /// <param name="messageTypeId">Идентификатор типа сообщения.</param>
        /// <returns>Наименование типа сообщения, если он был найден в БД, иначе идентификатор, который был передан.</returns>
        internal string GetMessageTypeName(string messageTypeId)
        {
            var lcs = LoadingCustomizationStruct.GetSimpleStruct(typeof(MessageType), MessageType.Views.ListView);
            lcs.LimitFunction = LangDef.GetFunction(
                LangDef.funcEQ,
                new VariableDef(LangDef.StringType, Information.ExtractPropertyPath<MessageType>(x => x.ID)),
                messageTypeId);
            lcs.ReturnTop = 1;

            DataObject[] messageTypes = _dataService.LoadObjects(lcs);
            if (messageTypes.Length > 0)
            {
                var messageType = (MessageType)messageTypes[0];
                if (messageType.ID == messageTypeId)
                {
                    return messageType.Name;
                }
            }

            return messageTypeId;
        }
    }
}
