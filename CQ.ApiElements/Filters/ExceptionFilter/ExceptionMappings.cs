using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters
{
    internal sealed class ExceptionMappings
    {
        private readonly IList<BaseExceptionMapping> Mappings;

        public ExceptionMappings(BaseExceptionMapping firstMapping)
        {
            Mappings = new List<BaseExceptionMapping>
            {
                firstMapping
            };
        }

        public void AddMapping(BaseExceptionMapping mapping)
        {
            Mappings.Add(mapping);
        }

        public BaseExceptionMapping GetMapping(string controllerName)
        {
            var exceptionOfController = Mappings.FirstOrDefault((m) => string.Compare(m.ControllerName, controllerName, StringComparison.OrdinalIgnoreCase) == 0);

            if(exceptionOfController != null) 
            {
                return exceptionOfController;
            }

            return  Mappings.First();
        }
    }
}
