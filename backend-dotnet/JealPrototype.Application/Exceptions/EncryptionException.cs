namespace JealPrototype.Application.Exceptions;

/// <summary>
/// Exception thrown when encryption operations fail.
/// </summary>
public class EncryptionException : Exception
{
    public EncryptionException(string message) : base(message)
    {
    }

    public EncryptionException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
