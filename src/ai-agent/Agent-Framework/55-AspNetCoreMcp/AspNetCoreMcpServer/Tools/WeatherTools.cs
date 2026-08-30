using ModelContextProtocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace _55_AspNetCoreMcpServer.Tools;

[McpServerToolType]
public sealed class WeatherTools(IHttpClientFactory httpClientFactory)
{
    [McpServerTool(Name = "get_china_weather_forecast"), Description("按中国城市名称查询实时天气和未来三天天气预报。")]
    public async Task<string> GetChinaWeatherForecast(
        [Description("中国城市名称，例如：北京、上海、广州、深圳。")]
        string city,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(city);

        var normalizedCity = city.Trim();
        var geocodingClient = httpClientFactory.CreateClient("ChinaWeatherGeocoding");
        var geocodingUrl = $"v1/search?name={Uri.EscapeDataString(normalizedCity)}&count=1&language=zh&format=json&countryCode=CN";
        var locationResult = await geocodingClient.GetFromJsonAsync<GeocodingResponse>(geocodingUrl, cancellationToken);
        var location = locationResult?.Results?.FirstOrDefault()
            ?? throw new McpException($"未找到中国城市“{normalizedCity}”，请尝试输入完整城市名称，例如“北京市”或“杭州市”。");

        var forecastClient = httpClientFactory.CreateClient("ChinaWeatherForecast");
        var forecastUrl = string.Create(
            CultureInfo.InvariantCulture,
            $"v1/forecast?latitude={location.Latitude}&longitude={location.Longitude}&current=temperature_2m,apparent_temperature,weather_code,wind_speed_10m&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_probability_max,wind_speed_10m_max&timezone=Asia%2FShanghai&forecast_days=3");
        var forecast = await forecastClient.GetFromJsonAsync<WeatherForecastResponse>(forecastUrl, cancellationToken)
            ?? throw new McpException("天气服务未返回有效的预报数据，请稍后重试。");

        var locationName = string.IsNullOrWhiteSpace(location.Admin1)
            ? location.Name
            : $"{location.Admin1} {location.Name}";
        var current = forecast.Current;
        var daily = forecast.Daily;
        var dailyForecasts = daily.Time.Select((date, index) => $"""
            {date}：{DescribeWeather(daily.WeatherCode[index])}，{daily.TemperatureMax[index]:0.#}～{daily.TemperatureMin[index]:0.#}℃，最高降水概率 {daily.PrecipitationProbabilityMax[index]}%，最大风速 {daily.WindSpeedMax[index]:0.#} km/h
            """);

        return $"""
            {locationName}天气
            当前：{DescribeWeather(current.WeatherCode)}，{current.Temperature:0.#}℃，体感 {current.ApparentTemperature:0.#}℃，风速 {current.WindSpeed:0.#} km/h

            未来三天
            {string.Join(Environment.NewLine, dailyForecasts)}

            数据来源：Open-Meteo（中国城市定位，时区 Asia/Shanghai）
            """;
    }

    private static string DescribeWeather(int code) => code switch
    {
        0 => "晴",
        1 => "大部晴朗",
        2 => "局部多云",
        3 => "阴",
        45 or 48 => "有雾",
        51 or 53 or 55 => "毛毛雨",
        56 or 57 => "冻毛毛雨",
        61 or 63 or 65 => "雨",
        66 or 67 => "冻雨",
        71 or 73 or 75 or 77 => "雪",
        80 or 81 or 82 => "阵雨",
        85 or 86 => "阵雪",
        95 => "雷暴",
        96 or 99 => "雷暴伴冰雹",
        _ => $"未知天气（代码 {code}）"
    };
}

file sealed record GeocodingResponse(
    [property: JsonPropertyName("results")] IReadOnlyList<GeocodingLocation>? Results);

file sealed record GeocodingLocation(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("admin1")] string? Admin1,
    [property: JsonPropertyName("latitude")] double Latitude,
    [property: JsonPropertyName("longitude")] double Longitude);

file sealed record WeatherForecastResponse(
    [property: JsonPropertyName("current")] CurrentWeather Current,
    [property: JsonPropertyName("daily")] DailyWeather Daily);

file sealed record CurrentWeather(
    [property: JsonPropertyName("temperature_2m")] double Temperature,
    [property: JsonPropertyName("apparent_temperature")] double ApparentTemperature,
    [property: JsonPropertyName("weather_code")] int WeatherCode,
    [property: JsonPropertyName("wind_speed_10m")] double WindSpeed);

file sealed record DailyWeather(
    [property: JsonPropertyName("time")] IReadOnlyList<string> Time,
    [property: JsonPropertyName("weather_code")] IReadOnlyList<int> WeatherCode,
    [property: JsonPropertyName("temperature_2m_max")] IReadOnlyList<double> TemperatureMax,
    [property: JsonPropertyName("temperature_2m_min")] IReadOnlyList<double> TemperatureMin,
    [property: JsonPropertyName("precipitation_probability_max")] IReadOnlyList<int> PrecipitationProbabilityMax,
    [property: JsonPropertyName("wind_speed_10m_max")] IReadOnlyList<double> WindSpeedMax);