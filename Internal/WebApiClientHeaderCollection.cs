using System.Net;
using System.Text;

namespace Dell.Premier.Web.Common.HttpClient.Internal
{
    internal class WebApiClientHeaderCollection : WebHeaderCollection
    {
        private const int ApproximateAverageHeaderLineSize = 30;

        public new void AddWithoutValidate(string name, string value)
        {
            // This works around the trouble of having to add certain headers by their property name.
            //
            // It also works around a few issues in creating the values for headers:
            // - "Range" header: The AddRange method does not allow specifying "last N bytes"
            // - "Date" and "If-Modified-Since": The properties are DateTime and use an obsolete, though still
            //    acceptable, format. See RFC 5322.
            base.AddWithoutValidate(name, value);
        }

        public override string ToString()
        {
            // HttpWebRequest uses its "Header" property's ToString() method to get the string to write as headers.
            // The default implementation concatenates header values with the same field name as a comma separated
            // list. This version allows for specifying a header field more than once in the request, on independent
            // lines, which is useful if a comma is part of the grammar used to parse the header's value.
            if (Count == 0)
            {
                return "\r\n";
            }

            var sb = new StringBuilder(ApproximateAverageHeaderLineSize * Count);
            for (var i = 0; i < Count; i++)
            {
                var key = GetKey(i);
                var values = GetValues(i);
                if (values != null)
                {
                    foreach (var value in values)
                    {
                        sb.Append(key);
                        sb.Append(": ");
                        sb.Append(value).Append("\r\n");
                    }
                }
            }
            sb.Append("\r\n");
            return sb.ToString();
        }
    }
}
