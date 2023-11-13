using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public sealed record class OriginError(string ControllerName, string Action);
}
