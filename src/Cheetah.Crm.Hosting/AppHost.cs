var builder = DistributedApplication.CreateBuilder(args);




var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var adminDb = postgres.AddDatabase("AdminDb", "cheetah_admin");

var featuresDb = postgres.AddDatabase("FeaturesDb", "cheetah_features");


var recruitmentDb=postgres.AddDatabase("Recruitment", "cheetah_recruitment");

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


var recruitment = builder.AddProject<Projects.Crm_Recruitment_Api>("crm-recruitment-api")
    .WithReference(recruitmentDb)
    .WithEnvironment("Redis__Instances__default__ConnectionString",
        redis.Resource.ConnectionStringExpression)
    .WaitFor(redis)
    .WaitFor(recruitmentDb)
    ;


builder.AddProject<Projects.Crm_Proxy>("crm-proxy")
    .WithReference(recruitment)
    .WaitFor(recruitment)
    ;

builder.Build().Run();