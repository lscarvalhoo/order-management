# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine3.22 AS build
RUN addgroup -S appgroup \
    && adduser -S appuser -G appgroup \
    && mkdir -p /src /app /home/appuser/.nuget/packages \
    && chown -R appuser:appgroup /src /app /home/appuser
ENV HOME=/home/appuser
ENV NUGET_PACKAGES=/home/appuser/.nuget/packages
WORKDIR /src
USER appuser

# Copy solution and project files
COPY --chown=appuser:appgroup ["OrderManagement.sln", "./"]
COPY --chown=appuser:appgroup ["src/API/OrderManagement.API/OrderManagement.API.csproj", "src/API/OrderManagement.API/"]
COPY --chown=appuser:appgroup ["src/Application/OrderManagement.Application/OrderManagement.Application.csproj", "src/Application/OrderManagement.Application/"]
COPY --chown=appuser:appgroup ["src/Domain/OrderManagement.Domain/OrderManagement.Domain.csproj", "src/Domain/OrderManagement.Domain/"]
COPY --chown=appuser:appgroup ["src/Infrastructure/OrderManagement.Infrastructure/OrderManagement.Infrastructure.csproj", "src/Infrastructure/OrderManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/API/OrderManagement.API/OrderManagement.API.csproj"

# Copy application source only
COPY --chown=appuser:appgroup ["src/", "src/"]

# Build the application
WORKDIR "/src/src/API/OrderManagement.API"
RUN dotnet build "OrderManagement.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "OrderManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine3.22 AS final
RUN addgroup -S appgroup \
    && adduser -S appuser -G appgroup \
    && mkdir -p /app /app/data /app/logs \
    && chown -R appuser:appgroup /app
WORKDIR /app

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --chown=appuser:appgroup --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "OrderManagement.API.dll"]
