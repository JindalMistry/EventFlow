namespace EventFlow.Application.Common
{
    public class ErrorResponse
    {
        public bool Success { get; set; } = false;

        public string Message { get; set; } = string.Empty;
    }
}