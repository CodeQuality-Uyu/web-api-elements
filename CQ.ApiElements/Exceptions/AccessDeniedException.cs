using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.Api.Filters.Exceptions
{
    internal class AccessDeniedException : Exception
    {
        public string Permission { get; set; }

        public AccessDeniedException(string permission) { this.Permission = permission; }
    }
}
