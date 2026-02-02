using Crm.Recruitment.DataAccess;
using Cheetah.Core.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Crm.Recruitment.Api.Tests.Fixtures;

public class RecruitmentApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("recruitment_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<RecruitmentDbContext>) ||
                            d.ServiceType == typeof(RecruitmentDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<RecruitmentDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            services.AddSingleton<IEventBus, NullEventBus>();

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RecruitmentDbContext>();
            db.Database.Migrate();
        });

        builder.UseEnvironment("Testing");
    }
}

internal class NullEventBus : IEventBus
{
    public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent => ValueTask.CompletedTask;

    public ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent => ValueTask.CompletedTask;

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
    }
}

[CollectionDefinition("RecruitmentApi")]
public class RecruitmentApiCollection : ICollectionFixture<RecruitmentApiFixture>
{
}