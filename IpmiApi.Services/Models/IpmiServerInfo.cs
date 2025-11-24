namespace IpmiApi.Services.Models;

public class IpmiServerInfo
{
    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public bool UseHttps { get; set; }
    public int TimeoutSeconds { get; set; }
}