using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Dtos
{
    public abstract class Request<TEntity> where TEntity : class
    {
        public TEntity Map()
        {
            Assert();

            return InnerMap();
        }

        protected virtual void Assert() { }

        protected abstract TEntity InnerMap();
    }
}
