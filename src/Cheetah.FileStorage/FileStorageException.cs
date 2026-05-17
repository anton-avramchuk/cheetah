namespace Cheetah.FileStorage;

public class FileStorageException : Exception
{
    public FileStorageException(string message) : base(message) { }
    public FileStorageException(string message, Exception inner) : base(message, inner) { }
}

public sealed class FileStorageNotFoundException : FileStorageException
{
    public string Key { get; }
    public FileStorageNotFoundException(string key)
        : base($"File '{key}' not found")
    {
        Key = key;
    }
}
