using Cheetah.Core.Domain;

namespace Crm.MasterData.Domain;

public class Location : Entity<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Country { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string Timezone { get; private set; } = null!;
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private Location() { }

    public static Location Create(string country, string city, string timezone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(timezone);
        return new Location { Id = Guid.NewGuid(), Country = country, City = city, Timezone = timezone };
    }

    public void Update(string country, string city, string timezone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(country);
        ArgumentException.ThrowIfNullOrWhiteSpace(city);
        ArgumentException.ThrowIfNullOrWhiteSpace(timezone);
        Country = country;
        City = city;
        Timezone = timezone;
    }
}
