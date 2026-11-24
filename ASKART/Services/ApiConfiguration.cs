namespace Askart.Services;

public static class ApiConfiguration
{
    public static Uri BaseAddress { get; } = new(
        (Environment.GetEnvironmentVariable("ASKART_API_URL") ?? "http://localhost:5025/api/").TrimEnd('/') + "/");
}
