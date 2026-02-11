using Crm.VacancyTasks.DataAccess;
using Crm.VacancyTasks.Domain;
using Cheetah.Core.Events;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Crm.VacancyTasks.Api.Tests.Fixtures;

public class VacancyTasksApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("vacancytasks_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public Guid DefaultStateId { get; private set; }

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
                .Where(d => d.ServiceType == typeof(DbContextOptions<VacancyTasksDbContext>) ||
                            d.ServiceType == typeof(VacancyTasksDbContext))
                .ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<VacancyTasksDbContext>(options =>
            {
                options.UseNpgsql(_dbContainer.GetConnectionString());
            });

            services.AddSingleton<IEventBus, NullEventBus>();

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<VacancyTasksDbContext>();
            db.Database.Migrate();

            var defaultState = TaskState.Create("To Do", 0, "#3498db", true);
            DefaultStateId = defaultState.Id;
            db.TaskStates.Add(defaultState);
            db.SaveChanges();
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

[CollectionDefinition("VacancyTasksApi")]
public class VacancyTasksApiCollection : ICollectionFixture<VacancyTasksApiFixture>
{
}