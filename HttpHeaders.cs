using System;
using System.Collections;
using System.Collections.Generic;

namespace Dell.Premier.Web.Common.HttpClient
{
    /// <summary><see cref="HttpHeaders" /> represents a collection of HTTP header values.</summary>
    public class HttpHeaders : IEnumerable<FieldValuePair>
    {
        private readonly Dictionary<string, List<FieldValuePair>> _headers = new Dictionary<string, List<FieldValuePair>>();

        /// <summary>
        ///     Indexer to <see cref="Get" /> or <see cref="Set" /> values within the header collection. If no value is
        ///     set for the <paramref name="field" /> the indexer returns an empty string; this indexer never returns
        ///     null. If the <paramref name="value" /> is set to null the header is removed from the collection. All
        ///     <see cref="Get" /> and <see cref="Set" /> operations on this indexer are case-insensitive.
        /// </summary>
        /// <param name="field">The field name (case-insensitive).</param>
        /// <returns>The value.</returns>
        public string this[string field]
        {
            get { return Get(field); }
            set { Set(field, value); }
        }

        /// <summary>Gets the enumerator.</summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the header collection.</summary>
        /// <returns>
        ///     A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the
        ///     header collection.
        /// </returns>
        public IEnumerator<FieldValuePair> GetEnumerator()
        {
            foreach (var fieldValuePairs in _headers.Values)
            {
                foreach (var fieldValuePair in fieldValuePairs)
                {
                    yield return fieldValuePair;
                }
            }
        }

        /// <summary>
        ///     Add adds a header entry for the <paramref name="field" /> and <paramref name="value" />. If the
        ///     <paramref name="field" /> already exists a new entry is appended to the header collection. If
        ///     <paramref name="value" /> is null nothing is added.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="field" /> is null.</exception>
        /// <param name="field">The field name.</param>
        /// <param name="value">The value.</param>
        public void Add(string field, string value)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (value == null)
            {
                return;
            }

            var lowerField = field.ToLowerInvariant();

            List<FieldValuePair> values;
            if (!_headers.TryGetValue(lowerField, out values))
            {
                values = new List<FieldValuePair>();
                _headers[lowerField] = values;
            }

            values.Add(new FieldValuePair(field, value));
        }

        /// <summary>
        ///     Delete removes the given <paramref name="field" /> (case-insensitive) from the header collection. If the
        ///     <paramref name="field" /> doesn't exist the method silently returns.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="field" /> is null.</exception>
        /// <param name="field">The field name (case-insensitive).</param>
        public void Delete(string field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            var lowerField = field.ToLowerInvariant();
            _headers.Remove(lowerField);
        }

        /// <summary>
        ///     <para>
        ///         Get returns the value of the specified <paramref name="field" /> (case-insensitive). If multiple
        ///         values are set for the header the first one is returned. If no value is set for the
        ///         <paramref name="field" /> the method returns an empty string. This method never returns null.
        ///     </para>
        ///     <para>
        ///         To retrieve all header values iterate over the <see cref="HttpHeaders" /> value.
        ///     </para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="field" /> is null.</exception>
        /// <param name="field">The field name (case-insensitive).</param>
        /// <returns>The value.</returns>
        public string Get(string field)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            var lowerField = field.ToLowerInvariant();

            List<FieldValuePair> values;
            if (!_headers.TryGetValue(lowerField, out values))
            {
                return string.Empty;
            }
            return values[0].Value;
        }

        /// <summary>
        ///     <para>
        ///         Set sets the specified <paramref name="field" /> (case-insensitive) to the
        ///         <paramref name="value" />. If <paramref name="value" /> is null the entry for
        ///         <paramref name="field" /> is deleted.
        ///     </para>
        ///     <para>
        ///         HTTP allows providing the same header multiple times. For that behavior, see <see cref="Add" />.
        ///     </para>
        ///     <para>
        ///         Calling <see cref="Set" /> sets the header value to the single <paramref name="value" />
        ///         specified, clearing previous entries for the same <paramref name="field" />, if any.
        ///     </para>
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="field" /> is null.</exception>
        /// <param name="field">The field name (case-insensitive).</param>
        /// <param name="value">The value.</param>
        public void Set(string field, string value)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));

            if (value == null)
            {
                Delete(field);
                return;
            }

            var lowerField = field.ToLowerInvariant();
            _headers[lowerField] = new List<FieldValuePair> { new FieldValuePair(field, value) };
        }
    }
}
