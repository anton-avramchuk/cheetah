var builder = DistributedApplication.CreateBuilder(args);


var adminApi= builder.AddProject<Projects.Cheetah_Admin_Api>("cheetah-admin-api");


var adminClient=builder.AddProject<Projects.Cheetah_Admin_Client>("cheetah-admin-client")
        .WithReference(adminApi)
        .WaitFor(adminApi)
    ;


builder.Build().Run();