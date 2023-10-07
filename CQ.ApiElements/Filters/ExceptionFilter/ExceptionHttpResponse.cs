using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public class ExceptionHttpResponse
    {
        public static ExceptionHttpResponse Default = new ExceptionHttpResponse
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Code = "ExceptionOccurred",
            Message = "An exception has occurred."
        };

        public string Message { get; set; }

        public string Code { get; set; }

        public HttpStatusCode StatusCode { get; set; }
    }
}
