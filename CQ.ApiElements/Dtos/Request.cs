using CQ.ApiElements.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Dtos
{
    public abstract record class Request<TEntity>
    {
        public TEntity Map()
        {
            try
            {
                Assert();

                return InnerMap();
            }
            catch(ArgumentException ex)
            {
                throw new InvalidRequestException(ex.ParamName, ex.Source, ex);
            }
        }

        protected virtual void Assert() { }

        protected abstract TEntity InnerMap();
    }
}
