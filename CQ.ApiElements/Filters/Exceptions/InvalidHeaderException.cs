
namespace CQ.ApiElements.Filters.Exceptions;

public class InvalidHeaderException(
    string header,
    string value,
    Exception? inner = null)
    : Exception(
        inner?.Message,
        inner)
{
    public readonly string Value = value;

    public readonly string Header = header;

    public static void Throw(string value, string header)
    {
        throw new InvalidHeaderException(value, header);
    }
}
