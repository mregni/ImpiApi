using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using IpmiApi.Services.Interfaces;
using IpmiApi.Services.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IpmiApi.Services.Services;

public class SuperMicroIpmiService : IIpmiService, IDisposable
{
    private readonly IpmiConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly ILogger<SuperMicroIpmiService> _logger;
    private bool _isLoggedIn;

    private const string MediaType = "application/x-www-form-urlencoded";
    private string BaseUrl => $"{(_config.UseHttps ? "https" : "http")}://{_config.Host}";
    private string IpmiUrl => $"{BaseUrl}/cgi/ipmi.cgi";
    private string LoginUrl => $"{BaseUrl}/cgi/login.cgi";
    private string LogoutUrl => $"{BaseUrl}/cgi/url_redirect.cgi?url_name=man_logout";

    public SuperMicroIpmiService(IOptions<IpmiConfiguration> config, HttpClient httpClient, ILogger<SuperMicroIpmiService> logger)
    {
        _config = config.Value;
        _httpClient = httpClient;
        _logger = logger;
        var cookieContainer = new CookieContainer();

        var handler = new HttpClientHandler()
        {
            CookieContainer = cookieContainer,
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        _httpClient = new HttpClient(handler);
        _httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

        SetupHttpClientHeaders();
    }

    private void SetupHttpClientHeaders()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");
        _httpClient.DefaultRequestHeaders.Add("Accept", "text/javascript, text/html, application/xml, text/xml, */*");
        _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
        _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        _httpClient.DefaultRequestHeaders.Add("DNT", "1");
        _httpClient.DefaultRequestHeaders.Add("X-Prototype-Version", "1.5.0");
        _httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
    }

    public async Task<bool> LoginAsync()
    {
        try
        {
            var encodedUsername = HttpUtility.UrlEncode(_config.Username);
            var encodedPassword = HttpUtility.UrlEncode(_config.Password);

            var loginData = $"name={encodedUsername}&pwd={encodedPassword}";
            var content = new StringContent(loginData, Encoding.UTF8, MediaType);

            _logger.LogInformation("Attempting to login to IPMI at {Host}", _config.Host);

            var response = await _httpClient.PostAsync(LoginUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _isLoggedIn = responseContent.Contains("url_redirect.cgi?url_name=mainmenu");

                if (_isLoggedIn)
                {
                    _logger.LogInformation("Successfully logged in to IPMI");
                }
                else
                {
                    _logger.LogWarning("Login failed - invalid credentials or server response");
                }

                return _isLoggedIn;
            }

            _logger.LogError("Login failed with status code: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during IPMI login");
            return false;
        }
    }

    public void Logout()
    {
        try
        {
            if (!_isLoggedIn)
            {
                return;
            }
            
            _httpClient.GetAsync(LogoutUrl);
            _isLoggedIn = false;

            _logger.LogInformation("Logged out from IPMI");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during IPMI logout");
        }
    }

    public async Task<PowerCommandResult> PowerOnAsync()
    {
        return await ExecutePowerCommandAsync(PowerCommand.PowerOn);
    }

    public async Task<PowerCommandResult> PowerOffAsync()
    {
        return await ExecutePowerCommandAsync(PowerCommand.PowerOff);
    }

    public async Task<PowerCommandResult> ResetAsync()
    {
        return await ExecutePowerCommandAsync(PowerCommand.Reset);
    }

    public async Task<PowerCommandResult> ForcePowerOffAsync()
    {
        return await ExecutePowerCommandAsync(PowerCommand.ForcePowerOff);
    }

    private async Task<PowerCommandResult> ExecutePowerCommandAsync(PowerCommand command)
    {
        var result = new PowerCommandResult
        {
            ExecutedAt = DateTime.UtcNow
        };

        try
        {
            if (!_isLoggedIn && !await LoginAsync())
            {
                result.Success = false;
                result.Message = "Failed to login to IPMI interface";
                return result;
            }

            var commandData = $"op=POWER_INFO.XML&r=(1%2C{(int)command})&_=";
            var content = new StringContent(commandData, Encoding.UTF8, MediaType);

            _logger.LogInformation("Executing power command: {Command}", command);

            var response = await _httpClient.PostAsync(IpmiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                result.Success = true;
                result.Message = $"Power command {command} executed successfully";

                _logger.LogInformation("Power command {Command} executed successfully", command);
            }
            else
            {
                result.Success = false;
                result.Message = $"Power command failed with status code: {response.StatusCode}";

                _logger.LogError("Power command {Command} failed with status code: {StatusCode}", command, response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = $"Error executing power command: {ex.Message}";

            _logger.LogError(ex, "Error executing power command {Command}", command);
        }

        return result;
    }

    public async Task<ServerStatus> GetServerStatusAsync()
    {
        var status = new ServerStatus
        {
            LastChecked = DateTime.UtcNow
        };

        try
        {
            if (!_isLoggedIn && !await LoginAsync())
            {
                status.PowerState = "Unknown - Login Failed";
                return status;
            }

            var statusData = "op=POWER_INFO.XML&r=(0%2C0)&_=";
            var content = new StringContent(statusData, Encoding.UTF8, MediaType);

            var response = await _httpClient.PostAsync(IpmiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                try
                {
                    var xml = XDocument.Parse(responseContent);
                    var powerElement = xml.Root?.Element("POWER_INFO")?.Element("POWER");
                    var powerStatus = powerElement?.Attribute("STATUS")?.Value;

                    if (powerStatus?.ToUpper() == "ON")
                    {
                        status.IsOn = true;
                        status.PowerState = "On";
                    }
                    else if (powerStatus?.ToUpper() == "OFF")
                    {
                        status.IsOn = false;
                        status.PowerState = "Off";
                    }
                    else
                    {
                        status.PowerState = $"Unknown - Status: {powerStatus}";
                    }

                    _logger.LogInformation("Server status retrieved: {PowerState}", status.PowerState);
                }
                catch (Exception xmlEx)
                {
                    status.PowerState = "Error - Invalid XML response";
                    _logger.LogError(xmlEx, "Failed to parse XML response: {Response}", responseContent);
                }
            }
            else
            {
                status.PowerState = $"Error - Status Code: {response.StatusCode}";
                _logger.LogError("Failed to get server status. Status code: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            status.PowerState = $"Error - {ex.Message}";
            _logger.LogError(ex, "Error getting server status");
        }

        return status;
    }

    public IpmiServerInfo GetServerInfo()
    {
        return new IpmiServerInfo
        {
            Host = _config.Host,
            Username = _config.Username,
            UseHttps = _config.UseHttps,
            TimeoutSeconds = _config.TimeoutSeconds
        };
    }

    public void Dispose()
    {
        Logout();
        GC.SuppressFinalize(this);
        _httpClient?.Dispose();
    }
}