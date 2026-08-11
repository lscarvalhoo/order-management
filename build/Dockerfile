# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN groupadd --system appgroup \
    && useradd --system --create-home --gid appgroup appuser \
    && mkdir -p /source /work /app /home/appuser/.nuget/packages \
    && chown -R appuser:appgroup /work /app /home/appuser \
    && chmod 755 /source
ENV HOME=/home/appuser
ENV NUGET_PACKAGES=/home/appuser/.nuget/packages
USER appuser
WORKDIR /work

# Copy solution and application source
COPY --chown=appuser:appgroup --chmod=644 OrderManagement.sln ./
COPY --chown=appuser:appgroup src/ src/

# Restore dependencies
WORKDIR /work
RUN dotnet restore "src/API/OrderManagement.API/OrderManagement.API.csproj"

# Build the application
WORKDIR "/work/src/API/OrderManagement.API"
RUN dotnet build "OrderManagement.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "OrderManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM build AS final
RUN mkdir -p /app/data /app/logs \
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
