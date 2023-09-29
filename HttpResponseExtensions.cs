using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Dell.Premier.Web.Common.Serialization;
using Newtonsoft.Json;

namespace Dell.Premier.Web.Common.HttpClient
{
    /// <summary>HttpResponseExtensions contains extension methods for <see cref="HttpResponse" />.</summary>
    public static class HttpResponseExtensions
    {
        private const int MaxBodyLengthOnError = 1000;
        private static readonly ConcurrentDictionary<string, XmlSerializer> _serializerCache = new ConcurrentDictionary<string, XmlSerializer>();

        /// <summary>
        ///     <para>
        ///         <see cref="Read{T}" /> converts the <see cref="HttpResponse.Body" /> into type
        ///         <typeparamref name="T" /> if the request was successful.
        ///     </para>
        ///     <para>
        ///         A <see cref="WebException" /> is thrown if <see cref="HttpResponse.Success" /> is false. To avoid
        ///         raising this exception check <see cref="HttpResponse.Success" />; if
        ///         <see cref="HttpResponse.Success" /> is false, use <see cref="ReadError{T}" /> instead.
        ///     </para>
        ///     <para>
        ///         The deserializer chosen depends on the Content-Type response header. If Content-Type is
        ///         application/xml or text/xml the response is deserialized as XML; otherwise, it will
        ///         optimisitically deserialize as JSON without checking the Content-Type for a match.
        ///     </para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpResponse" /> is null.</exception>
        /// <exception cref="WebException">Thrown when <see cref="HttpResponse.Success" /> is false.</exception>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="httpResponse">The <see cref="HttpResponse" /> to deserialize.</param>
        /// <returns>The response body as type <typeparamref name="T" />.</returns>
        public static T Read<T>(this HttpResponse httpResponse)
            where T : class
        {
            if (httpResponse == null)
                throw new ArgumentNullException(nameof(httpResponse));

            if (!httpResponse.Success)
            {
                HandleUnsuccessfulResponse(httpResponse);
            }

            return DeserializeHttpResponse<T>(httpResponse);
        }

        private static void HandleUnsuccessfulResponse(HttpResponse httpResponse)
        {
            var message = $"An error occurred performing a WebApiClient request to {httpResponse.Request.Uri}.";
            if (httpResponse.Status == 0)
                throw new WebException(message, httpResponse.Exception);

            var statusCode = (int)httpResponse.Status;
            var statusDescription = HttpWorkerRequest.GetStatusDescription(statusCode);

            var contentType = httpResponse.Headers["Content-Type"];
            var messageBody = GetStringUsingEncoding(contentType, httpResponse.Body);

            if (messageBody.Length > MaxBodyLengthOnError)
            {
                messageBody = messageBody.Substring(0, MaxBodyLengthOnError);
            }

            message += $" StatusCode={statusCode} {statusDescription} Message Body={messageBody}";

            throw new WebException(message, httpResponse.Exception);
        }

        /// <summary>
        ///     <para>
        ///         <see cref="ReadBytes" /> converts the <see cref="HttpResponse.Body" /> into a byte array if the
        ///         request was successful; otherwise a <see cref="WebException" /> is thrown. This method never
        ///         returns null.
        ///     </para>
        ///     <para>
        ///         See <see cref="Read{T}" /> for more details.
        ///     </para>
        /// </summary>
        /// <param name="httpResponse">The <see cref="HttpResponse" /> to deserialize.</param>
        /// <returns>The response body as a byte array. This method never returns null.</returns>
        public static byte[] ReadBytes(this HttpResponse httpResponse)
        {
            return Read<byte[]>(httpResponse);
        }

        /// <summary>
        ///     <para>
        ///         <see cref="ReadError{T}" /> converts the <see cref="HttpResponse.Body" /> into type
        ///         <typeparamref name="T" /> if the request was not successful. If the response was successful this
        ///         method will raise a <see cref="ReadErrorWhenNoErrorException" />.
        ///     </para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpResponse" /> is null.</exception>
        /// <exception cref="ReadErrorWhenNoErrorException">
        ///     Thrown when <see cref="HttpResponse.Success" /> is true.
        /// </exception>
        /// <typeparam name="T">The type to return.</typeparam>
        /// <param name="httpResponse">The <see cref="HttpResponse" /> to deserialize.</param>
        /// <returns>The response body as type <typeparamref name="T" />.</returns>
        public static T ReadError<T>(this HttpResponse httpResponse)
            where T : class
        {
            if (httpResponse == null)
                throw new ArgumentNullException(nameof(httpResponse));

            if (httpResponse.Success)
            {
                var message = "No error occurred performing a WebApiClient request to " +
                    $"{httpResponse.Request.Uri}; use the `Read` method instead.";

                throw new ReadErrorWhenNoErrorException(message);
            }

            return DeserializeHttpResponse<T>(httpResponse);
        }

