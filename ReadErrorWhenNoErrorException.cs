using System;
using System.Runtime.Serialization;

namespace Dell.Premier.Web.Common.HttpClient
{
    /// <summary>
    ///     <see cref="ReadErrorWhenNoErrorException" /> is thrown when <see cref="HttpResponseExtensions.ReadError{T}" />
    ///     is called and <see cref="HttpResponse.Success" /> is true.
    /// </summary>
    [Serializable]
    public class ReadErrorWhenNoErrorException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="ReadErrorWhenNoErrorException" /> class.</summary>
        public ReadErrorWhenNoErrorException()
            : base("ReadError called when no error exists")
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ReadErrorWhenNoErrorException" /> class.</summary>
        /// <param name="message">The exception message.</param>
        public ReadErrorWhenNoErrorException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ReadErrorWhenNoErrorException" /> class with serialized data.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="SerializationInfo" /> that holds the serialized object data about
        ///     the exception being thrown.
        /// </param>
        /// <param name="context">
        ///     The <see cref="StreamingContext" /> that contains contextual information about
        ///     the source or destination.
        /// </param>
        /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null.</exception>
        /// <exception cref="SerializationException">
        ///     The class name is null or <see cref="P:System.Exception.HResult" />
        ///     is zero (0).
        /// </exception>
        protected ReadErrorWhenNoErrorException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
