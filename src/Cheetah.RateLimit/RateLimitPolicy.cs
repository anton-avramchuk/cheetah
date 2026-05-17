namespace Cheetah.RateLimit;

public class RateLimitPolicy
{
    /// <summary>Максимум запросов в окне.</summary>
    public long Limit { get; set; }

    /// <summary>Длина окна.</summary>
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
}
