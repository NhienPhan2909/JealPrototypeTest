namespace JealPrototype.Application.Exceptions;

/// <summary>
/// Exception thrown when decryption operations fail.
/// </summary>
public class DecryptionException : Exception
{
    public DecryptionException(string message) : base(message)
    {
    }

    public DecryptionException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