        /// <summary>
        ///     <para>
        ///         <see cref="ReadErrorBytes" /> converts the <see cref="HttpResponse.Body" /> into a byte array if
        ///         the request was not successful. If the response was successful this method will raise a
        ///         <see cref="ReadErrorWhenNoErrorException" />. This method never returns null.
        ///     </para>
        ///     <para>
        ///         See <see cref="ReadError{T}" /> for more details.
        ///     </para>
        /// </summary>
        /// <param name="httpResponse">The <see cref="HttpResponse" /> to deserialize.</param>
        /// <returns>The response body as a byte array. This method never returns null.</returns>
        public static byte[] ReadErrorBytes(this HttpResponse httpResponse)
        {
            return ReadError<byte[]>(httpResponse);
        }

        /// <summary>
        ///     <para>
        ///         <see cref="ReadErrorString" /> converts the <see cref="HttpResponse.Body" /> into a string if the
        ///         request was not successful. If the response was successful this method will raise a
        ///         <see cref="ReadErrorWhenNoErrorException" />. This method never returns null.
        ///     </para>
        ///     <para>
        ///         See <see cref="ReadError{T}" /> for more details.
        ///     </para>
        /// </summary>
        /// <param name="httpResponse">The <see cref="HttpResponse" /> to deserialize.</param>
        /// <returns>The response body as a string. This method never returns null.</returns>
        public static string ReadErrorString(this HttpResponse httpResponse)
        {
            return ReadError<string>(httpResponse);
        }

        /// <summary>
        ///     <para>
        ///         <see cref="ReadString" /> converts the <see cref="HttpResponse.Body" /> into a string if the
        ///         request was successful; otherwise a <see cref="WebException" /> is thrown. This method never
        ///         returns null.
        ///     </para>
        ///     <para>
        ///         See <see cref="Read{T}" /> for more details.
        ///     </para>
        /// </summary>
        /// <param name="httpResponse">The <see cref="HttpResponse" /> to deserialize.</param>
        /// <returns>The response body as a string. This method never returns null.</returns>
        public static string ReadString(this HttpResponse httpResponse)
        {
            return Read<string>(httpResponse);
        }

        private static bool ByteArrayHasPrefix(byte[] prefix, byte[] byteArray)
        {
            // Borrowed from System.Net.WebClient
            if (prefix == null || byteArray == null || prefix.Length > byteArray.Length)
            {
                return false;
            }
            for (var i = 0; i < prefix.Length; i++)
            {
                if (prefix[i] != byteArray[i])
                {
                    return false;
                }
            }
            return true;
        }

        private static T DeserializeHttpResponse<T>(HttpResponse httpResponse)
            where T : class
        {
            var body = httpResponse.Body ?? new byte[0];

            if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)body;
            }

            var contentType = httpResponse.Headers["Content-Type"];
            if (typeof(T) == typeof(string))
            {
                return (T)(object)GetStringUsingEncoding(contentType, body);
            }

            if (contentType.Contains("xml"))
            {
                return DeserializeXml<T>(body);
            }

