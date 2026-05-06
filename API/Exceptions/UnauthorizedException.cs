namespace API.Exceptions
{
    public class UnauthorizedException(string message) : AppException(message, 401);
}
