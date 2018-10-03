// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectInstaller.cs" company="IIS">
//   Copyright (c) IIS. All rights reserved.
// </copyright>
// <summary>
//   The project installer.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NewPlatform.Flexberry.ServiceBus.WinServiceHost
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Configuration.Install;
    using System.Reflection;

    /// <summary>
    /// Класс установщика win-сервиса.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        /// <summary>
        /// Инсталлер вычитывает параметры из AppConfig
        /// http://www.codeproject.com/Articles/21320/Multiple-Instance-NET-Windows-Service
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();

            // Чтение имени сервиса из конфигурационного файла, если оно там есть.
            Assembly service = Assembly.GetAssembly(this.GetType());
            Configuration config = ConfigurationManager.OpenExeConfiguration(service.Location);
            KeyValueConfigurationElement configElement = config.AppSettings.Settings["ServiceName"];
            if (configElement != null)
                serviceInstaller.ServiceName = configElement.Value;
        }
    }
}
