# IPMI API

A REST API Docker container written in C# for controlling SuperMicro servers via IPMI interface.

## Features

- Power on/off server
- Force power off server
- Reset server
- Get server power status
- Test IPMI connection
- Health checks
- Swagger/OpenAPI documentation
- Docker containerized
- Environment variable configuration

## Architecture

The solution is structured with clean architecture principles:

- **IpmiApi.Host**: Web API host project with dependency injection and configuration
- **IpmiApi.Controllers**: API controllers with Swagger documentation
- **IpmiApi.Services**: IPMI service layer implementing HTTP-based IPMI communication

## Quick Start

### Using Docker Compose

1. Clone the repository
2. Copy `.env.example` to `.env` and configure your IPMI settings:
   ```bash
   cp .env.example .env
   ```

3. Edit `.env` with your SuperMicro server IPMI credentials:
   ```
   IPMI_HOST=192.168.1.100
   IPMI_USERNAME=ADMIN
   IPMI_PASSWORD=ADMIN
   IPMI_TIMEOUT=30
   IPMI_USE_HTTPS=true
   ```

4. Start the container:
   ```bash
   docker-compose up -d
   ```

5. Access the API documentation at: http://localhost:9856

### Using Docker

```bash
# Build the image
docker build -t ipmi-api .
[docker-compose.yml](docker-compose.yml)
# Run the container
docker run -d \
  -p 9856:9856 \
  -e IPMI_HOST=192.168.1.100 \
  -e IPMI_USERNAME=ADMIN \
  -e IPMI_PASSWORD=ADMIN \
  --name ipmi-api \
  ipmi-api
```

### Manual Build

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build[docker-compose.yml](docker-compose.yml)

# Run the API
dotnet run --project IpmiApi.Host
```

## API Endpoints

### Power Control
- `POST /api/ipmi/power-on` - Power on the server
- `POST /api/ipmi/power-off` - Power off the server gracefully
- `POST /api/ipmi/force-power-off` - Force power off the server
- `POST /api/ipmi/reset` - Reset the server

### Status & Health
- `GET /api/ipmi/status` - Get current server power status
- `GET /api/ipmi/test-connection` - Test IPMI connection
- `GET /health` - Health check endpoint

### Documentation
- `GET /` - API information and links
- `GET /swagger` - Swagger UI documentation

## Configuration

The API can be configured using environment variables:

| Variable | Description | Default |
|----------|-------------|---------|
| `IPMI_HOST` | IPMI interface IP address | `192.168.1.100` |
| `IPMI_USERNAME` | IPMI username | `ADMIN` |
| `IPMI_PASSWORD` | IPMI password | `ADMIN` |
| `IPMI_TIMEOUT` | Request timeout in seconds | `30` |
| `IPMI_USE_HTTPS` | Use HTTPS for IPMI communication | `true` |

## IPMI Protocol Support

This implementation uses HTTP-based communication with SuperMicro IPMI interfaces. It's compatible with:

- SuperMicro servers with web-based IPMI (ATEN)
- IPMI 1.5 and 2.0 specifications
- Both HTTP and HTTPS protocols

## License

This project is provided as-is development purposes.