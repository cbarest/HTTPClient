using System;
using System.Collections.Generic;
using System.Net;

namespace Dell.Premier.Web.Common.HttpClient
{
    /// <summary>
    ///     <see cref="IWebApiClient" /> is an interface for an HTTP client. See <see cref="WebApiClient" />.
    /// </summary>
    public interface IWebApiClient
    {
        /// <summary>Get sends a GET request to the specified <paramref name="url" />.</summary>
        /// <param name="url">The URL.</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        HttpResponse Get(string url, IDictionary<string, string> extraHeaders = null, TimeSpan? timeout = null);

        /// <summary>Post sends a POST request to the specified <paramref name="url" />.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse Post(string url, string body, IDictionary<string, string> extraHeaders = null);

        /// <summary>
        ///     PostJson sends a POST request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <param name="timeout">The number of seconds to wait before the request times out</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse PostJson(string url, string body, IDictionary<string, string> extraHeaders = null, TimeSpan? timeout = null, WebProxy proxy = null);

        /// <summary>
        ///     PostJson sends a POST request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="body" />.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="body">The object to serialize into a JSON body.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse PostJson<T>(string url, T body, IDictionary<string, string> extraHeaders = null, TimeSpan? timeout = null, bool includeDefaults = true, WebProxy proxy = null)
            where T : class;

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
        HttpResponse PostForm<T>(string url, T body, IDictionary<string, string> extraHeaders = null, WebProxy proxy = null)
            where T : class;

        /// <summary>Patch sends a PATCH request to the specified <paramref name="url" />.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <param name="extraHeaders"></param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse Patch(string url, string body, IDictionary<string, string> extraHeaders = null);

        /// <summary>
        ///     PatchJson sends a PATCH request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse PatchJson(string url, string body, IDictionary<string, string> extraHeaders = null);

        /// <summary>
        ///     PatchJson sends a PATCH request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <typeparam name="T">The type of <paramref name="body" />.</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="body">The object to serialize into a JSON body.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse PatchJson<T>(string url, T body, IDictionary<string, string> extraHeaders = null, bool includeDefaults = true)
            where T : class;

        /// <summary>Put sends a PUT request to the specified <paramref name="url" />.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse Put(string url, string body);

        /// <summary>
        ///     PutJson sends a PUT request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The string body; can be null.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse PutJson(string url, string body, IDictionary<string, string> extraHeaders = null, TimeSpan? timeout = null);

        /// <summary>
        ///     PutJson sends a PUT request to the specified <paramref name="url" /> and sets the Content-Type
        ///     header to application/json.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="body">The object to serialize into a JSON body.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse PutJson<T>(string url, T body, IDictionary<string, string> extraHeaders = null, TimeSpan? timeout = null, bool includeDefaults = true)
            where T : class;

        /// <summary>Delete sends a DELETE request to the specified <paramref name="url" />.</summary>
        /// <param name="url">The URL.</param>
        /// <param name="extraHeaders">Security or other headers to add to the request</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse Delete(string url, IDictionary<string, string> extraHeaders = null);

        /// <summary>
        ///     Do performs the specified HTTP <paramref name="request" />. Use this method when you need more control
        ///     over the request.
        /// </summary>
        /// <param name="request">The request parameters.</param>
        /// <returns>An <see cref="HttpResponse" />.</returns>
        HttpResponse Do(HttpRequest request);
    }
}
