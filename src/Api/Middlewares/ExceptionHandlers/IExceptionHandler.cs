namespace Mind_Manager.Api.Middlewares.ExceptionHandlers;

public interface IExceptionHandler
{
    bool CanHandle(Exception exception);
    (int statusCode, object response) Handle(Exception exception);
}
