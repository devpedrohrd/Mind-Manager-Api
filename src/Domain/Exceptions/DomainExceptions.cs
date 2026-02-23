namespace Mind_Manager.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception inner) 
        : base(message, inner) { }
}

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }

    public BusinessException(string message, Exception inner) 
        : base(message, inner) { }
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }

    public ValidationException(string message, Exception inner) 
        : base(message, inner) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }

    public ForbiddenException(string message, Exception inner) 
        : base(message, inner) { }
}
