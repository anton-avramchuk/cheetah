var builder = DistributedApplication.CreateBuilder(args);




var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .WithPgAdmin();

var adminDb = postgres.AddDatabase("AdminDb", "cheetah_admin");

var featuresDb = postgres.AddDatabase("FeaturesDb", "cheetah_features");


var recruitmentDb=postgres.AddDatabase("Recruitment", "cheetah_recruitment");


var candidatesDb=postgres.AddDatabase("Candidates", "cheetah_candidates");

var vacancyTasksDb=postgres.AddDatabase("VacancyTasks", "cheetah_vacancy_tasks");

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


var candidatesApi = builder.AddProject<Projects.Crm_Candidates_Api>("candidates-api")
        .WithReference(candidatesDb)
        .WithEnvironment("Redis__Instances__default__ConnectionString",
            redis.Resource.ConnectionStringExpression)
        .WaitFor(redis)
        .WaitFor(candidatesDb)
    ;

var vacancyTasksApi = builder.AddProject<Projects.Crm_VacancyTasks_Api>("vacancyTasks-api")
        .WithReference(vacancyTasksDb)
        .WithEnvironment("Redis__Instances__default__ConnectionString",
            redis.Resource.ConnectionStringExpression)
        .WaitFor(redis)
        .WaitFor(vacancyTasksDb)
    ;



var proxy = builder.AddProject<Projects.Crm_Proxy>("crm-proxy")
    .WithReference(recruitment)
    .WithReference(candidatesApi)
    .WithReference(vacancyTasksApi)
    .WaitFor(recruitment)
    .WaitFor(candidatesApi)
    .WaitFor(vacancyTasksApi)
    ;

builder.AddProject<Projects.Crm_Recruitment_Client>("recruitment-client")
    .WithReference(proxy)
    .WithEnvironment("Api__ApiUrl", proxy.GetEndpoint("https"))
    .WaitFor(proxy);


builder.Build().Run();