using IpmiApi.Services.Models;

namespace IpmiApi.Services.Interfaces;

public interface IIpmiService
{
    Task<PowerCommandResult> PowerOnAsync();
    Task<PowerCommandResult> PowerOffAsync();
    Task<PowerCommandResult> ResetAsync();
    Task<PowerCommandResult> ForcePowerOffAsync();
    Task<ServerStatus> GetServerStatusAsync();
    Task<bool> LoginAsync();
    void Logout();
    IpmiServerInfo GetServerInfo();
}