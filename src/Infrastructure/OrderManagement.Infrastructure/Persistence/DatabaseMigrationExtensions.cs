using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace OrderManagement.Infrastructure.Persistence;

using System.Diagnostics.CodeAnalysis;

namespace OrderManagement.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
public static class DatabaseMigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }
}
