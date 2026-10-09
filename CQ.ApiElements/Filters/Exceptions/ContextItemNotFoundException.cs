
namespace CQ.ApiElements.Filters.Exceptions;

internal sealed class ContextItemNotFoundException<TKey>(TKey item)
    : Exception
    where TKey : Enum
{
    public readonly TKey Item = item;

    public static void Throw(TKey item)
    {
        throw new ContextItemNotFoundException<TKey>(item);
    }
}
