# Multi-stage Dockerfile for Tomer Group ASP.NET Core API
# Target: Render Free Web Service / Container Deployment

# Stage 1: Runtime Base
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Stage 2: SDK Build & Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project definition files for layer caching
COPY ["src/TomerGroup.Core/TomerGroup.Core.csproj", "src/TomerGroup.Core/"]
COPY ["src/TomerGroup.Infrastructure/TomerGroup.Infrastructure.csproj", "src/TomerGroup.Infrastructure/"]
COPY ["src/TomerGroup.Api/TomerGroup.Api.csproj", "src/TomerGroup.Api/"]

# Restore dependencies
RUN dotnet restore "src/TomerGroup.Api/TomerGroup.Api.csproj"

# Copy remaining source code
COPY src/ src/

# Build project
WORKDIR "/src/src/TomerGroup.Api"
RUN dotnet build "TomerGroup.Api.csproj" -c Release -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "TomerGroup.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Production Runtime Image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Render dynamically sets $PORT. Program.cs binds to http://0.0.0.0:$PORT
ENTRYPOINT ["dotnet", "TomerGroup.Api.dll"]

