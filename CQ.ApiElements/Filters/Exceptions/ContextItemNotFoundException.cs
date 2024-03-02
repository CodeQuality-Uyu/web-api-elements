using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters.Exceptions
{
    internal sealed class ContextItemNotFoundException : Exception
    {
        public readonly ContextItems Item;

        public ContextItemNotFoundException(ContextItems item)
        {
            this.Item = item;
        }
    }
}
