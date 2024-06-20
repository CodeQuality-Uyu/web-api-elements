using CQ.Exceptions;

namespace CQ.ApiElements.Dtos;
public abstract record class Request<TEntity>
{
    public TEntity Map()
    {
        try
        {
            return InnerMap();
        }
        catch (ArgumentException ex)
        {
            throw new InvalidRequestException(ex.ParamName, ex.Source, ex);
        }
    }

    protected abstract TEntity InnerMap();
}
