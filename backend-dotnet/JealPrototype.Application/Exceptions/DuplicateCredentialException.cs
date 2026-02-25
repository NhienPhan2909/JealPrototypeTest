namespace JealPrototype.Application.Exceptions;

/// <summary>
/// Exception thrown when attempting to create duplicate credentials
/// </summary>
public class DuplicateCredentialException : Exception
{
    public DuplicateCredentialException(string message) : base(message) { }

    public DuplicateCredentialException(string message, Exception innerException) 
        : base(message, innerException) { }
}
