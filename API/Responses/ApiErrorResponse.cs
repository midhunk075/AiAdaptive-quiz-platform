namespace API.Responses
{
    public class ApiErrorResponse(int statusCode, string message, string? traceId = null)
    {
        public bool Success { get; } = false;
        public int StatusCode { get; } = statusCode;
        public string Message { get; } = message;
        public string? TraceId { get; } = traceId;
    }
}
