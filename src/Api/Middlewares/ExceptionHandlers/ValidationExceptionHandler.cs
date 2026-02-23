using Mind_Manager.Domain.Exceptions;

namespace Mind_Manager.Api.Middlewares.ExceptionHandlers;

public class ValidationExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => exception is ValidationException;

    public (int statusCode, object response) Handle(Exception exception)
    {
        var ex = (ValidationException)exception;
        return (400, new ErrorResponse
        {
            Message = ex.Message,
            ErrorCode = "VALIDATION_ERROR",
            Timestamp = DateTime.UtcNow
        });
    }
}
