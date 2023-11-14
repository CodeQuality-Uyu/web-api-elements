using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Exceptions
{
    public class InvalidRequestException : Exception
    {
        public readonly string Prop;

        public readonly string Value;

        public InvalidRequestException(
            string prop,
            string value,
            Exception inner) : base(inner.Message, inner)
        {
            Prop = prop;
            Value = value;
        }
    }
}
