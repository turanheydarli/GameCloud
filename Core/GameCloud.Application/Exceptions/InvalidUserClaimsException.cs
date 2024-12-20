namespace GameCloud.Application.Exceptions;

public class InvalidUserClaimsException : Exception
{
    public InvalidUserClaimsException(string message) : base(message)
    {
    }
}