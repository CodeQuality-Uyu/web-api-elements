using CQ.ApiElements.Filters.ExceptionFilter;
using CQ.AuthProvider.Abstractions;
using CQ.Extensions.ServiceCollection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            .AddService<ExceptionStoreService, TExceptionStore>(storeLifeTime)
            .AddService<TExceptionStore>(storeLifeTime)
            ;

        return services;
    }

    public static IServiceCollection AddFakeAuthentication<TPrincipal>(
        this IServiceCollection services,
        IConfiguration configuration,
        string fakeAuthenticationKey = "Authentication:Fake",
        string fakeAuthenticationActiveKey = "Authentication:Fake:IsActive",
        LifeTime fakeAuthenticationLifeTime = LifeTime.Scoped)
        where TPrincipal : IPrincipal
    {
        var isActive = configuration.GetValue<bool>(fakeAuthenticationActiveKey);

        if (!isActive)
        {
            return services;
        }

        TPrincipal val = configuration.GetSection(fakeAuthenticationKey).Get<TPrincipal>();
        services.AddService((IPrincipal)val, fakeAuthenticationLifeTime);

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

    public static IServiceCollection AddItemLoggedService<TService>(
        this IServiceCollection services,
        LifeTime itemLoggedServiceLifeTime)
        where TService : class, IItemLoggedService
    {
        services.AddService<IItemLoggedService, TService>(itemLoggedServiceLifeTime);

        return services;
    }
}
