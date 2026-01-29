using Cheetah.Admin.Api;
using Cheetah.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

Bootstrap.Start(builder.Services);

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseCors();

app.InitializeApplication();

app.UseHttpsRedirection();

app.Run();

// Required for WebApplicationFactory in integration tests
public partial class Program { }
