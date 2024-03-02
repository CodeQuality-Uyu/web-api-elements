using CQ.Exceptions;

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
