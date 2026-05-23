using Cheetah.AspNetCore.Extensions;
using Crm.Proxy;

var builder = WebApplication.CreateBuilder(args);


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

app.UseCors();

app.InitializeApplication();


app.Run();