using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    internal sealed record class DinamicExceptionResponse<TException> : ExceptionResponse
        where TException : Exception
    {
        private readonly Func<TException, ExceptionThrownContext, string> _messageFunction;

        private readonly Func<TException, ExceptionThrownContext, string> _logMessageFunction;

        private readonly Func<TException, ExceptionThrownContext, string>? _codeFunction;

        public DinamicExceptionResponse(
            string code,
            HttpStatusCode statusCode,
            Func<TException, ExceptionThrownContext, string> messageFunction,
            Func<TException, ExceptionThrownContext, string>? logMessageFunction = null) : base(code, statusCode, string.Empty, string.Empty)
        {
            this._messageFunction = messageFunction;
            this._logMessageFunction = logMessageFunction ?? messageFunction;
        }

        public DinamicExceptionResponse(
            Func<TException, ExceptionThrownContext, string> codeFunction,
            HttpStatusCode statusCode,
            Func<TException, ExceptionThrownContext, string> messageFunction,
            Func<TException, ExceptionThrownContext, string>? logMessageFunction = null) : base(null, statusCode, string.Empty, string.Empty)
        {
            this._codeFunction = codeFunction;
            this._messageFunction = messageFunction;
            this._logMessageFunction = logMessageFunction ?? messageFunction;
        }

        public override void SetContext(ExceptionThrownContext context)
        {
            base.SetContext(context);

            this.Message = this.BuildMessage(this._messageFunction);
            this.LogMessage = this.BuildMessage(this._logMessageFunction);
            this.Code ??= this.BuildMessage(this._codeFunction);
        }

        private string BuildMessage(Func<TException, ExceptionThrownContext, string> messageFunction)
        {
            if (base.Context == null) return "Context of exception unknown";

            var exception = (TException)base.Context.Exception;

            if (exception == null) return $"Unknown exception: {base.Context?.Exception}";

            return messageFunction(exception, base.Context);
        }
    }
}
