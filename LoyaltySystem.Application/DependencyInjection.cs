using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace LoyaltySystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // 1. Kích hoạt AutoMapper (Viết đúng theo Action nó yêu cầu)
        services.AddAutoMapper(config =>
        {
            // Bảo AutoMapper tự động tìm tất cả các file Mapping trong Project này
            config.AddMaps(Assembly.GetExecutingAssembly());
        });

        // 2. Kích hoạt FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // 3. Kích hoạt MediatR
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            // cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            // cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        return services;
    }
}