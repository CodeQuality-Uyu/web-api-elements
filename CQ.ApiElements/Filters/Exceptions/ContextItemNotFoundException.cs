
namespace CQ.ApiElements.Filters.Exceptions;

internal sealed class ContextItemNotFoundException(ContextItems item) : Exception
{
    public readonly ContextItems Item = item;

    public static void Throw(ContextItems item)
    {
        throw new ContextItemNotFoundException(item);
    }
}
