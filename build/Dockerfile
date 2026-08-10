# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine3.22 AS build
RUN addgroup -S appgroup \
    && adduser -S appuser -G appgroup \
    && mkdir -p /source /work /app /home/appuser/.nuget/packages \
    && chown -R appuser:appgroup /work /app /home/appuser \
    && chmod 755 /source
ENV HOME=/home/appuser
ENV NUGET_PACKAGES=/home/appuser/.nuget/packages
USER appuser
WORKDIR /source

# Copy solution and project files
COPY --chmod=444 ["OrderManagement.sln", "./"]
COPY --chmod=444 ["src/API/OrderManagement.API/OrderManagement.API.csproj", "src/API/OrderManagement.API/"]
COPY --chmod=444 ["src/Application/OrderManagement.Application/OrderManagement.Application.csproj", "src/Application/OrderManagement.Application/"]
COPY --chmod=444 ["src/Domain/OrderManagement.Domain/OrderManagement.Domain.csproj", "src/Domain/OrderManagement.Domain/"]
COPY --chmod=444 ["src/Infrastructure/OrderManagement.Infrastructure/OrderManagement.Infrastructure.csproj", "src/Infrastructure/OrderManagement.Infrastructure/"]
RUN cp -R /source/. /work/ \
    && chmod -R u+w /work

# Restore dependencies
WORKDIR /work
RUN dotnet restore "src/API/OrderManagement.API/OrderManagement.API.csproj"

# Copy application source only
WORKDIR /source
COPY --chmod=555 ["src/", "src/"]
RUN cp -R /source/src/. /work/src/ \
    && chmod -R u+w /work/src

# Build the application
WORKDIR "/work/src/API/OrderManagement.API"
RUN dotnet build "OrderManagement.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "OrderManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine3.22 AS final
RUN addgroup -S appgroup \
    && adduser -S appuser -G appgroup \
    && mkdir -p /app /app/data /app/logs \
    && chown -R appuser:appgroup /app/data /app/logs
WORKDIR /app

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --chmod=555 --from=publish /app/publish/ /app/

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 CMD ["wget", "--quiet", "--tries=1", "--spider", "http://localhost:8080/health"]

ENTRYPOINT ["dotnet", "OrderManagement.API.dll"]
