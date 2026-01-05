using Cheetah.AspNetCore.Extensions;
using Cheetah.Core.EntityFramework.Tenants.Extensions;
using Cheetah.Crm;
using Cheetah.Tenants.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Start();

var app = builder.Build();


app.UseHttpsRedirection();

app.InitializeApplication();


app.Run();
