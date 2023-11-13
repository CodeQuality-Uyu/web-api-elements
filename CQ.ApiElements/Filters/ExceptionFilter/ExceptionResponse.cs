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
        public string Code;

        public string Message { get; protected set; }

        public string LogMessage { get; protected set; }

        public readonly HttpStatusCode StatusCode;

        public ExceptionThrownContext? Context;

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
