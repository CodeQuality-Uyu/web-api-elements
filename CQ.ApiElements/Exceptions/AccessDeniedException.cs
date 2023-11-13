using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.Api.Filters.Exceptions
{
    public class AccessDeniedException : Exception
    {
        public readonly string Permission;

        public AccessDeniedException(string permission) { this.Permission = permission; }
    }
}
