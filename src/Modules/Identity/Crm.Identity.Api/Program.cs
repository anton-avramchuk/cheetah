using Cheetah.AspNetCore.Extensions;
using Crm.Identity.Api;

var builder = WebApplication.CreateBuilder(args);


Bootstrap.Start(builder.Services);

var app = builder.Build();


app.InitializeApplication();

app.Run();

public partial class Program
{
}