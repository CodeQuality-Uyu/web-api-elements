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
        private readonly Func<TException, ExceptionThrownContext, string>? _messageFunction;

        private readonly Func<TException, ExceptionThrownContext, string>? _logMessageFunction;

        private readonly Func<TException, ExceptionThrownContext, string>? _codeFunction;

        private readonly Func<TException, ExceptionThrownContext, HttpStatusCode>? _statusCodeFunction;

        private readonly Func<TException, ExceptionThrownContext, (string code, HttpStatusCode statusCode, string message, string? logMessage)>? _function;

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
            Func<TException, ExceptionThrownContext, string>? logMessageFunction = null) : base(string.Empty, statusCode, string.Empty, string.Empty)
        {
            this._codeFunction = codeFunction;
            this._messageFunction = messageFunction;
            this._logMessageFunction = logMessageFunction ?? messageFunction;
        }

        public DinamicExceptionResponse(
            Func<TException, ExceptionThrownContext, string> codeFunction,
            Func<TException, ExceptionThrownContext, HttpStatusCode> statusCodeFunction,
            Func<TException, ExceptionThrownContext, string> messageFunction,
            Func<TException, ExceptionThrownContext, string>? logMessageFunction = null)
        {
            this._codeFunction = codeFunction;
            this._statusCodeFunction = statusCodeFunction;
            this._messageFunction = messageFunction;
            this._logMessageFunction = logMessageFunction ?? messageFunction;
        }

        public DinamicExceptionResponse(Func<TException, ExceptionThrownContext, (string code, HttpStatusCode statusCode, string message, string? logMessage)> function)
        {
            this._function = function;
        }

        public override void SetContext(ExceptionThrownContext context)
        {
            base.SetContext(context);

            base.Code = this._codeFunction != null ? this.BuildElement(this._codeFunction) : base.Code;
            base.StatusCode = this._statusCodeFunction != null ? this.BuildElement(this._statusCodeFunction) : base.StatusCode;
            base.Message = this._messageFunction != null ? this.BuildElement(this._messageFunction) : base.Message;
            base.LogMessage = this._logMessageFunction != null ? this.BuildElement(this._logMessageFunction) : base.LogMessage;
            (base.Code, base.StatusCode, base.Message, base.LogMessage) = this._function != null ? this.BuildElement(this._function) : (base.Code, base.StatusCode, base.Message, base.LogMessage ?? base.Message);
        }

        private TElement BuildElement<TElement>(Func<TException, ExceptionThrownContext, TElement> function)
        {
            var concreteException = (TException)base.Context.Exception;

            return function(concreteException, base.Context);
        }
    }
}
