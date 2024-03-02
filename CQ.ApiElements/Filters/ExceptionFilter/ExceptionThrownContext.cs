using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public record class ExceptionThrownContext(
        ExceptionContext ExceptionContext,
        Exception Exception,
        string ControllerName,
        string Action);
}
