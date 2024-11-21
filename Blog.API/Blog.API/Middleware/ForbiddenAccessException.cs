namespace Blog.API.Middleware
{
    public class ForbiddenAccessException : Exception
    {
        public ForbiddenAccessException(string message = "An attempt to access a forbidden resource") : base(message)
        {
        }
    }
}