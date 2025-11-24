namespace IpmiApi.Services.Models;

public class IpmiConfiguration
{
    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public bool UseHttps { get; set; } = true;
}