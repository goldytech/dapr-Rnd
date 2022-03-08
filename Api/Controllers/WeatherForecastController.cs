using System.Text.Json;
using Api.CacheLib;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IRedisDataStore _redisDataStore;
    private readonly DaprClient _daprClient;


    public WeatherForecastController(ILogger<WeatherForecastController> logger, IRedisDataStore redisDataStore, DaprClient daprClient)
    {
        _logger = logger;
        _redisDataStore = redisDataStore;
        _daprClient = daprClient;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>?> Get()
    {
        var data = await _redisDataStore.GetStringAsync("weather-data");
        

       
        if (data is null)
        {
           var weatherData = Enumerable.Range(1, 5).Select(index =>
                
                    new WeatherForecast
                    {
                        Date = DateTime.Now.AddDays(index),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                    })
              
                .ToArray();

           var jsonData = JsonSerializer.Serialize(weatherData);

          var isSuccess = await _redisDataStore.AddStringAsync("weather-data", jsonData, 500000);
          return weatherData;

        }
        return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(data);
    }

    [HttpGet("{id}",Name = "Weather forecast by Id")]
    public async Task<WeatherForecast> Get(string id)
    {
        var weatherForecast = await GetDataFromRedis(id);
        if (weatherForecast is null)
        {
            weatherForecast = new WeatherForecast
            {
                Date = DateTime.Now.AddDays(Random.Shared.Next(10)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            };
            await SetDataInRedis(id, weatherForecast);
        }

        return weatherForecast;
    }


    private async Task<WeatherForecast> GetDataFromRedis(string key)
    {
        var data = await _daprClient.GetStateAsync<WeatherForecast>("weather", key);
        return data;
    }

    private async Task SetDataInRedis(string key, WeatherForecast weatherForecast)
    {
        await _daprClient.SaveStateAsync("weather",key,weatherForecast,new StateOptions {Concurrency = ConcurrencyMode.LastWrite}, 
            new Dictionary<string, string>
            {
                {"ttlInSecononds", "50000"}
            });
    }
}