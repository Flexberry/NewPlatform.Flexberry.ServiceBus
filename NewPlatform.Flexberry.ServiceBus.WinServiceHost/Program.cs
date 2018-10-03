namespace NewPlatform.Flexberry.ServiceBus.WinServiceHost
{
    using System;
    using System.ServiceProcess;

    using ICSSoft.STORMNET;

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {
                ServiceBase.Run(new ServiceBase[] { new WinService() });
            }
            catch (Exception ex)
            {
                LogService.LogError("Ошибка старта сервиса", ex);
            }
        }
    }
}
