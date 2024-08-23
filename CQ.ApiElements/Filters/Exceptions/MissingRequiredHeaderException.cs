namespace CQ.ApiElements.Filters.Exceptions;

public class MissingRequiredHeaderException(string header)
    : Exception
{
    public readonly string Header = header;

    public static void Throw(string header)
    {
        throw new MissingRequiredHeaderException(header);
    }
}
