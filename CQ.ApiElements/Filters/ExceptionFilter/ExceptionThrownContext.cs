using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public record class ExceptionThrownContext(Exception Exception, string ControllerName, string Action);
}
