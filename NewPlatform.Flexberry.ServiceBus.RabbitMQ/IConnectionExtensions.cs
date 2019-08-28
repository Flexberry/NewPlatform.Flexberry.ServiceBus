using System.Text;
using RabbitMQ.Client;

namespace NewPlatform.Flexberry.ServiceBus
{
    public static class IConnectionExtensions
    {
        /// <summary>
        /// Get RabbitMQ node name.
        /// </summary>
        /// <param name="connection">RabbitMQ connection</param>
        /// <returns>RabbitMQ node node.</returns>
        public static string GetNodeName(this IConnection connection)
        {
            object clusterNameObj = null;

            if (connection.ServerProperties != null &&
                connection.ServerProperties.TryGetValue("cluster_name", out clusterNameObj))
            {
                if (clusterNameObj is byte[] bytes)
                {
                    return Encoding.UTF8.GetString(bytes);
                }
            }

            return null;
        }
    }
}