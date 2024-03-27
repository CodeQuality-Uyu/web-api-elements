using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.Exceptions
{
    public class InvalidHeaderException : Exception
    {
        public readonly string Value;

        public readonly string Header;

        public InvalidHeaderException(
            string header,
            string value,
            Exception? inner = null) 
            : base(inner?.Message, inner)
        {
            Header = header;
            Value = value;
        }
    }
}
