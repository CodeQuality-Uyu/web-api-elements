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
            var exceptionOfController = Mappings.FirstOrDefault((ExceptionMapping m) => string.Compare(m.ControllerName, controllerName, StringComparison.OrdinalIgnoreCase) == 0);

            if(exceptionOfController != null) 
            {
                return exceptionOfController;
            }

            var defaultException = Mappings.FirstOrDefault(m => m.IsDefault);

            if(defaultException != null)
            {
                return defaultException;
            }

            return  Mappings.First();
        }
    }
}
