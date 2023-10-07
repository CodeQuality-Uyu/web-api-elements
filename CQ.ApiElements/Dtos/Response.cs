using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Dtos
{
    public abstract class Response<TEntity>
    {
        public Response(TEntity entity)
        {
            Map(entity);
        }

        protected abstract void Map(TEntity entity);
    }
}
