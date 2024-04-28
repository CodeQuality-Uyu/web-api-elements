namespace CQ.Exceptions
{
    public sealed class SpecificResourceDuplicatedException<TResource> : ResourceDuplicatedException
        where TResource : class
    {
        public SpecificResourceDuplicatedException(
            List<string> parameters,
            List<string> values)
            : base(
                  parameters,
                  values,
                  typeof(TResource).Name)
        {
        }

        public SpecificResourceDuplicatedException(
            string parameter,
            string value)
            : base(
                  new List<string> { parameter },
                  new List<string> { value },
                  typeof(TResource).Name)
        {
        }

        public static void Throw(string parameter, string value)
        {
            throw new SpecificResourceDuplicatedException<TResource>(parameter, value);
        }
    }
}
