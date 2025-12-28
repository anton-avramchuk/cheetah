namespace Cheetah.Core.Domain;

public interface IUpdatedAtEntity
{
    DateTimeOffset? UpdatedAt { get; set; }
}