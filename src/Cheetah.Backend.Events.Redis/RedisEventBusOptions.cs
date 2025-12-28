namespace Cheetah.Backend.Events.Redis;

public class RedisEventBusOptions
{
    /// <summary>
    /// Redis instance name to use for event bus
    /// </summary>
    public string InstanceName { get; set; } = "default";

    /// <summary>
    /// Prefix for event channel names
    /// </summary>
    public string ChannelPrefix { get; set; } = "events:";
}
