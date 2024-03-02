namespace CQ.Exceptions
{
    public abstract class ResourceDuplicatedException : Exception
    {
        public readonly string Resource;

        public readonly List<string> Values;

        public readonly List<string> Parameters;

        public ResourceDuplicatedException(
            List<string> parameters,
            List<string> values,
            string resource = "Resource")
        {
            this.Resource = resource;
            this.Parameters = parameters;
            this.Values = values;
        }
    }
}
