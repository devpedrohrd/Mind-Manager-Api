namespace Mind_Manager.Api.Middlewares.ExceptionHandlers;

public class ArgumentExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => exception is ArgumentException;

    public (int statusCode, object response) Handle(Exception exception)
    {
        return (400, new ErrorResponse
        {
            Message = exception.Message,
            ErrorCode = "BAD_REQUEST",
            Timestamp = DateTime.UtcNow
        });
    }
}
