namespace Cheetah.Core.Domain;

public interface IUpdatedAtEntity
{
    DateTimeOffset? UpdatedAt { get; }
}

public interface IRemovedAtEntity
{
    DateTimeOffset? RemovedAt { get; }
}