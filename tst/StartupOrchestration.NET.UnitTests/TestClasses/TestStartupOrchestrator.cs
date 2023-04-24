using Microsoft.Extensions.Configuration;

namespace StartupOrchestration.NET.UnitTests.TestClasses;

internal sealed class TestStartupOrchestrator : StartupOrchestrator<TestCoreServiceRegistrationOrchestrator>
{
    protected override void AddConfigurationProviders(IConfigurationBuilder builder)
    {
        builder.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "BasePath", "test" }
        }!);
    }

    protected override IConfigurationBuilder DefaultConfigurationBuilder { get; set; } = new ConfigurationBuilder();

    internal void CreateConfigurationBuilder(IConfigurationBuilder builder)
    {
        DefaultConfigurationBuilder = builder;
    }
}