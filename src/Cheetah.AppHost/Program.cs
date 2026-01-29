var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin();

var adminDb = postgres.AddDatabase("AdminDb", "cheetah_admin");

var redis = builder.AddRedis("redis")
    .WithRedisInsight();

var adminApi = builder.AddProject<Projects.Cheetah_Admin_Api>("admin-api")
    .WithReference(adminDb)
    .WithEnvironment("Redis__Instances__default__ConnectionString",
        redis.Resource.ConnectionStringExpression)
    .WaitFor(adminDb)
    .WaitFor(redis);

builder.AddProject<Projects.Cheetah_Admin_Client>("admin-client")
    .WithReference(adminApi)
    .WithEnvironment("AdminClients__BaseUrl", adminApi.GetEndpoint("https"))
    .WaitFor(adminApi);

builder.Build().Run();
