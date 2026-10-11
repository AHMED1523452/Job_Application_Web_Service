
namespace JobApplication.Application.CustomExcpetions
{
    public class IDempotencyException : Exception
    {
        public int StatusCode { get; }
        //. bringing the message from the base or the father or from the exception class to the child class or the derived class
        public IDempotencyException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
