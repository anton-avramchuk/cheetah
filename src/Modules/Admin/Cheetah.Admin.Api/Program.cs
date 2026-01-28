using Cheetah.Admin.Api;
using Cheetah.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

Bootstrap.Start(builder.Services);

var app = builder.Build();

app.InitializeApplication();


app.UseHttpsRedirection();


app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
