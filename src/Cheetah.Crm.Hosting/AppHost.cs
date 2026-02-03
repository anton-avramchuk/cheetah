var builder = DistributedApplication.CreateBuilder(args);



var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var adminDb = postgres.AddDatabase("AdminDb", "cheetah_admin");

var featuresDb = postgres.AddDatabase("FeaturesDb", "cheetah_features");

var redis = builder.AddRedis("redis")
    .WithDataVolume()
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


builder.AddProject<Projects.Crm_Features_Api>("crm-features-api")
    .WithReference(featuresDb)
    .WithEnvironment("Redis__Instances__default__ConnectionString",
        redis.Resource.ConnectionStringExpression)
    .WaitFor(redis)
    .WaitFor(featuresDb)
    ;


builder.Build().Run();