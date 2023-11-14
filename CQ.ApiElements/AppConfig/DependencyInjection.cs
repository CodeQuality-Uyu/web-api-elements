using CQ.ApiElements.Filters;
using CQ.ServiceExtension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static IServiceCollection AddHandleException<TExceptionStore, TExceptionRegistry>(
            this IServiceCollection services,
            LifeTime storeLifeTime = LifeTime.Scoped,
            LifeTime registryLifeTime = LifeTime.Scoped)
            where TExceptionStore : ExceptionStoreService
            where TExceptionRegistry : ExceptionRegistryService
        {
            services
                .AddService<ExceptionStoreService, TExceptionStore>(storeLifeTime)
                .AddService<ExceptionRegistryService, TExceptionRegistry>(registryLifeTime);

            return services;
        }
    }
}
