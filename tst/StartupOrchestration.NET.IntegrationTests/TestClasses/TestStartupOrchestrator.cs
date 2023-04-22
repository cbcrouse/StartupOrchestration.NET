using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StartupOrchestration.NET.IntegrationTests.TestClasses
{
    public class TestStartupOrchestrator : StartupOrchestrator<TestServiceRegistrationOrchestrator>
    {
        public TestStartupOrchestrator()
        {
            ServiceRegistrationExpressions.Add((services, config) => services.AddTransient<IService, Service>());
        }

        protected override void AddConfigurationProviders(IConfigurationBuilder builder)
        {
            builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        }

        public void Configure()
        {
            // This function is here to support test host startup.
        }
    }
}

public interface IService { }
public class Service : IService { }