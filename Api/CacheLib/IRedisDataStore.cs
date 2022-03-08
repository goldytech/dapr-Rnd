namespace Api.CacheLib;

public interface IRedisDataStore
{
    Task<string> GetStringAsync(string key);
    Task<bool> AddStringAsync(string key, string value, int timeToLiveinSeconds);
    
}