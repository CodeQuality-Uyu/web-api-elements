using CQ.ApiElements.Config.Extensions;
using CQ.ApiElements.Filters;
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
        public static IServiceCollection AddExceptionFilter<TExceptionRegistry>(this IServiceCollection services, LifeTime lifeTime) 
            where TExceptionRegistry : ExceptionRegistryService 
        {
            services.AddService<ExceptionRegistryService, TExceptionRegistry>(lifeTime);

            return services;
        }

        public static IServiceCollection AddExceptionFilter<TExceptionStore, TExceptionRegistry>(
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
