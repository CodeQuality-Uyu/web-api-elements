using CQ.ApiElements.Dtos;

namespace CQ.ApiElements.Extensions
{
    public static class CollectionExtension
    {
        public static List<TResult> MapTo<TResult, TEntity>(this List<TEntity> collection)
            where TEntity : class
            where TResult : Response<TEntity>
        {
            return collection.Select(e => (TResult)Activator.CreateInstance(typeof(TResult), e)).ToList();
        }
    }
}
