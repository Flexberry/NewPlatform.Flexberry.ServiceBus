namespace NewPlatform.Flexberry.ServiceBus.ConsoleHost
{
    using System;

    /// <summary>
    /// Консольное приложение для запуска сервисов ServiceBus.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The entry point of the application.
        /// </summary>
        public static void Main()
        {
            using (var serviceBus = ServiceBusCreator.CreateServiceBus())
            {
                serviceBus.Start();

                Console.WriteLine("Service Bus started.\nPress any key to shutdown...");
                Console.ReadKey();

                serviceBus.Stop();
                Environment.Exit(0);
            }
        }
    }
}
