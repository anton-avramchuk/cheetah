namespace Cheetah.Core.Exceptions;

public class CrmException: Exception
{
    public CrmException()
    {
        
    }
    
    public CrmException(string message) : base(message) { }

    public CrmException(string message, Exception innerException) : base(message, innerException) { }
}