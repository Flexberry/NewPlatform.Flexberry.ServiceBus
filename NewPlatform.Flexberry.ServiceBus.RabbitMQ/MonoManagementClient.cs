namespace NewPlatform.Flexberry.ServiceBus
{
    using System;
    using System.Net.Http;
    using System.Text;

    using EasyNetQ.Management.Client;

    /// <summary>
    /// <see cref="ManagementClient"/> to run in Mono with hacked authorization.
    /// </summary>
    public class MonoManagementClient : ManagementClient
    {
        /// <summary>
        /// Value for the 'Authorization' header.
        /// </summary>
        private static string header;

        /// <summary>
        /// Adds an 'Authorization' header to each request.
        /// </summary>
        private static Action<HttpRequestMessage> configureRequest = (request) =>
        {
            request.Headers.Add("Authorization", header);
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MonoManagementClient"/> class.
        /// </summary>
        /// <param name="host">The URL of the host.</param>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="port">Port number.</param>
        public MonoManagementClient(string host, string username, string password, int port)
            : base(host, username, password, port, null, configureRequest)
        {
            header = $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"))}";
        }
    }
}
