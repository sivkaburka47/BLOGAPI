namespace Blog.API.Middleware
{
    public class ValidationAccessException : Exception
    {
        public ValidationAccessException(string message = "The entered data is invalid") : base(message)
        {
        }
    }
}