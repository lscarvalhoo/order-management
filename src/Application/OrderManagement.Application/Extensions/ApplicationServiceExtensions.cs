using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Behaviors;
using OrderManagement.Application.Commands.CreateOrder;

namespace OrderManagement.Application.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
        });

        // Add FluentValidation
        services.AddValidatorsFromAssembly(typeof(CreateOrderCommand).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
