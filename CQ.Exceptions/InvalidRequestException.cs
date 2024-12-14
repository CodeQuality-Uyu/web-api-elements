namespace CQ.Exceptions
{
    public class InvalidRequestException : Exception
    {
        public readonly string Prop;

        public readonly string Value;

        public InvalidRequestException(
            string prop,
            string value,
            Exception? inner = null) 
            : base(inner?.Message, inner)
        {
            Prop = prop;
            Value = value;
        }

        public static void Throw(string prop, string value)
        {
            throw new InvalidRequestException(prop, value);
        }
    }
}
