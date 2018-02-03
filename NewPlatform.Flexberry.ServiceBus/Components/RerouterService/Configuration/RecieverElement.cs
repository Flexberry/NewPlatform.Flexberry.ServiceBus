namespace NewPlatform.Flexberry.ServiceBus.Components.RerouterConfiguration
{
    using System.Configuration;

    public class RecieverElement : ConfigurationElement
    {
        [ConfigurationProperty("path")]
        public string Path
        {
            get { return (string)base["path"]; }
        }

        [ConfigurationProperty("wsdl")]
        public string Wsdl
        {
            get { return (string)base["wsdl"]; }
        }

        [ConfigurationProperty("rerouteTo")]
        public string RerouteTo
        {
            get { return (string)base["rerouteTo"]; }
        }

        [ConfigurationProperty("fixWsaTo", DefaultValue = false)]
        public bool FixWsaTo
        {
            get { return (bool)base["fixWsaTo"]; }
        }

        [ConfigurationProperty("sbRequestType")]
        public string SbRequestType
        {
            get { return (string)base["sbRequestType"]; }
        }

        [ConfigurationProperty("sbResponseType")]
        public string SbResponseType
        {
            get { return (string)base["sbResponseType"]; }
        }
    }
}
