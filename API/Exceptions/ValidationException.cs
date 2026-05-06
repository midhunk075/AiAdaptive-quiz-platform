namespace API.Exceptions
{
    public class ValidationException(string message) : AppException(message, 422);
}
