using NewPlatform.Flexberry.ServiceBus.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    /// <summary>
    /// Component for collecting statistics from external systems
    /// </summary>
    public interface IExternalStatisticsCollector
    {
        /// <summary>
        /// Gets or sets statistics interval for 
        /// </summary>
        StatisticsInterval Interval { get; set; }
    }
}
