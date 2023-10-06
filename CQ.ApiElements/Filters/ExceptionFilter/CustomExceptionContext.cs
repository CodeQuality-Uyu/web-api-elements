using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public class CustomExceptionContext
    {
        public Exception Exception { get; set; }

        public string ControllerName { get; set; }
    }
}
