using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IpmiApi.Services.Interfaces;
using IpmiApi.Services.Models;

namespace IpmiApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IpmiController : ControllerBase
{
    private readonly IIpmiService _ipmiService;
    private readonly ILogger<IpmiController> _logger;

    public IpmiController(IIpmiService ipmiService, ILogger<IpmiController> logger)
    {
        _ipmiService = ipmiService;
        _logger = logger;
    }

    /// <summary>
    /// Powers on the server
    /// </summary>
    /// <returns>Result of the power on command</returns>
    [HttpPost("power-on")]
    public async Task<ActionResult<PowerCommandResult>> PowerOn()
    {
        try
        {
            _logger.LogInformation("Power on command requested");
            var result = await _ipmiService.PowerOnAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing power on command");
            return StatusCode(500, new PowerCommandResult
            {
                Success = false,
                Message = "Internal server error occurred",
                ExecutedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Powers off the server gracefully
    /// </summary>
    /// <returns>Result of the power off command</returns>
    [HttpPost("power-off")]
    public async Task<ActionResult<PowerCommandResult>> PowerOff()
    {
        try
        {
            _logger.LogInformation("Power off command requested");
            var result = await _ipmiService.PowerOffAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing power off command");
            return StatusCode(500, new PowerCommandResult
            {
                Success = false,
                Message = "Internal server error occurred",
                ExecutedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Forces power off the server immediately
    /// </summary>
    /// <returns>Result of the force power off command</returns>
    [HttpPost("force-power-off")]
    public async Task<ActionResult<PowerCommandResult>> ForcePowerOff()
    {
        try
        {
            _logger.LogInformation("Force power off command requested");
            var result = await _ipmiService.ForcePowerOffAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing force power off command");
            return StatusCode(500, new PowerCommandResult
            {
                Success = false,
                Message = "Internal server error occurred",
                ExecutedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Resets the server
    /// </summary>
    /// <returns>Result of the reset command</returns>
    [HttpPost("reset")]
    public async Task<ActionResult<PowerCommandResult>> Reset()
    {
        try
        {
            _logger.LogInformation("Reset command requested");
            var result = await _ipmiService.ResetAsync();

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing reset command");
            return StatusCode(500, new PowerCommandResult
            {
                Success = false,
                Message = "Internal server error occurred",
                ExecutedAt = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Gets the current server power status
    /// </summary>
    /// <returns>Current server status information</returns>
    [HttpGet("status")]
    public async Task<ActionResult<ServerStatus>> GetStatus()
    {
        try
        {
            _logger.LogInformation("Server status requested");
            var status = await _ipmiService.GetServerStatusAsync();

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting server status");
            return StatusCode(500, new ServerStatus
            {
                PowerState = "Error retrieving status",
                LastChecked = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Tests IPMI connection by attempting to login
    /// </summary>
    /// <returns>Result of the connection test</returns>
    [HttpGet("test-connection")]
    public async Task<ActionResult<object>> TestConnection()
    {
        try
        {
            _logger.LogInformation("Connection test requested");
            var success = await _ipmiService.LoginAsync();

            if (success)
            {
                _ipmiService.Logout();
                return Ok(new {
                    Success = true,
                    Message = "Successfully connected to IPMI interface",
                    TestedAt = DateTime.UtcNow
                });
            }

            return BadRequest(new {
                Success = false,
                Message = "Failed to connect to IPMI interface",
                TestedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing IPMI connection");
            return StatusCode(500, new {
                Success = false,
                Message = "Internal server error occurred during connection test",
                TestedAt = DateTime.UtcNow
            });
        }
    }
}
