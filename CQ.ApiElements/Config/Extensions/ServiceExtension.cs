using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.ApiElements.Config.Extensions
{
    internal static class ServiceExtension
    {
        public static IServiceCollection AddService<TService, TImplementation>(this IServiceCollection services, LifeTime lifeTime)
            where TService : class
            where TImplementation : class, TService
        {
            if(lifeTime == LifeTime.Scoped)
            {
                services.AddScoped<TService, TImplementation>();
            }

            if (lifeTime == LifeTime.Transient)
            {
                services.AddTransient<TService, TImplementation>();
            }

            if (lifeTime == LifeTime.Singleton)
            {
                services.AddSingleton<TService, TImplementation>();
            }

            return services;
        }
    }
}
