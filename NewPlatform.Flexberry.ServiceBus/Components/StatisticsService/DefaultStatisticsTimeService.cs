namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;

    /// <summary>
    /// Class, that returns the current time for statistics service.
    /// </summary>
    internal class DefaultStatisticsTimeService : BaseServiceBusComponent, IStatisticsTimeService
    {
        /// <summary>
        /// Current date and time.
        /// </summary>
        public DateTime Now => DateTime.Now;
    }
}