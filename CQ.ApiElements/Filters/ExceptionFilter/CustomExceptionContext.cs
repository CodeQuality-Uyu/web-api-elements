using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public record CustomExceptionContext
    {
        public Exception Exception { get; init; }

        public string ControllerName { get; init; }

        public CustomExceptionContext(Exception exception, string controllerName)
        {
            Exception = exception;
            ControllerName = controllerName;
        }
    }
}
