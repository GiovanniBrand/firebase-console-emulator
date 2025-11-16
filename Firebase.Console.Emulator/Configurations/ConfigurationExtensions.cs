using Firebase.Emulator.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Firebase.Emulator.Configurations
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddAppConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmulatorOptions>(
                configuration.GetSection("TestConfig:Emulator")
            );
            services.Configure<AuthServiceOptions>(
                configuration.GetSection("TestConfig")
            );
            services.Configure<PathFirebaseConfig>(
                configuration.GetSection("TestConfig")
            );
            return services;
        }
    }
}
