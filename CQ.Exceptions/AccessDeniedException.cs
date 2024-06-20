namespace CQ.Exceptions
{
    public class AccessDeniedException(string permission) : Exception
    {
        public readonly string Permission = permission;

        public static void Throw(string permission)
        {
            throw new AccessDeniedException(permission);
        }
    }
}
