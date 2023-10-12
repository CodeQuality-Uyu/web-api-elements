using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public class ExceptionMapping<TException> where TException : Exception
    {
        public string BaseLogMessage { get; set; }

        public Func<TException, CustomExceptionContext, string> LogMessage { get; set; }

        public Func<TException, CustomExceptionContext, object> ExtraInformationGetter { get; set; }

        public HttpStatusCode ResponseStatusCode { get; set; }

        public Func<TException, CustomExceptionContext, string> ResponseMessage { get; set; }

        public string ResponseCode { get; set; }

        public string ControllerName { get; set; }

        public bool IsDefault { get; set; }

        public string LogLevel { get; set; }
    }
}
