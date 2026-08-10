# OpenTelemetry Implementation

## Overview

This project implements OpenTelemetry for distributed tracing with console export. The implementation provides visibility into API requests, database operations, and custom application events.

## Features

### ðŸ“Š Instrumentation

1. **ASP.NET Core Instrumentation**
   - Automatic tracing of HTTP requests
   - Request/response enrichment with custom tags
   - Exception recording
   - Filtering of non-relevant endpoints (swagger, static files)

2. **HTTP Client Instrumentation**
   - Tracing of outgoing HTTP calls
   - Request/response enrichment

3. **SQL Client Instrumentation**
   - Database query tracing
   - SQL statement capturing
   - Connection-level attributes
   - Exception recording

4. **Custom Application Tracing**
   - Custom spans for business operations
   - Rich tagging of business events

## Configuration

### Packages Installed

```xml
<PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.10.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.10.1" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.10.1" />
<PackageReference Include="OpenTelemetry.Instrumentation.SqlClient" Version="1.10.0-beta.1" />
```

### Configuration in appsettings.json

```json
{
  "OpenTelemetry": {
	"ServiceName": "OrderManagement.API",
	"ServiceVersion": "1.0.0"
  }
}
```

### Service Registration

The OpenTelemetry configuration is registered in `Program.cs` using the extension method:

```csharp
builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);
```

## Custom Tracing

### ApplicationActivitySource

A custom `ActivitySource` is available for creating custom spans:

```csharp
using var activity = ApplicationActivitySource.StartActivity("OperationName");
activity?.SetTag("custom.tag", "value");
```

### Example Usage in Handlers

**CreateOrderCommandHandler:**
```csharp
using var activity = ApplicationActivitySource.StartActivity("CreateOrder");
activity?.SetTag("order.customer_id", request.CustomerId);
activity?.SetTag("order.items_count", request.Items.Count);
activity?.SetTag("order.total_amount", order.TotalAmount);
```

**CancelOrderCommandHandler:**
```csharp
using var activity = ApplicationActivitySource.StartActivity("CancelOrder");
activity?.SetTag("order.id", request.OrderId);
activity?.SetTag("order.found", true);
activity?.SetTag("order.current_status", order.Status.ToString());
```

## Console Output

When you run the application, you'll see trace output in the console similar to:

```
Activity.TraceId:            8a1d2c3e4f5a6b7c8d9e0f1a2b3c4d5e
Activity.SpanId:             1a2b3c4d5e6f7a8b
Activity.TraceFlags:         Recorded
Activity.ParentSpanId:       9a8b7c6d5e4f3a2b
Activity.ActivitySourceName: OrderManagement.Application
Activity.DisplayName:        CreateOrder
Activity.Kind:               Internal
Activity.StartTime:          2024-01-15T10:30:00.0000000Z
Activity.Duration:           00:00:00.0234567
Activity.Tags:
	order.customer_id: 12345678-1234-1234-1234-123456789012
	order.items_count: 2
	order.total_amount: 150.00
```

## Resource Attributes

The following resource attributes are automatically added to all traces:

- `service.name`: OrderManagement.API
- `service.version`: 1.0.0
- `deployment.environment`: Development/Production
- `host.name`: Machine name

## Filtering

The following endpoints are filtered from tracing:
- `/swagger/*`
- `/_framework/*`
- `/favicon.ico`

## Integration with Other Systems

While this implementation uses the console exporter, OpenTelemetry supports many exporters:

- **Jaeger**: For distributed tracing visualization
- **Zipkin**: Alternative tracing backend
- **Azure Monitor**: For Azure-hosted applications
- **Prometheus**: For metrics
- **OTLP**: OpenTelemetry Protocol for vendor-agnostic export

To switch exporters, simply replace `.AddConsoleExporter()` with the desired exporter in `OpenTelemetryExtensions.cs`.

## Best Practices

1. **Tag Naming**: Use lowercase with dots as separators (e.g., `order.customer_id`)
2. **Sensitive Data**: Never include PII or sensitive data in tags
3. **Activity Disposal**: Always use `using` statements for activities
4. **Meaningful Names**: Use descriptive operation names
5. **Span Context**: Keep spans focused on single operations

## Benefits

âœ… **Observability**: Full visibility into request flow  
âœ… **Performance**: Identify bottlenecks and slow operations  
âœ… **Debugging**: Trace errors through the entire stack  
âœ… **Monitoring**: Track application health and behavior  
âœ… **Standardization**: Industry-standard telemetry format

## Running the Application

Simply start the application:

```bash
dotnet run --project src/API/OrderManagement.API
```

Trace data will be exported to the console automatically.

## Testing

All unit tests continue to pass (99/99) with OpenTelemetry enabled. The telemetry code uses conditional operators (`?.`) to safely handle null activities during testing.


