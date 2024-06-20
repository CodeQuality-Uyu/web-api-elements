using CQ.ApiElements.Filters;
using CQ.Extensions.ServiceCollection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.ApiElements.AppConfig;
public static class ServiceCollectionExtensions
{
    public static MvcOptions AddExceptionGlobalHandler(this MvcOptions options)
    {
        options.Filters.Add<ExceptionFilter>();

        return options;
    }

    public static MvcOptions AddExceptionGlobalHandler<TExceptionFilter>(this MvcOptions options)
        where TExceptionFilter : ExceptionFilter
    {
        options.Filters.Add<TExceptionFilter>();

        return options;
    }

    public static IServiceCollection AddExceptionGlobalHandlerService(
        this IServiceCollection services,
        LifeTime storeLifeTime)
    {
        services
            .AddService<ExceptionStoreService>(storeLifeTime);

        return services;
    }

    public static IServiceCollection AddExceptionGlobalHandlerService<TExceptionStore>(
        this IServiceCollection services,
        LifeTime storeLifeTime)
        where TExceptionStore : ExceptionStoreService
    {
        services
            .AddService<ExceptionStoreService, TExceptionStore>(storeLifeTime);

        return services;
    }
}
