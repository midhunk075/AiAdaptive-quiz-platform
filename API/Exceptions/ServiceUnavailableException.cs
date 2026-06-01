namespace API.Exceptions
{
    public class ServiceUnavailableException(string message) : AppException(message, 503);
}
