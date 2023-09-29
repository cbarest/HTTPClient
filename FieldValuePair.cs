using System.Diagnostics;

namespace Dell.Premier.Web.Common.HttpClient
{
    /// <summary>
    ///     <see cref="FieldValuePair" /> contains an immutable field value string pair.
    /// </summary>
    [DebuggerDisplay("{Field}: {Value}")]
    public class FieldValuePair
    {
        private readonly string _field;
        private readonly string _value;
        private readonly int _hashCode;

        /// <summary>Initializes a new instance of the <see cref="FieldValuePair" /> class.</summary>
        /// <param name="field">The field name.</param>
        /// <param name="value">The value.</param>
        public FieldValuePair(string field, string value)
        {
            _field = field;
            _value = value;
            _hashCode = string.Format("{0}={1}", _field, _value).GetHashCode();
        }

        /// <summary>Gets the field name.</summary>
        public string Field
        {
            get { return _field; }
        }

        /// <summary>Gets the value.</summary>
        public string Value
        {
            get { return _value; }
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            var value = obj as FieldValuePair;
            if (value == null)
            {
                return false;
            }
            return Field == value.Field && Value == value.Value;
        }

        /// <summary>Serves as a hash function for a particular type.</summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}
