using Mind_Manager.Domain.Exceptions;

namespace Mind_Manager.Api.Middlewares.ExceptionHandlers;

public class NotFoundExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => exception is NotFoundException;

    public (int statusCode, object response) Handle(Exception exception)
    {
        var ex = (NotFoundException)exception;
        return (404, new ErrorResponse
        {
            Message = ex.Message,
            ErrorCode = "NOT_FOUND",
            Timestamp = DateTime.UtcNow
        });
    }
}
