namespace JealPrototype.Application.Exceptions;

/// <summary>
/// Base exception for all EasyCars API errors
/// </summary>
public class EasyCarsException : Exception
{
    public int ResponseCode { get; }

    public EasyCarsException(string message, int responseCode = -1)
        : base(message)
    {
        ResponseCode = responseCode;
    }

    public EasyCarsException(string message, Exception innerException, int responseCode = -1)
        : base(message, innerException)
    {
        ResponseCode = responseCode;
    }
}

/// <summary>
/// Exception thrown when EasyCars authentication fails (ResponseCode 1)
/// </summary>
public class EasyCarsAuthenticationException : EasyCarsException
{
    public EasyCarsAuthenticationException(string message)
        : base(message, 1)
    {
    }
}

/// <summary>
/// Exception thrown for temporary EasyCars errors that should be retried (ResponseCode 5)
/// </summary>
public class EasyCarsTemporaryException : EasyCarsException
{
    public EasyCarsTemporaryException(string message)
        : base(message, 5)
    {
    }
}

/// <summary>
/// Exception thrown for validation errors in request data (ResponseCode 7)
/// </summary>
public class EasyCarsValidationException : EasyCarsException
{
    public EasyCarsValidationException(string message)
        : base(message, 7)
    {
    }
}

/// <summary>
/// Exception thrown for fatal/permanent EasyCars errors (ResponseCode 9)
/// </summary>
public class EasyCarsFatalException : EasyCarsException
{
    public EasyCarsFatalException(string message)
        : base(message, 9)
    {
    }
}

/// <summary>
/// Exception thrown for unknown/unexpected response codes
/// </summary>
public class EasyCarsUnknownException : EasyCarsException
{
    public EasyCarsUnknownException(string message, int responseCode)
        : base(message, responseCode)
    {
    }
}
