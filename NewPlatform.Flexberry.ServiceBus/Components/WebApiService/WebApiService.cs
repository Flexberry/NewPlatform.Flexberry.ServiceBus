﻿#if NETFRAMEWORK
namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Web.Http;
    using Microsoft.Owin.Hosting;
    using NewPlatform.Flexberry.ServiceBus.Controllers;
    using Owin;

    /// <summary>
    /// The class of component that allows to communicate with the SB by HTTP REST interface.
    /// </summary>
    /// <seealso cref="BaseServiceBusComponent" />
    internal class WebApiService : BaseServiceBusComponent, IWebApiService
    {
        /// <summary>
        /// The base address of WebAPI REST interface.
        /// </summary>
        private readonly string _baseAddress;

        /// <summary>
        /// The sending manager.
        /// </summary>
        private readonly ISendingManager _sendingManager;

        /// <summary>
        /// The receiving manager.
        /// </summary>
        private readonly IReceivingManager _receivingManager;

        /// <summary>
        /// The WebAPI host.
        /// </summary>
        private IDisposable _host;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiService"/> class.
        /// </summary>
        /// <param name="baseAddress">The base address of WebAPI REST interface.</param>
        /// <param name="sendingManager">The sending manager.</param>
        /// <param name="receivingManager">The receiving manager.</param>
        public WebApiService(string baseAddress, ISendingManager sendingManager, IReceivingManager receivingManager)
        {
            if (baseAddress == null)
                throw new ArgumentNullException(nameof(baseAddress));

            if (baseAddress == string.Empty)
                throw new ArgumentException(nameof(baseAddress));

            _baseAddress = baseAddress;
            _sendingManager = sendingManager ?? throw new ArgumentNullException(nameof(sendingManager));
            _receivingManager = receivingManager ?? throw new ArgumentNullException(nameof(receivingManager));
        }

        /// <summary>
        /// Starts WebAPI host.
        /// </summary>
        public override void Start()
        {
            _host = WebApp.Start(
                _baseAddress,
                builder =>
                {
                    var config = new HttpConfiguration();

                    config.MapHttpAttributeRoutes();
                    config.DependencyResolver = new RestDependencyResolver(_sendingManager, _receivingManager);
                    config.Formatters.Remove(config.Formatters.XmlFormatter);

                    // Генерация документации WebAPI сервиса.
                    // Будет доступна по пути /swagger (http://localhost:1235/RestService/swagger).
                    // Подробнее тут: https://github.com/domaindrivendev/Swashbuckle.
//                    config.EnableSwagger(
//                        c =>
//                        {
//                            c.RootUrl(message => _baseAddress);
//                            c.SingleApiVersion("v1", "Rest Service API").Description("RESTful сервис шины");
//                            c.Schemes(new[] { "http" });
//                            c.UseFullTypeNameInSchemaIds();
//
//                            // Папка, в которой расположен исполняемый файл текущего приложения.
//                            string execDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
//
//                            // Файл документации для контроллеров текущего проекта (генерация должна быть включена в свойствах проекта).
//                            c.IncludeXmlComments(Path.Combine(execDirPath, typeof(SBService).Namespace + ".xml"));
//
//                            // Файл документации для моделей.
//                            c.IncludeXmlComments(Path.Combine(execDirPath, "NewPlatform.Flexberry.ServiceBus.Objects.xml"));
//                        }).EnableSwaggerUi();

                    builder.UseWebApi(config);
                });
        }

        /// <summary>
        /// Stops WebAPI host.
        /// </summary>
        public override void Stop()
        {
            if (_host != null)
            {
                _host.Dispose();
                _host = null;
            }
        }
    }
}
#endif
