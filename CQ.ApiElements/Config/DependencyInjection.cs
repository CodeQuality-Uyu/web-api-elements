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

namespace CQ.ApiElements.Config
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

        public static IServiceCollection AddHandleException<TExceptionRegistry>(this IServiceCollection services, LifeTime registryLifeTime, LifeTime storeLifeTime) 
            where TExceptionRegistry : ExceptionRegistryService 
        {
            services.AddService<ExceptionStoreService>(storeLifeTime);
            services.AddService<ExceptionRegistryService, TExceptionRegistry>(registryLifeTime);

            return services;
        }

        public static IServiceCollection AddHandleException<TExceptionStore, TExceptionRegistry>(
            this IServiceCollection services, 
            LifeTime storeLifeTime,
            LifeTime registryLifeTime)
            where TExceptionStore : ExceptionStoreService
            where TExceptionRegistry : ExceptionRegistryService
        {
            services.AddService<ExceptionStoreService, TExceptionStore>(storeLifeTime);
            services.AddService<ExceptionRegistryService, TExceptionRegistry>(registryLifeTime);

            return services;
        }
    }
}
