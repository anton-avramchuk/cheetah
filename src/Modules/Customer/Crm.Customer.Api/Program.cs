using Cheetah.AspNetCore.Extensions;
using Crm.Customer.Api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

Bootstrap.Start(builder.Services);

var app = builder.Build();

app.MapDefaultEndpoints();

app.InitializeApplication();

app.Run();

public partial class Program
{
}