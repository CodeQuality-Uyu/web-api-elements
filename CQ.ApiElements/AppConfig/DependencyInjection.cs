using CQ.ApiElements.Filters;
using CQ.ServiceExtension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace CQ.ApiElements.AppConfig
{
    public static class DependencyInjection
    {
        public static MvcOptions AddExceptionFilter(this MvcOptions options)
        {
            options.Filters.Add<ExceptionFilter>();

            return options;
        }

        public static MvcOptions AddExceptionFilter<TExceptionFilter>(this MvcOptions options)
            where TExceptionFilter : ExceptionFilter
        {
            options.Filters.Add<TExceptionFilter>();

            return options;
        }

        public static IServiceCollection AddHandleException<TExceptionRegistry>(
            this IServiceCollection services,
            LifeTime registryLifeTime = LifeTime.Scoped,
            LifeTime storeLifeTime = LifeTime.Scoped)
            where TExceptionRegistry : ExceptionRegistryService
        {
            services
                .AddService<ExceptionStoreService>(storeLifeTime)
                .AddService<ExceptionRegistryService, TExceptionRegistry>(registryLifeTime);

            return services;
        }
    }
}
