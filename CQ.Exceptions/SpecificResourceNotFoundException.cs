namespace CQ.Exceptions
{
    public sealed class SpecificResourceNotFoundException<TResource> : ResourceNotFoundException
    {
        public SpecificResourceNotFoundException(
            List<string> parameters,
            List<string> values)
            : base(parameters, values, typeof(TResource).Name)
        {
        }

        public SpecificResourceNotFoundException(string parameter, string value)
            : base(
                  new List<string> { parameter },
                  new List<string> { value },
                  typeof(TResource).Name)
        {
        }
    }
}
