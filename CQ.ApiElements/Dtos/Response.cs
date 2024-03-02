
namespace CQ.ApiElements.Dtos
{
    public abstract record class Response<TEntity>
    {
        public Response(TEntity entity)
        {
        }
    }
}
