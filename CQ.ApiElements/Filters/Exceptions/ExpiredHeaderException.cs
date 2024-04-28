using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters.Exceptions
{
    public sealed class ExpiredHeaderException : Exception
    {
        public readonly string Header;

        public readonly string Value;

        public ExpiredHeaderException(
            string header,
            string value,
            Exception? inner = null) 
            : base(inner?.Message, inner)
        {
            Header = header;
            Value = value;
        }

        public static void Throw(string header, string value)
        {
            throw new ExpiredHeaderException(header, value);
        }
    }
}
