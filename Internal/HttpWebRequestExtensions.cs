using System;
using System.Net;
using System.Reflection;

namespace Dell.Premier.Web.Common.HttpClient.Internal
{
    internal static class HttpWebRequestExtensions
    {
        private static readonly FieldInfo s_httpRequestHeaders;

        static HttpWebRequestExtensions()
        {
            // Reflection is used to set the private field _HttpRequestHeaders to an instance of
            // WebApiClientHeaderCollection to get its behavior.
            //
            // The HttpWebRequest property "Headers" has a setter, but it creates a new WebHeaderCollection from the
            // value passed in, and the goal here is to use WebApiClientHeaderCollection.
            //
            // HttpWebRequest isn't sealed and the Headers property is virtual, so we could derive from HttpWebRequest
            // and override the Headers property. However, HttpWebRequest is created using WebRequest.CreateHttp()
            // which manages connection pools; that seems like more trouble than accessing a private field.
            //
            // I understand if there's an objection to this approach. Alternative options are to either use reflection
            // to access the "AddWithoutValidate" method on WebHeaderCollection (it's protected; our subclass is able
            // to access it without reflection), or to avoid reflection completely you can have an if/else which checks
            // for the ~13 header fields which HttpWebRequest requires to be set through properties. However it's
            // implemented I don't want the users of our API to be restricted or have to write special cases to
            // set HTTP headers.

            const BindingFlags privateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

            s_httpRequestHeaders = typeof(HttpWebRequest).GetField("_HttpRequestHeaders", privateInstance);
        }

        public static void SetHeaders(this HttpWebRequest httpWebRequest, HttpHeaders headers)
        {
            if (httpWebRequest == null)
                throw new ArgumentNullException(nameof(httpWebRequest));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            var requestHeaders = new WebApiClientHeaderCollection();
            foreach (var fieldValue in headers)
            {
                requestHeaders.AddWithoutValidate(fieldValue.Field, fieldValue.Value);
            }

            s_httpRequestHeaders.SetValue(httpWebRequest, requestHeaders);
        }
    }
}
