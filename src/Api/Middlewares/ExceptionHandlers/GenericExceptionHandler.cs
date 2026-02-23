namespace Mind_Manager.Api.Middlewares.ExceptionHandlers;

public class GenericExceptionHandler : IExceptionHandler
{
    public bool CanHandle(Exception exception) => true;

    public (int statusCode, object response) Handle(Exception exception)
    {
        return (500, new ErrorResponse
        {
            Message = "Erro interno do servidor.",
            ErrorCode = "INTERNAL_ERROR",
            Timestamp = DateTime.UtcNow
        });
    }
}
