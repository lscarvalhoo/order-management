# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["OrderManagement.sln", "./"]
COPY ["src/API/OrderManagement.API/OrderManagement.API.csproj", "src/API/OrderManagement.API/"]
COPY ["src/Application/OrderManagement.Application/OrderManagement.Application.csproj", "src/Application/OrderManagement.Application/"]
COPY ["src/Domain/OrderManagement.Domain/OrderManagement.Domain.csproj", "src/Domain/OrderManagement.Domain/"]
COPY ["src/Infrastructure/OrderManagement.Infrastructure/OrderManagement.Infrastructure.csproj", "src/Infrastructure/OrderManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/API/OrderManagement.API/OrderManagement.API.csproj"

# Copy remaining source code
COPY . .

# Build the application
WORKDIR "/src/src/API/OrderManagement.API"
RUN dotnet build "OrderManagement.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "OrderManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Create directory for logs and database
RUN mkdir -p /app/logs /app/data

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "OrderManagement.API.dll"]
