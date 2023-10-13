using CQ.ApiElements.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Filters.Extension
{
    public static class CollectionExtension
    {
        public static IList<TResult> MapTo<TResult, TEntity>(this IList<TEntity> collection)
            where TEntity : class
            where TResult : Response<TEntity>
        {
            return collection.Select(e => (TResult)Activator.CreateInstance(typeof(TResult), e)).ToList();
        }
    }
}
