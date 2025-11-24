namespace IpmiApi.Services.Models;

public enum PowerCommand
{
    PowerOn = 1,
    PowerOff = 0,
    Reset = 3,
    ForcePowerOff = 5
}

public class PowerCommandResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
}

public class ServerStatus
{
    public bool IsOn { get; set; }
    public string PowerState { get; set; } = string.Empty;
    public DateTime LastChecked { get; set; }
}