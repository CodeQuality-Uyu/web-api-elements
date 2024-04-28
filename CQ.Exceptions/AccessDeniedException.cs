namespace CQ.Exceptions
{
    public class AccessDeniedException : Exception
    {
        public readonly string Permission;

        public AccessDeniedException(string permission)
        { 
            this.Permission = permission; 
        }

        public static void Throw(string permission)
        {
            throw new AccessDeniedException(permission);
        }
    }
}
