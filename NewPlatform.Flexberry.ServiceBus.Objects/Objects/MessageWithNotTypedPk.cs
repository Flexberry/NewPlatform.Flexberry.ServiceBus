namespace NewPlatform.Flexberry.ServiceBus
{
    public class MessageWithNotTypedPk : Message
    {
        /// <summary>
        /// Sets or Gets __Primarykey
        /// </summary>
        public override object __PrimaryKey
        {
            get;
            set;
        }
    }
}
