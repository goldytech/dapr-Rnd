using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Api.CacheLib;

public class RedisDataStore : IRedisDataStore, IDisposable
{
    private readonly ILogger<RedisDataStore> _logger;
    private readonly RedisConnectionOptions? _options;
    private volatile IConnectionMultiplexer? _connection;
    private IDatabase? _cache;
    private bool _disposed;
    private readonly SemaphoreSlim _connectionLock = new(initialCount: 1, maxCount: 1);

    public RedisDataStore(IOptions<RedisConnectionOptions> options, ILogger<RedisDataStore> logger)
    {
        _logger = logger;
        ArgumentNullException.ThrowIfNull(nameof(options));
        _options = options.Value;
    }

    public async Task<string> GetStringAsync(string key)
    {
       ArgumentNullException.ThrowIfNull(nameof(key));
        await ConnectAsync();
        var value = await _cache.StringGetAsync(key);
        return value;
    }

    public async Task<bool> AddStringAsync(string key, string value, int timeToLiveinSeconds)
    {
        ArgumentNullException.ThrowIfNull(nameof(key));
        ArgumentNullException.ThrowIfNull(nameof(timeToLiveinSeconds));
        await ConnectAsync();

        try
        {
            await _cache.StringSetAsync(key, new RedisValue(value), TimeSpan.FromSeconds(timeToLiveinSeconds));
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
       
    }

    private async Task ConnectAsync(CancellationToken token = default(CancellationToken))
    {
        CheckDisposed();
        token.ThrowIfCancellationRequested();

        if (_cache != null)
        {
            return;
        }

        await _connectionLock.WaitAsync(token).ConfigureAwait(false);
        try
        {
            if (_cache == null)
            {
                if (_options?.ConfigurationOptions is not null)
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions)
                        .ConfigureAwait(false);
                    
                    _logger.LogDebug("Connected successfully with Configuration");
                }
                else
                {
                    _connection = await ConnectionMultiplexer.ConnectAsync(_options?.ConnectionString)
                        .ConfigureAwait(false);
                    _logger.LogDebug("Connected successfully with Connectionstring");
                }


                _cache = _connection.GetDatabase();
            }
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _connection?.Close();
        _connection?.Dispose();
        _logger.LogDebug("Connection successfully disposed");
    }

    private void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(this.GetType().FullName);
        }
    }
}