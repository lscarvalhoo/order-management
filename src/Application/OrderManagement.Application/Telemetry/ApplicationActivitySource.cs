using System.Diagnostics;

namespace OrderManagement.Application.Telemetry;

public static class ApplicationActivitySource
{
    public const string SourceName = "OrderManagement.Application";

    public static readonly ActivitySource Instance = new(SourceName, "1.0.0");

    public static Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return Instance.StartActivity(name, kind);
    }
}
