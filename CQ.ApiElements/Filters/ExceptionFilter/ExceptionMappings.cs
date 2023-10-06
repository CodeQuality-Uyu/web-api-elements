using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    public sealed class ExceptionMappings
    {
        public IList<ExceptionMapping> Mappings { get; set; }

        public ExceptionMappings()
        {
            Mappings = new List<ExceptionMapping>();
        }

        public ExceptionMapping GetMapping(string controllerName)
        {
            return Mappings.FirstOrDefault((ExceptionMapping m) => string.Compare(m.ControllerName, controllerName, StringComparison.OrdinalIgnoreCase) == 0) ?? Mappings.First();
        }
    }
}
