using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Api.CacheLib;

public class RedisConnectionOptions : IOptions<RedisConnectionOptions>
{
    public RedisConnectionOptions()
    {
        ConfigurationOptions = new ConfigurationOptions();
    }
    public string ConnectionString { get; set; }
    public ConfigurationOptions ConfigurationOptions { get; set; }
    RedisConnectionOptions IOptions<RedisConnectionOptions>.Value => this;
}