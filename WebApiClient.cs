using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Text;
using System.Threading;
using Dell.Premier.Web.Common.Extensions;
using Dell.Premier.Web.Common.Http;
using Dell.Premier.Web.Common.HttpClient.Internal;
using Newtonsoft.Json;

namespace Dell.Premier.Web.Common.HttpClient
{
    /// <summary>
    ///     <para><see cref="WebApiClient" /> provides methods for interacting with web services.</para>
    /// </summary>
    public class WebApiClient : IWebApiClient
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);

        static WebApiClient()
        {
            //specify to use TLS 1.2 as default connection
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        /// <summary>Get sends a GET request to the specified <paramref name="url" />.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <param name="timeout">The number of seconds to wait before the request times out</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse Get(string url, IDictionary<string, string> extraHeaders = null, TimeSpan? timeout = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            var httpRequest = new HttpRequest
            {
                Method = "GET",
                Uri = new Uri(url)
            };

            if (timeout.HasValue)
            {
                httpRequest.Timeout = timeout;
            }

            AddHeaders(httpRequest.Headers, extraHeaders);

            return Do(httpRequest);
        }

        private HttpResponse Post(string url, string body, HttpRequest request)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            byte[] bodyBytes = body == null ? null : Encoding.UTF8.GetBytes(body);

            var httpRequest = new HttpRequest(request)
            {
                Method = "POST",
                Uri = new Uri(url),
                Body = bodyBytes
            };

            return Do(httpRequest);
        }

        /// <summary>Post sends a POST request to the specified <paramref name="url" />.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <param name="extraHeaders">Custom headers to be added</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse Post(string url, string body, IDictionary<string, string> extraHeaders = null)
        {
            if (!extraHeaders.IsNullOrEmpty())
            {
                var httpRequest = new HttpRequest();
                AddHeaders(httpRequest.Headers, extraHeaders);
                return Post(url, body, httpRequest);
            }
            return Post(url, body, (HttpRequest)null);
        }

        /// <summary>
        ///     PostJson sends a POST request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <param name="timeout">The number of seconds to wait before the request times out</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse PostJson(string url, string body, IDictionary<string, string> extraHeaders = null, TimeSpan? timeout = null, WebProxy proxy = null)
        {
            var httpRequest = new HttpRequest();
            httpRequest.Headers["Content-Type"] = ContentType.ApplicationJson;

            if (timeout.HasValue)
            {
                httpRequest.Timeout = timeout;
            }

            AddHeaders(httpRequest.Headers, extraHeaders);
            AddProxy(httpRequest, proxy);

            return Post(url, body, httpRequest);
        }

        /// <summary>
        ///     PostJson sends a POST request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="body" />.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="body">The object to serialize into a JSON body.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <param name="timeout">The number of seconds to wait before the request times out</param>
        /// <param name="includeDefaults"></param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse PostJson<T>(string url, T body, IDictionary<string, string> extraHeaders = null, TimeSpan? timeout = null, bool includeDefaults = true, WebProxy proxy = null)
            where T : class
        {
            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();

            if (!includeDefaults)
                jsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;

            var json = JsonConvert.SerializeObject(body, jsonSerializerSettings);
            return PostJson(url, json == "null" ? null : json, extraHeaders, timeout, proxy);
        }

        /// <summary>
        ///     PostForm sends a POST request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/x-www-form-urlencoded.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="body" />.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="body">
        ///     The <paramref name="body" /> object's public instance properties with public getters will be converted
        ///     to an application/x-www-form-urlencoded string. Each property will have <see cref="object.ToString" />
        ///     called to obtain its value. <c>null</c> properties will be added as an empty string.
        /// </param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse PostForm<T>(string url, T body, IDictionary<string, string> extraHeaders = null, WebProxy proxy = null)
            where T : class
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            return PostForm(url, new FormData(body), extraHeaders, proxy);
        }

        private HttpResponse PostForm(string url, FormData body, IDictionary<string, string> extraHeaders = null, WebProxy proxy = null)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var httpRequest = new HttpRequest();
            httpRequest.Headers["Content-Type"] = ContentType.ApplicationXwwwFormUrlEncoded;

            AddHeaders(httpRequest.Headers, extraHeaders);
            AddProxy(httpRequest, proxy);

            var bodyString = body.GetFormEncodedValue();
            return Post(url, bodyString, httpRequest);
        }

        private HttpResponse Patch(string url, string body, HttpRequest request)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            byte[] bodyBytes = body == null ? null : Encoding.UTF8.GetBytes(body);

            var httpRequest = new HttpRequest(request)
            {
                Method = "PATCH",
                Uri = new Uri(url),
                Body = bodyBytes
            };

            return Do(httpRequest);
        }

        /// <summary>Patch sends a PATCH request to the specified <paramref name="url" />.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <param name="extraHeaders"></param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse Patch(string url, string body, IDictionary<string, string> extraHeaders = null)
        {
            var httpRequest = new HttpRequest();

            AddHeaders(httpRequest.Headers, extraHeaders);
            return Patch(url, body, httpRequest);
        }

        /// <summary>
        ///     PatchJson sends a PATCH request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse PatchJson(string url, string body, IDictionary<string, string> extraHeaders = null)
        {
            var httpRequest = new HttpRequest();
            httpRequest.Headers["Content-Type"] = ContentType.ApplicationJson;

            AddHeaders(httpRequest.Headers, extraHeaders);

            return Patch(url, body, httpRequest);
        }

        /// <summary>
        ///     PatchJson sends a PATCH request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="body" />.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="body">The object to serialize into a JSON body.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <param name="includeDefaults"></param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse PatchJson<T>(string url, T body, IDictionary<string, string> extraHeaders = null, bool includeDefaults = true)
            where T : class
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();

            if (!includeDefaults)
                jsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;

            var json = JsonConvert.SerializeObject(body, jsonSerializerSettings);
            return PatchJson(url, json, extraHeaders);
        }

        private HttpResponse Put(string url, string body, HttpRequest request)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            byte[] bodyBytes = body == null ? null : Encoding.UTF8.GetBytes(body);

            var httpRequest = new HttpRequest(request)
            {
                Method = "PUT",
                Uri = new Uri(url),
                Body = bodyBytes
            };

            return Do(httpRequest);
        }

        /// <summary>Put sends a PUT request to the specified <paramref name="url" />.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse Put(string url, string body)
        {
            return Put(url, body, null);
        }

        /// <summary>
        ///     PutJson sends a PUT request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <param name="timeout">The number of seconds to wait before the request times out</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse PutJson(string url, string body, IDictionary<string, string> extraHeaders = null, TimeSpan? timeout = null)
        {
            var httpRequest = new HttpRequest();
            httpRequest.Headers["Content-Type"] = ContentType.ApplicationJson;

            if (timeout.HasValue)
            {
                httpRequest.Timeout = timeout;
            }

            AddHeaders(httpRequest.Headers, extraHeaders);

            return Put(url, body, httpRequest);
        }

        /// <summary>
        ///     PutJson sends a PUT request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The object to serialize into a JSON body.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <param name="timeout">The number of seconds to wait before the request times out</param>
        /// <param name="includeDefaults"></param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse PutJson<T>(string url, T body, IDictionary<string, string> extraHeaders = null, TimeSpan? timeout = null, bool includeDefaults = true)
            where T : class
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();

            if (!includeDefaults)
                jsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;

            var json = JsonConvert.SerializeObject(body, jsonSerializerSettings);
            return PutJson(url, json, extraHeaders, timeout);
        }

        public HttpResponse Delete(string url, IDictionary<string, string> extraHeaders = null)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            var httpRequest = new HttpRequest
            {
                Method = "DELETE",
                Uri = new Uri(url)
            };

            AddHeaders(httpRequest.Headers, extraHeaders);

            return Do(httpRequest);
        }

        /// <summary>
        ///     Do performs the specified HTTP <paramref name="request" />. Use this method when you need more control
        ///     over the request.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        public HttpResponse Do(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var httpRequest = new HttpRequest(request);
            if (string.IsNullOrEmpty(httpRequest.Method))
                throw new ArgumentException("request.Method must contain a value", nameof(request));
            if (httpRequest.Uri == null)
                throw new ArgumentException("request.Uri must not be null", nameof(request));
            if (httpRequest.Timeout != null && httpRequest.Timeout.Value < TimeSpan.Zero)
                throw new ArgumentException("request.Timeout must be >= 0 or null", nameof(request));

            var webRequest = CreateHttpWebRequest(httpRequest);

            PersistCorrelationId(httpRequest);

            // add "Accept-Encoding: gzip" if not explicitly disabled or implicitly disabled by the pre-conditions check
            var addedGzip = false;
            if (ShouldCompress(httpRequest))
            {
                addedGzip = true;
                httpRequest.Headers["Accept-Encoding"] = "gzip";
            }

            webRequest.SetHeaders(httpRequest.Headers);

            byte[] responseBody = null;
            var uncompressed = false;
            HttpStatusCode statusCode = 0;
            Exception exception = null;
            HttpHeaders responseHeaders = null;
            try
            {
                WriteRequest(webRequest, httpRequest.Body);

                // read response
                using (var response = (HttpWebResponse)webRequest.GetResponse())
                {
                    statusCode = response.StatusCode;

                    responseHeaders = GetHeaders(response.Headers);
                    responseBody = GetBody(response, addedGzip, out uncompressed);
                }
            }
            catch (WebException webEx)
            {
                var response = webEx.Response as HttpWebResponse;
                if (response != null)
                {
                    try
                    {
                        statusCode = response.StatusCode;

                        responseHeaders = GetHeaders(response.Headers);
                        responseBody = GetBody(response, addedGzip, out uncompressed);
                    }
                    catch (Exception ex)
                    {
                        if (ShouldThrow(ex))
                        {
                            throw;
                        }

                        // silently ignore this exception and report the initial exception
                    }
                }

                exception = webEx;
            }
            catch (Exception ex)
            {
                if (ShouldThrow(ex))
                {
                    throw;
                }

                exception = ex;
            }

            return new HttpResponse(statusCode, responseHeaders, responseBody, uncompressed, httpRequest, exception);
        }

        internal static void PersistCorrelationId(HttpRequest httpRequest)
        {
            //persist correlation ids
            const string xDellTraceid = "Dell_TraceID";

            var traceId = (string)CallContext.LogicalGetData(xDellTraceid);

            httpRequest.Headers.Add(xDellTraceid, traceId);
        }

        private static HttpWebRequest CreateHttpWebRequest(HttpRequest httpRequest)
        {
            var webRequest = WebRequest.CreateHttp(httpRequest.Uri);
            webRequest.Method = httpRequest.Method;
            webRequest.AllowAutoRedirect = !httpRequest.DisableAutoRedirect;
            webRequest.KeepAlive = !httpRequest.DisableKeepAlive;
            webRequest.Proxy = httpRequest.Proxy ?? WebRequest.DefaultWebProxy;
            webRequest.Credentials = httpRequest.Credentials;

            // set timeout
            var timeout = httpRequest.Timeout ?? DefaultTimeout;
            var timeoutMillseconds = (int)timeout.TotalMilliseconds;
            webRequest.Timeout = timeoutMillseconds;
            webRequest.ReadWriteTimeout = timeoutMillseconds;

            return webRequest;
        }

        private static void AddHeaders(HttpHeaders headers, IDictionary<string, string> extraHeaders)
        {
            if (headers != null && !extraHeaders.IsNullOrEmpty())
            {
                foreach (var kvp in extraHeaders)
                {
                    headers[kvp.Key] = kvp.Value;
                }
            }
        }

        private static void AddProxy(HttpRequest httpRequest, WebProxy proxy)
        {
            if (httpRequest != null && proxy != null)
            {
                httpRequest.Proxy = proxy;
            }
        }

        private static void WriteRequest(WebRequest webRequest, byte[] body)
        {
            if (body == null || body.Length == 0)
            {
                webRequest.ContentLength = 0;
            }
            else
            {
                webRequest.ContentLength = body.Length;

                using (var requestStream = webRequest.GetRequestStream())
                using (var writer = new BinaryWriter(requestStream))
                {
                    writer.Write(body);
                }
            }
        }

        private static HttpHeaders GetHeaders(WebHeaderCollection headerCollection)
        {
            var responseHeaders = new HttpHeaders();
            foreach (var key in headerCollection.AllKeys)
            {
                var values = headerCollection.GetValues(key);
                if (values != null)
                {
                    foreach (var value in values)
                    {
                        responseHeaders.Add(key, value);
                    }
                }
            }

            return responseHeaders;
        }

        private static byte[] GetBody(WebResponse response, bool addedGzip, out bool uncompressed)
        {
            const int bufferSize = 8192;

            // if the response was gzip'd and the client added the Accept-Encoding header, decompress the response
            if (addedGzip && response.Headers["Content-Encoding"] == "gzip")
            {
                using (var responseStream = response.GetResponseStream())
                // ReSharper disable once AssignNullToNotNullAttribute
                using (var decompressionStream = new GZipStream(responseStream, CompressionMode.Decompress))
                using (var memoryStream = new MemoryStream())
                {
                    decompressionStream.CopyTo(memoryStream, bufferSize);
                    uncompressed = true;
                    return memoryStream.ToArray();
                }
            }

            // use raw response
            using (var responseStream = response.GetResponseStream())
            using (var memoryStream = new MemoryStream())
            {
                // ReSharper disable once PossibleNullReferenceException
                responseStream.CopyTo(memoryStream, bufferSize);
                uncompressed = false;
                return memoryStream.ToArray();
            }
        }

        private static bool ShouldCompress(HttpRequest request)
        {
            // NOTE: regarding skipping gzip on HEAD requests, see https://trac.nginx.org/nginx/ticket/261

            return !request.DisableCompression &&
                   string.IsNullOrEmpty(request.Headers["Accept-Encoding"]) &&
                   string.IsNullOrEmpty(request.Headers["Range"]) &&
                   (request.Method != "HEAD");
        }

        private static bool ShouldThrow(Exception ex)
        {
            // These exceptions should always be thrown instead of added as an Exception property on the HttpResponse.
            return ex is ThreadAbortException
                   || ex is StackOverflowException
                   || ex is OutOfMemoryException
                   || ex is SecurityException;
        }
    }
}
