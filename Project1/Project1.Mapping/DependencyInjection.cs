#nullable enable
using Microsoft.Extensions.DependencyInjection;
using Project1.Mapping.Common;

namespace Project1.Mapping
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddCrmMappingServices(this IServiceCollection services)
        {
            services.AddSingleton<IObjectMapper, ObjectMapper>();
            return services;
        }
    }
}
