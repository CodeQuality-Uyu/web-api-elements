using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CQ.ApiElements.Filters
{
    public abstract class ExceptionFilter : Attribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var errorResponse = this.BuildErrorResponse(context.Exception);

            string message;
            string code;
            int statusCode;
            if (errorResponse is null)
            {
                (message, code, statusCode) = BuildGenericErrorMessage(context);
            }
            else
            {
                message = errorResponse.Message;
                code = errorResponse.InnerCode;
                statusCode = errorResponse.StatusCode;
            }

            context.Result = new ObjectResult(new
            {
                message,
                code,
            })
            {
                StatusCode = statusCode
            };
        }

        private (string message, string code, int statusCode) BuildGenericErrorMessage(ExceptionContext context)
        {
            string message;
            string code;
            int statusCode;

            try
            {
                throw context.Exception;
            }
            catch (ArgumentNullException ex)
            {
                message = ex.Message ?? $"Missing or invalid {ex.ParamName}";
                code = "InvalidProp";
                statusCode = 400;
            }
            catch (Exception)
            {
                message = "Problems with the server";
                code = "InternalProblem";
                statusCode = 500;
            }


            return (message, code, statusCode);
        }

        protected abstract HttpException BuildErrorResponse(Exception exception);
    }

    public sealed class HttpException
    {
        public int StatusCode { get; set; }

        public string InnerCode { get; set; }

        public string Message { get; set; }
    }
}