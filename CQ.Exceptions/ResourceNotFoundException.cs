namespace CQ.Exceptions
{
    public abstract class ResourceNotFoundException : Exception
    {
        public readonly string Resource;

        public readonly List<string> Parameters;

        public readonly List<string> Values;

        public ResourceNotFoundException(
            List<string> parameters,
            List<string> values,
            string resource = "Resource")
        {
            this.Parameters = parameters;
            this.Resource = resource;
            this.Values = values;
        }
    }
}
