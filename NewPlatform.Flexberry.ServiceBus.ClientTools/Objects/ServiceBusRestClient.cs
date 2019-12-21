using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Net;

namespace NewPlatform.Flexberry.ServiceBus
{
    /// <summary>
    /// Client for REST-API of Service Bus.
    /// </summary>
    public partial class ServiceBusRestClient
    {
        private readonly HttpClient _httpClient;
        private readonly Lazy<JsonSerializerSettings> _settings;

        /// <summary>
        /// Client for working with REST-API of Service Bus.
        /// </summary>
        /// <param name="httpClient">HTTP-client for making requests.</param>
        public ServiceBusRestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _settings = new Lazy<JsonSerializerSettings>(() =>
            {
                var settings = new JsonSerializerSettings();
                UpdateJsonSerializerSettings(settings);
                return settings;
            });
        }

        /// <summary>
        /// URL of REST-API of Service Bus.
        /// </summary>
        public string BaseUrl { get; set; } = "http://localhost:7085/RestService";

        /// <summary>
        /// Settings for JSON-serializer.
        /// </summary>
        protected JsonSerializerSettings JsonSerializerSettings { get { return _settings.Value; } }

        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings);
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, string url);
        partial void PrepareRequest(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder);
        partial void ProcessResponse(HttpClient client, HttpResponseMessage response);

        /// <summary>Get incomming messages list for a client.</summary>
        /// <param name="clientId">ID of receiving client.</param>
        /// <returns>List of message infos.</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task<ICollection<ServiceBusMessageInfo>> GetMessagesAsync(string clientId)
        {
            return GetMessagesAsync(clientId, CancellationToken.None);
        }

        /// <summary>Get incomming messages list for a client.</summary>
        /// <param name="clientId">ID of receiving client.</param>
        /// <returns>List of message infos.</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<ICollection<ServiceBusMessageInfo>> GetMessagesAsync(string clientId, CancellationToken cancellationToken)
        {
            if (clientId == null)
                throw new ArgumentNullException(nameof(clientId));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl?.TrimEnd('/')).Append("/Messages?");
            urlBuilder.Append("clientId=").Append(Uri.EscapeDataString(ConvertToString(clientId, CultureInfo.InvariantCulture)));

            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                request.RequestUri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                try
                {
                    var headers = Enumerable.ToDictionary(response.Headers, h => h.Key, h => h.Value);
                    if (response.Content?.Headers != null)
                        foreach (var item in response.Content.Headers)
                            headers[item.Key] = item.Value;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        try
                        {
                            var result = JsonConvert.DeserializeObject<ICollection<ServiceBusMessageInfo>>(responseData, _settings.Value);
                            return result;
                        }
                        catch (Exception ex)
                        {
                            throw new SwaggerException("Could not deserialize the response body.", (int)response.StatusCode, responseData, headers, ex);
                        }
                    }
                    else if (response.StatusCode != HttpStatusCode.NoContent)
                    {
                        var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response.StatusCode + ").", (int)response.StatusCode, responseData, headers, null);
                    }

                    return default;
                }
                finally
                {
                    response?.Dispose();
                }
            }
        }

        /// <summary>Get specific incomming message for a client.</summary>
        /// <param name="clientId">Client's ID.</param>
        /// <param name="messageTypeId">Message type's ID.</param>
        /// <param name="index">Index of message.</param>
        /// <returns>Incomming message for a client.</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task<HttpMessageFromEsb> GetMessageAsync(string clientId, string messageTypeId, int index)
        {
            return GetMessageAsync(clientId, messageTypeId, index, CancellationToken.None);
        }

        /// <summary>Get incomming message by type and index.</summary>
        /// <param name="clientId">Client's ID.</param>
        /// <param name="messageTypeId">Message type's ID.</param>
        /// <param name="index">Index of message.</param>
        /// <returns>Incomming message for a client.</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<HttpMessageFromEsb> GetMessageAsync(string clientId, string messageTypeId, int index, CancellationToken cancellationToken)
        {
            if (clientId == null)
                throw new ArgumentNullException(nameof(clientId));

            if (messageTypeId == null)
                throw new ArgumentNullException(nameof(messageTypeId));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl?.TrimEnd('/')).Append("/Message?");
            urlBuilder.Append("clientId=").Append(Uri.EscapeDataString(ConvertToString(clientId, CultureInfo.InvariantCulture))).Append("&");
            urlBuilder.Append("messageTypeId=").Append(Uri.EscapeDataString(ConvertToString(messageTypeId, CultureInfo.InvariantCulture))).Append("&");
            urlBuilder.Append("index=").Append(Uri.EscapeDataString(ConvertToString(index, CultureInfo.InvariantCulture)));

            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                request.RequestUri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                try
                {
                    var headers = Enumerable.ToDictionary(response.Headers, h => h.Key, h => h.Value);
                    if (response.Content?.Headers != null)
                        foreach (var item_ in response.Content.Headers)
                            headers[item_.Key] = item_.Value;

                    ProcessResponse(_httpClient, response);

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        try
                        {
                            var result = JsonConvert.DeserializeObject<HttpMessageFromEsb>(responseData, _settings.Value);
                            return result;
                        }
                        catch (Exception ex)
                        {
                            throw new SwaggerException("Could not deserialize the response body.", (int)response.StatusCode, responseData, headers, ex);
                        }
                    }
                    else if (response.StatusCode != HttpStatusCode.NoContent)
                    {
                        var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response.StatusCode + ").", (int)response.StatusCode, responseData, headers, null);
                    }

                    return default;
                }
                finally
                {
                    response?.Dispose();
                }
            }
        }

        /// <summary>Get incomming message by ID.</summary>
        /// <param name="id">Message's ID.</param>
        /// <returns>Incomming message.</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task<HttpMessageFromEsb> GetMessageAsync(string id)
        {
            return GetMessageAsync(id, CancellationToken.None);
        }

        /// <summary>Get incomming message by ID.</summary>
        /// <param name="id">Message's ID.</param>
        /// <returns>Incomming message.</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task<HttpMessageFromEsb> GetMessageAsync(string id, CancellationToken cancellationToken)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl?.TrimEnd('/')).Append("/Message/{id}");
            urlBuilder.Replace("{id}", Uri.EscapeDataString(ConvertToString(id, CultureInfo.InvariantCulture)));

            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
                request.RequestUri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                try
                {
                    var headers = Enumerable.ToDictionary(response.Headers, h => h.Key, h => h.Value);
                    if (response.Content?.Headers != null)
                        foreach (var item in response.Content.Headers)
                            headers[item.Key] = item.Value;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        try
                        {
                            var result = JsonConvert.DeserializeObject<HttpMessageFromEsb>(responseData, _settings.Value);
                            return result;
                        }
                        catch (Exception ex)
                        {
                            throw new SwaggerException("Could not deserialize the response body.", (int)response.StatusCode, responseData, headers, ex);
                        }
                    }
                    else if (response.StatusCode != HttpStatusCode.NoContent)
                    {
                        var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response.StatusCode + ").", (int)response.StatusCode, responseData, headers, null);
                    }

                    return default;
                }
                finally
                {
                    response?.Dispose();
                }
            }
        }

        /// <summary>Send outgoing message.</summary>
        /// <param name="value">Outgoing message.</param>
        /// <returns>No Content</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task PostMessageAsync(ServiceBusMessage value)
        {
            return PostMessageAsync(value, CancellationToken.None);
        }

        /// <summary>Send outgoing message.</summary>
        /// <param name="value">Outgoing message.</param>
        /// <returns>No Content</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task PostMessageAsync(ServiceBusMessage value, CancellationToken cancellationToken)
        {
            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl?.TrimEnd('/') ?? "").Append("/Message");

            using (var request = new HttpRequestMessage())
            {
                var content = new StringContent(JsonConvert.SerializeObject(value, _settings.Value));
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                request.Content = content;
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                try
                {
                    var headers = Enumerable.ToDictionary(response.Headers, h => h.Key, h => h.Value);
                    if (response.Content?.Headers != null)
                        foreach (var item in response.Content.Headers)
                            headers[item.Key] = item.Value;

                    if (response.StatusCode == HttpStatusCode.NoContent)
                        return;
                    else if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response.StatusCode + ").", (int)response.StatusCode, responseData, headers, null);
                    }
                }
                finally
                {
                    response?.Dispose();
                }
            }
        }

        /// <summary>Remove message from Service Bus.</summary>
        /// <param name="id">ID of message to delete.</param>
        /// <returns>No Content</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        public Task DeleteMessageAsync(string id)
        {
            return DeleteMessageAsync(id, CancellationToken.None);
        }

        /// <summary>Remove message from Service Bus.</summary>
        /// <param name="id">ID of message to delete.</param>
        /// <returns>No Content</returns>
        /// <exception cref="SwaggerException">A server side error occurred.</exception>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        public async Task DeleteMessageAsync(string id, CancellationToken cancellationToken)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl?.TrimEnd('/')).Append("/Message/{id}");
            urlBuilder.Replace("{id}", Uri.EscapeDataString(ConvertToString(id, CultureInfo.InvariantCulture)));

            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Delete;
                request.RequestUri = new Uri(urlBuilder.ToString(), UriKind.RelativeOrAbsolute);

                var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                try
                {
                    var headers = Enumerable.ToDictionary(response.Headers, h => h.Key, h => h.Value);
                    if (response.Content?.Headers != null)
                        foreach (var item in response.Content.Headers)
                            headers[item.Key] = item.Value;

                    if (response.StatusCode == HttpStatusCode.NoContent)
                        return;
                    else if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var responseData = response.Content == null ? null : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new SwaggerException("The HTTP status code of the response was not expected (" + (int)response.StatusCode + ").", (int)response.StatusCode, responseData, headers, null);
                    }
                }
                finally
                {
                    response?.Dispose();
                }
            }
        }

        private string ConvertToString(object value, CultureInfo cultureInfo)
        {
            switch (value)
            {
                case Enum v:
                    var name = Enum.GetName(v.GetType(), v);
                    if (name != null)
                    {
                        var field = IntrospectionExtensions.GetTypeInfo(v.GetType()).GetDeclaredField(name);
                        if (field != null && CustomAttributeExtensions.GetCustomAttribute(field, typeof(EnumMemberAttribute)) is EnumMemberAttribute attribute)
                            return attribute.Value;
                    }
                    break;
                case bool v:
                    return Convert.ToString(v, cultureInfo).ToLowerInvariant();
                case byte[] v:
                    return Convert.ToBase64String(v);
                case Array v:
                    var array = Enumerable.OfType<object>(v);
                    return string.Join(",", Enumerable.Select(array, o => ConvertToString(o, cultureInfo)));
            }

            return Convert.ToString(value, cultureInfo);
        }
    }

    /// <summary>
    /// Exception in client for swagger-service.
    /// </summary>
    public class SwaggerException : Exception
    {
        /// <summary>
        /// HTTP status code.
        /// </summary>
        public int StatusCode { get; private set; }

        /// <summary>
        /// Text of response.
        /// </summary>
        public string Response { get; private set; }

        public Dictionary<string, IEnumerable<string>> Headers { get; private set; }

        /// <summary>
        /// Exception in client for swagger-service.
        /// </summary>
        public SwaggerException(string message, int statusCode, string response, Dictionary<string, IEnumerable<string>> headers, Exception innerException)
            : base(message + "\n\nStatus: " + statusCode + "\nResponse: \n" + response.Substring(0, response.Length >= 512 ? 512 : response.Length), innerException)
        {
            StatusCode = statusCode;
            Response = response;
            Headers = headers;
        }

        /// <summary>
        /// Creates and returns a string representation of the current exception.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
        }
    }

    /// <summary>
    /// Exception in client for swagger-service with result.
    /// </summary>
    /// <typeparam name="TResult">Result's type.</typeparam>
    public partial class SwaggerException<TResult> : SwaggerException
    {
        /// <summary>
        /// Result.
        /// </summary>
        public TResult Result { get; private set; }

        /// <summary>
        /// Exception in client for swagger-service with result.
        /// </summary>
        public SwaggerException(string message, int statusCode, string response, Dictionary<string, IEnumerable<string>> headers, TResult result, Exception innerException)
            : base(message, statusCode, response, headers, innerException)
        {
            Result = result;
        }
    }

}
