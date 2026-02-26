using Mind_Manager.Domain.Exceptions;

namespace Mind_Manager.Api.Middlewares.ExceptionHandlers;

public class UnauthorizedExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => exception is UnauthorizedException;

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
