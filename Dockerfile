FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["IpmiApi.Host/IpmiApi.Host.csproj", "IpmiApi.Host/"]
COPY ["IpmiApi.Controllers/IpmiApi.Controllers.csproj", "IpmiApi.Controllers/"]
COPY ["IpmiApi.Services/IpmiApi.Services.csproj", "IpmiApi.Services/"]

RUN dotnet restore "IpmiApi.Host/IpmiApi.Host.csproj"

COPY . .

WORKDIR "/src/IpmiApi.Host"
RUN dotnet build "IpmiApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "IpmiApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_HTTP_PORTS="9856"

ENTRYPOINT ["dotnet", "IpmiApi.Host.dll"]