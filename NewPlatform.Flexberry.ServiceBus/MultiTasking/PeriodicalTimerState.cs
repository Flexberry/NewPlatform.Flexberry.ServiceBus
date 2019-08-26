using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewPlatform.Flexberry.ServiceBus.MultiTasking
{
    /// <summary>
    /// States of periodical timer.
    /// </summary>
    public enum PeriodicalTimerState
    {
        /// <summary>
        /// Periodical processing stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// Stoping periodical processing.
        /// </summary>
        Stoping,

        /// <summary>
        /// Periodical processing in progress.
        /// </summary>
        Working,
    }
}
