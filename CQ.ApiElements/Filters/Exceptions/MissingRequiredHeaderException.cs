using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.Exceptions
{
    public class MissingRequiredHeaderException : Exception 
    {
        public readonly string Header;

        public MissingRequiredHeaderException(string header)
        {
            Header = header;
        }
    }
}
