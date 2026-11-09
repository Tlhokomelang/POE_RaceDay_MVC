# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Restore first so this layer is cached until the .csproj changes.
COPY POE_RaceDay_MVC.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish POE_RaceDay_MVC.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# The MVC app only ever talks to the Part 2 API. When the API runs on the
# host machine, Docker reaches it through host.docker.internal. Override with
# -e ApiSettings__BaseUrl=... to point somewhere else.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true \
    ApiSettings__BaseUrl=http://host.docker.internal:5146

EXPOSE 8080

COPY --from=build /app/publish .

# Run as the non-root user that ships with the .NET 8 images.
USER app

ENTRYPOINT ["dotnet", "POE_RaceDay_MVC.dll"]
