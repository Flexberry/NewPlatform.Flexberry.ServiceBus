﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System;
    using System.Xml;
    
    
    // *** Start programmer edit section *** (Using statements)

    // *** End programmer edit section *** (Using statements)


    /// <summary>
    /// IStatisticsSaveService.
    /// </summary>
    // *** Start programmer edit section *** (IStatisticsSaveService CustomAttributes)

    // *** End programmer edit section *** (IStatisticsSaveService CustomAttributes)
    public interface IStatisticsSaveService : NewPlatform.Flexberry.ServiceBus.Components.IServiceBusComponent
    {
        
        // *** Start programmer edit section *** (IStatisticsSaveService CustomMembers)

        // *** End programmer edit section *** (IStatisticsSaveService CustomMembers)

        
        // *** Start programmer edit section *** (IStatisticsSaveService.Save System.Collections.Generic.IEnumerable<StatisticsRecord> CustomAttributes)

        // *** End programmer edit section *** (IStatisticsSaveService.Save System.Collections.Generic.IEnumerable<StatisticsRecord> CustomAttributes)
        void Save(System.Collections.Generic.IEnumerable<StatisticsRecord> stats);
    }
}
