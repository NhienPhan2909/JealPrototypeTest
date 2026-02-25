namespace JealPrototype.Application.Exceptions;

/// <summary>
/// Exception thrown when credentials are not found
/// </summary>
public class CredentialNotFoundException : Exception
{
    public CredentialNotFoundException(string message) : base(message) { }

    public CredentialNotFoundException(string message, Exception innerException) 
        : base(message, innerException) { }
}
