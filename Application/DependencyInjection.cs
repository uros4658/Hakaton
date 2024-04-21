using Application.Interfaces;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(
            typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(assembly);

        // Specify ChatGptService as the implementation type for IChatGptService
        services.AddScoped<IChatGptService, ChatGptService>();

        return services;
    }
}
