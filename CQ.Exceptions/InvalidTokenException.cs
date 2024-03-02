using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.Exceptions
{
    public class InvalidTokenException : Exception
    {
        public readonly string Token;

        public InvalidTokenException(
            string token,
            Exception? inner = null) 
            : base(inner?.Message, inner)
        {
            Token = token;
        }
    }
}
