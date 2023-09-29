using System;
using System.Net;

namespace Dell.Premier.Web.Common.HttpClient
{
    /// <summary>HttpRequest represents an HTTP request.</summary>
    public class HttpRequest
    {
        private readonly HttpHeaders _headers = new HttpHeaders();

        /// <summary>Initializes a new instance of <see cref="HttpRequest" />.</summary>
        public HttpRequest()
        {
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="HttpRequest" /> by copying the header values from
        ///     <paramref name="headers" />.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="headers" /> is null.</exception>
        /// <param name="headers">The <see cref="HttpHeaders" /> to copy into this <see cref="HttpRequest" />.</param>
        public HttpRequest(HttpHeaders headers)
        {
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            foreach (var header in headers)
            {
                Headers.Add(header.Field, header.Value);
            }
        }

        /// <summary>Initializes a new instance of <see cref="HttpRequest" />.</summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="method" /> or <paramref name="url" /> arguments are null.
        /// </exception>
        /// <param name="method">The HTTP method.</param>
        /// <param name="url">URL of the document.</param>
        public HttpRequest(string method, string url)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            Method = method;
            Uri = new Uri(url);
        }

        /// <summary>
        ///     Initializes a new instance of <see cref="HttpRequest" /> by creating a copy of the specified
        ///     <paramref name="request" />.
        /// </summary>
        /// <param name="request">The request to copy; can be null.</param>
        public HttpRequest(HttpRequest request)
        {
            if (request == null)
                return;

            Uri = request.Uri;
            Method = request.Method;
            Body = request.Body;
            Timeout = request.Timeout;
            Proxy = request.Proxy;
            Credentials = request.Credentials;
            DisableCompression = request.DisableCompression;
            DisableKeepAlive = request.DisableKeepAlive;
            DisableAutoRedirect = request.DisableAutoRedirect;

            foreach (var header in request.Headers)
            {
                Headers.Add(header.Field, header.Value);
            }
        }

        /// <summary>Gets or sets the request body.</summary>
        public byte[] Body { get; set; }

        /// <summary>Gets or sets the credentials.</summary>
        public ICredentials Credentials { get; set; }

        /// <summary>Gets or sets a value indicating whether automatic redirect is disabled.</summary>
        public bool DisableAutoRedirect { get; set; }

        /// <summary>Gets or sets a value indicating whether Gzip compression is disabled.</summary>
        public bool DisableCompression { get; set; }

        /// <summary>Gets or sets a value indicating whether keep-alive is disabled.</summary>
        public bool DisableKeepAlive { get; set; }

        /// <summary>Gets the HTTP headers for this request.</summary>
        public HttpHeaders Headers
        {
            get { return _headers; }
        }

        /// <summary>Gets or sets the HTTP method.</summary>
        public string Method { get; set; }

        /// <summary>Gets or sets the proxy.</summary>
        public IWebProxy Proxy { get; set; }

        /// <summary>Gets or sets the timeout.</summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>Gets or sets the request URI.</summary>
        public Uri Uri { get; set; }
    }
}
