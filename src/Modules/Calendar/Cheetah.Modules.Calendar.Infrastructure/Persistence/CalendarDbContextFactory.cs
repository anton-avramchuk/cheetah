using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cheetah.Modules.Calendar.Infrastructure.Persistence;

/// <summary>Design-time фабрика для <c>dotnet ef migrations</c>.</summary>
public class CalendarDbContextFactory : IDesignTimeDbContextFactory<CalendarDbContext>
{
    public CalendarDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CalendarDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=calendar;Username=postgres;Password=postgres");
        return new CalendarDbContext(optionsBuilder.Options);
    }
}
