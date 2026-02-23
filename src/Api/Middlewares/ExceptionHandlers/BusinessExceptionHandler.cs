using Mind_Manager.Domain.Exceptions;

namespace Mind_Manager.Api.Middlewares.ExceptionHandlers;

public class BusinessExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => exception is BusinessException;

    public (int statusCode, object response) Handle(Exception exception)
    {
        var ex = (BusinessException)exception;
        return (400, new ErrorResponse
        {
            Message = ex.Message,
            ErrorCode = "BUSINESS_ERROR",
            Timestamp = DateTime.UtcNow
        });
    }
}
