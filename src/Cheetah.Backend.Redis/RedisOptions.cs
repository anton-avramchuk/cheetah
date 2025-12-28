namespace Cheetah.Backend.Redis;

public class RedisOptions
{
    public Dictionary<string, RedisInstanceOptions> Instances { get; set; } = new();
}

public class RedisInstanceOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public int Database { get; set; } = 0;
}
