
namespace CQ.ApiElements.Filters.Exceptions;

public sealed class ExpiredHeaderException(
    string header,
    string value,
    Exception? inner = null)
    : Exception(
        inner?.Message,
        inner)
{
    public readonly string Header = header;

    public readonly string Value = value;

    public static void Throw(
        string header,
        string value)
    {
        throw new ExpiredHeaderException(header, value);
    }
}
