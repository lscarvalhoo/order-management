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
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(CreateOrderCommand).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
