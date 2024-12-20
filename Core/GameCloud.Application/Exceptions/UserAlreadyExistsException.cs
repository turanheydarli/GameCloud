namespace GameCloud.Application.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string email) : base($"A user with the email '{email}' already exists.")
    {
    }
}