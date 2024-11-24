
namespace CQ.ApiElements.Filters.Exceptions;

internal sealed class ContextItemNotFoundException(ContextItem item)
    : Exception
{
    public readonly ContextItem Item = item;

    public static void Throw(ContextItem item)
    {
        throw new ContextItemNotFoundException(item);
    }
}
