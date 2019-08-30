namespace NewPlatform.Flexberry.ServiceBus.Components
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    /// <summary>
    /// SOAP 1.2 binding without WSA.
    /// </summary>
    public class Soap12Binding : CustomBinding
    {
        private HttpTransportBindingElement httpTransportBindingElement;

        public Soap12Binding()
        {
            this.Elements.Add(new TextMessageEncodingBindingElement(MessageVersion.Soap12, System.Text.Encoding.UTF8));
            httpTransportBindingElement = new HttpTransportBindingElement();
            this.Elements.Add(new HttpTransportBindingElement());
        }

        /// <summary>Gets and sets the maximum allowable message size, in bytes, that can be received.</summary>
        /// <returns>The maximum allowable message size that can be received. The default is 65,536 bytes.</returns>
        public long MaxReceivedMessageSize
        {
            get { return httpTransportBindingElement.MaxReceivedMessageSize; }
            set { httpTransportBindingElement.MaxReceivedMessageSize = value; }
        }

        /// <summary>Gets or sets the maximum size, in bytes, of any buffer pools used by the transport. </summary>
        /// <returns>The maximum size of the buffer pool. The default is 524,288 bytes.</returns>
        public long MaxBufferPoolSize
        {
            get { return httpTransportBindingElement.MaxBufferPoolSize; }
            set { httpTransportBindingElement.MaxBufferPoolSize = value; }
        }
    }
}