            return DeserializeJson<T>(GetStringUsingEncoding(contentType, body));
        }

        private static T DeserializeJson<T>(string body)
        {
            return JsonConvert.DeserializeObject<T>(body);
        }

        private static T DeserializeXml<T>(byte[] body)
        {
            using (var stringReader = new MemoryStream(body))
            {
                using (var xmlReader = new XmlTextReader(stringReader))
                {
                    XmlReader reader = XmlReader.Create(new MemoryStream(body));
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(reader);
                    XmlElement root = xmlDoc.DocumentElement;
                    var serializer = HttpResponseXmlSerializerBuilder.Build(typeof(T), root);
                    return (T)serializer.Deserialize(xmlReader);
                }
            }
        }

        private static string GetStringUsingEncoding(string contentType, byte[] data)
        {
            // Borrowed from System.Net.WebClient, with some modifications.

            Encoding enc = null;
            var bomLengthInData = -1;

            if (!string.IsNullOrEmpty(contentType))
            {
                contentType = contentType.ToLowerInvariant();
                // splitting on '"' will remove quotes around charset, ex: charset="utf-8"
                var parsedList = contentType.Split(';', '=', ' ', '"');
                var nextItem = false;
                foreach (var item in parsedList)
                {
                    if (item == "charset")
                    {
                        nextItem = true;
                    }
                    else if (nextItem)
                    {
                        try
                        {
                            enc = Encoding.GetEncoding(item);
                        }
                        catch (ArgumentException)
                        {
                            // Eat ArgumentException here.
                            // We'll assume that Content-Type encoding might have been garbled and wasn't present at all.
                            break;
                        }
                        // Unexpected exceptions are thrown back to caller
                    }
                }
            }

            // If no content encoding listed in the ContentType HTTP header, or no Content-Type header present, then
            // check for a byte-order-mark (BOM) in the data to figure out encoding.
            if (enc == null)
            {
                // UTF32 must be tested before Unicode because its BOM is the same but longer.
                Encoding[] encodings = { Encoding.UTF8, Encoding.UTF32, Encoding.Unicode, Encoding.BigEndianUnicode };
                foreach (var encoding in encodings)
                {
                    var preamble = encoding.GetPreamble();
                    if (ByteArrayHasPrefix(preamble, data))
                    {
                        enc = encoding;
                        bomLengthInData = preamble.Length;
                        break;
                    }
                }
            }

            // Do we have an encoding guess? If not, use UTF-8.
            if (enc == null)
            {
                // NOTE: Modified. Original from WebClient used the system's default codepage.
                //
                // From RFC 2616:
                //   Some HTTP/1.0 software has interpreted a Content-Type header without
                //   charset parameter incorrectly to mean "recipient should guess."
                //   Senders wishing to defeat this behavior MAY include a charset
                //   parameter even when the charset is ISO-8859-1 and SHOULD do so when
                //   it is known that it will not confuse the recipient.
                //
                // Which is funny because that's exactly what the above code has been doing, and it seems to be
                // more reliable than depending on a server to conform to a spec.
                //
                // RFC 7231 goes on to say:
                //   The default charset of ISO-8859-1 for text media types has been
                //   removed; the default is now whatever the media type definition says.
                //
                // In other words, not HTTP's problem. Each MIME type gets its own default charset. Unless that service
                // was written with old information in mind.
                //
                // The default charset for "application/json" is utf-8.
                // See https://tools.ietf.org/html/rfc4627#section-3
                //
                // "text/xml" is either iso-8859-1 (8-bit), or us-ascii (7-bit). It could be Windows-1252 which is
                // often used interchangeably with iso-8859-1 even though they differ in 32 codepoints. That's what
                // System.Net.WebClient was about to do for us. See RFC 3023 "XML Media Types, 2001" for the origins
                // of this confusion.
                //
                // RFC 7303 "XML Media Types, 2014" approaches sanity and says "text/xml" should be treated as an alias
                // for "application/xml".
                //
                // "application/xml" avoids the whole issue and says let your XML parser figure it out. That's
                // actually pretty reasonable. Content-Type might be missing a charset, or we might not be using
                // HTTP at all, so we have the "encoding" attribute on XML documents.
                //
                // The calling code optimistically won't call this method if the Content-Type contains "xml" and
                // let the parser figure it out. We miss the opportunity to use the "charset" if one was specified,
                // but _hopefully_ XML documents contain an "encoding" attribute. A slightly better solution might
                // be to check the XML document for an encoding, if none exists use the Content-Type's charset.
                enc = Encoding.UTF8;
            }

            // Calculate BOM length based on encoding guess. Then check for it in the data.
            if (bomLengthInData == -1)
            {
                var preamble = enc.GetPreamble();
                if (ByteArrayHasPrefix(preamble, data))
                {
                    bomLengthInData = preamble.Length;
                }
                else
                {
                    bomLengthInData = 0;
                }
            }

            // Convert byte array to string stripping off any BOM before calling GetString().
            // This is required since GetString() doesn't handle stripping off BOM.
            return enc.GetString(data, bomLengthInData, data.Length - bomLengthInData);
        }
    }
}
