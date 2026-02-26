namespace Mind_Manager.Api.Middlewares.ExceptionHandlers;

public class UnauthorizedAccessExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => exception is UnauthorizedAccessException;

    public (int statusCode, object response) Handle(Exception exception)
    {
        return (401, new ErrorResponse
        {
            Message = exception.Message,
            ErrorCode = "UNAUTHORIZED",
            Timestamp = DateTime.UtcNow
        });
    }
}
