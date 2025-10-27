using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.Extensions.ServiceCollection;
using CQ.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Principal;

namespace CQ.ApiElements.AppConfig;

public static class ApiElementsConfig
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
        LifeTime storeLifeTime = LifeTime.Scoped)
    {
        services
            .AddService<ExceptionStoreService>(storeLifeTime);

        return services;
    }

    public static IServiceCollection AddExceptionGlobalHandlerService<TExceptionStore>(
        this IServiceCollection services,
        LifeTime storeLifeTime = LifeTime.Scoped)
        where TExceptionStore : ExceptionStoreService
    {
        services
            .AddService<ExceptionStoreService, TExceptionStore>(storeLifeTime)
            .AddService<TExceptionStore>(storeLifeTime)
            ;

        return services;
    }

    public static IServiceCollection AddExceptionGlobalHandlerService<TIExceptionStore, TExceptionStore>(
        this IServiceCollection services,
        LifeTime storeLifeTime = LifeTime.Scoped)
        where TIExceptionStore : class
        where TExceptionStore : ExceptionStoreService, TIExceptionStore
    {
        services
            .AddService<ExceptionStoreService, TExceptionStore>(storeLifeTime)
            .AddService<TIExceptionStore, TExceptionStore>(storeLifeTime)
            .AddService<TExceptionStore>(storeLifeTime)
            ;

        return services;
    }

    public static IServiceCollection AddFakeAuthentication<TPrincipal>(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        string fakeAuthenticationKey = "Authentication:Fake",
        string fakeAuthenticationActiveKey = "Authentication:Fake:IsActive",
        LifeTime fakeAuthenticationLifeTime = LifeTime.Scoped)
        where TPrincipal : IPrincipal
    {
        var isActive = configuration.GetValue<bool>(fakeAuthenticationActiveKey);

        if (!isActive || environment.IsProduction())
        {
            return services;
        }

        var val = configuration.GetSection(fakeAuthenticationKey).Get<TPrincipal>();

        Guard.ThrowIsNull(val, fakeAuthenticationKey);

        services.AddService((IPrincipal)val!, fakeAuthenticationLifeTime);

        return services;
    }

    public static IServiceCollection AddTokenService<TService>(
        this IServiceCollection services,
        LifeTime tokenServiceLifeTime)
        where TService : class, ITokenService
    {
        services.AddService<ITokenService, TService>(tokenServiceLifeTime);

        return services;
    }
}
