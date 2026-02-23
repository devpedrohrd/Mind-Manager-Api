using Mind_Manager.Domain.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace Mind_Manager.Api.Middlewares.ExceptionHandlers;

public class ForbiddenExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => exception is ForbiddenException;

    public (int statusCode, object response) Handle(Exception exception)
    {
        var ex = (ForbiddenException)exception;
        return (403, new ErrorResponse
        {
            Message = ex.Message,
            ErrorCode = "FORBIDDEN",
            Timestamp = DateTime.UtcNow
        });
    }
}
