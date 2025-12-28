namespace Cheetah.Core.DataAccess;

public record CrmConnectionStringCheckResult
{
    public bool Connected { get; set; }

    public bool DatabaseExists { get; set; }
}