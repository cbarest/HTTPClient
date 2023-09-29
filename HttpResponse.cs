using System;
using System.Net;

namespace Dell.Premier.Web.Common.HttpClient
{
    /// <summary>HttpResponse represents an HTTP response.</summary>
    public class HttpResponse
    {
        private readonly byte[] _body;
        private readonly Exception _exception;
        private readonly HttpHeaders _headers;
        private readonly HttpRequest _request;
        private readonly HttpStatusCode _status;
        private readonly bool _uncompressed;

        public HttpResponse(
            HttpStatusCode status,
            HttpHeaders headers,
            byte[] body,
            bool uncompressed,
            HttpRequest request,
            Exception exception
        )
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            _status = status;
            _headers = headers ?? new HttpHeaders();
            _body = body ?? new byte[0];
            _uncompressed = uncompressed;
            _request = request;
            _exception = exception;
        }

        /// <summary>Gets the body in bytes. This property is never <c>null</c>.</summary>
        internal byte[] Body
        {
            get { return _body; }
        }

        /// <summary>Gets the exception, if any.</summary>
        public Exception Exception
        {
            get { return _exception; }
        }

        /// <summary>Gets the HTTP response headers. This property is never null, though it may be empty.</summary>
        public HttpHeaders Headers
        {
            get { return _headers; }
        }

        /// <summary>Gets the HTTP request sent to the server. This property is never null.</summary>
        public HttpRequest Request
        {
            get { return _request; }
        }

        /// <summary>Gets the HTTP status code.</summary>
        public HttpStatusCode Status
        {
            get { return _status; }
        }

        /// <summary>
        ///     Success indicates whether the request has an HTTP <see cref="Status" /> in the 2xx range and
        ///     encountered no other exceptions. Check this property before calling
        ///     <see cref="HttpResponseExtensions.Read{T}" /> or
        ///     <see cref="HttpResponseExtensions.ReadError{T}" />.
        /// </summary>
        public bool Success
        {
            get { return (int)Status >= 200 && (int)Status <= 299 && Exception == null; }
        }

        /// <summary>
        ///     Gets a value indicating whether the response was uncompressed by the <see cref="WebApiClient" />.
        ///     To disable gzip handling in <see cref="WebApiClient" />, set <see cref="HttpRequest.DisableCompression" />
        ///     to true or set the <see cref="HttpRequest.Headers" /> "Accept-Encoding" header value.
        /// </summary>
        public bool Uncompressed
        {
            get { return _uncompressed; }
        }
    }
}
