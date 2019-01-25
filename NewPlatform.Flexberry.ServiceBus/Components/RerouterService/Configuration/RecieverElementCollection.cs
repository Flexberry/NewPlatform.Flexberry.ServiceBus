namespace NewPlatform.Flexberry.ServiceBus.Components.RerouterConfiguration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(RecieverElement), AddItemName = "reciever")]
    public class RecieverElementCollection : ConfigurationElementCollection
    {
        public RecieverElement this[int index]
        {
            get { return (RecieverElement)BaseGet(index); }
        }

        public new RecieverElement this[string name]
        {
            get { return (RecieverElement)base.BaseGet(name); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RecieverElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RecieverElement)(element)).Path;
        }
    }
}
