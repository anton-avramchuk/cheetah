using Cheetah.Core.Domain;

namespace Crm.Features.Domain;

public class Feature : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;


    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }
}