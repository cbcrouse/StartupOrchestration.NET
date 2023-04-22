using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StartupOrchestration.NET.UnitTests.TestClasses
{
    public static class TestExtensions
    {
        public static void ServiceCollectionExtension(this IServiceCollection services)
        {
            // Do nothing
        }
        public static void ConfigurationExtension(this IConfiguration config)
        {
            // Do nothing
        }
    }
}
