using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Web;

namespace Dell.Premier.Web.Common.HttpClient
{
    /// <summary><see cref="FormData" /> represents a collection of <see cref="FieldValuePair" /> values.</summary>
    [DebuggerDisplay("{GetFormEncodedValue()}")]
    public class FormData : IEnumerable<FieldValuePair>
    {
        private readonly List<FieldValuePair> _values = new List<FieldValuePair>();

        /// <summary>Initializes a new instance of <see cref="FormData" />.</summary>
        public FormData()
        {
        }

        /// <summary>Initializes a new instance of <see cref="FormData" /> from an object.</summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj" /> is null.</exception>
        /// <param name="obj">
        ///     The object's public instance properties with public getters will be converted
        ///     to <see cref="FieldValuePair" />'s. Each property will have have its name as the field and
        ///     <see cref="object.ToString" /> will be called to obtain its value. <c>null</c> properties will be
        ///     omitted from the list.
        /// </param>
        public FormData(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var getMethod = property.GetGetMethod();
                // skip if no public getter
                if (getMethod == null)
                {
                    continue;
                }

                var name = property.Name;
                var valueObject = getMethod.Invoke(obj, null);
                if (valueObject == null)
                {
                    continue;
                }
                var value = valueObject.ToString();

                Add(name, value);
            }
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
            foreach (var fieldValuePair in _values)
            {
                yield return fieldValuePair;
            }
        }

        /// <summary>Add adds a field-value pair to the collection. Field names do not need to be unique.</summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="field" /> or <paramref name="value" /> are null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="field" /> is empty string.</exception>
        /// <param name="field">The field name.</param>
        /// <param name="value">The value.</param>
        public void Add(string field, string value)
        {
            if (field == null)
                throw new ArgumentNullException(nameof(field));
            if (field == string.Empty)
                throw new ArgumentException("field cannot be empty string", nameof(field));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            _values.Add(new FieldValuePair(field, value));
        }

        /// <summary>GetFormEncodedValue returns a string in application/x-www-form-urlencoded format.</summary>
        /// <returns>The application/x-www-form-urlencoded value.</returns>
        public string GetFormEncodedValue()
        {
            var sb = new StringBuilder();
            foreach (var fieldValue in _values)
            {
                if (sb.Length != 0)
                {
                    sb.Append("&");
                }

                sb.AppendFormat("{0}={1}", HttpUtility.UrlEncode(fieldValue.Field), HttpUtility.UrlEncode(fieldValue.Value));
            }

            return sb.ToString();
        }

        /// <summary>GetBytes returns a byte array in application/x-www-form-urlencoded format.</summary>
        /// <returns>The application/x-www-form-urlencoded value.</returns>
        public byte[] GetBytes()
        {
            return Encoding.UTF8.GetBytes(GetFormEncodedValue());
        }
    }
}
