namespace Cheetah.Core.Exceptions;

public class CrmExceptionInitialization : CrmException
{
    

    public CrmExceptionInitialization(string message)
        : base(message)
    {

    }

    public CrmExceptionInitialization(string message, Exception innerException)
        : base(message, innerException)
    {

    }

}