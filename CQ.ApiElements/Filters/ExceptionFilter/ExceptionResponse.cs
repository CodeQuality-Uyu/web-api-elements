using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public record class ExceptionResponse
    {
        public string Code { get; protected set; } = null!;

        public string Message { get; protected set; } = null!;

        public string LogMessage { get; protected set; } = null!;

        public HttpStatusCode StatusCode { get; protected set; }

        public ExceptionThrownContext Context { get; protected set; } = null!;

        public ExceptionResponse() { }

        public ExceptionResponse(
            string code,
            HttpStatusCode statusCode,
            string message,
            string? logMessage = null)
        {
            this.Code = code;
            this.StatusCode = statusCode;
            this.Message = message;
            this.LogMessage = logMessage ?? message;
        }

        public virtual void SetContext(ExceptionThrownContext context)
        {
            this.Context = context;
        }
    }
}
