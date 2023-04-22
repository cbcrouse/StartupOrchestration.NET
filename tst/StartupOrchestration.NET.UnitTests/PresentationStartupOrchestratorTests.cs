using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace StartupOrchestration.NET.UnitTests;

public class PresentationStartupOrchestratorTests
{
    [Fact]
    public void ConfigureServices_RegistersServices()
    {
        // Arrange
        var orchestrator = new TestStartupOrchestrator();
        var serviceCollection = new ServiceCollection();
        orchestrator.ServiceRegistrationExpressions.Add((x,y) => x.AddScoped<ITestPresentationService, TestPresentationService>());

        // Act
        orchestrator.ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(serviceProvider.GetService<ITestCoreService>());
        Assert.NotNull(serviceProvider.GetService<ITestPresentationService>());
    }

    [Fact]
    public void ConfigureServices_SetsBasePath()
    {
        // Arrange
        var orchestrator = new TestStartupOrchestrator();
        var builder = new ConfigurationBuilder();
        orchestrator.CreateConfigurationBuilder(builder);

        // Act
        orchestrator.ConfigureServices(new ServiceCollection());
        var configuration = builder.Build();

        // Assert
        Assert.NotEmpty(builder.Sources);
        Assert.Equal(expected:"test", actual: configuration["BasePath"]);
    }

    [Fact]
    public void ConfigureServices_Throws_When_ExpressionIsNotValid()
    {
        // Arrange
        var orchestrator = new TestStartupOrchestrator();
        orchestrator.ServiceRegistrationExpressions.Add((x, y) => Expression.Empty());

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => orchestrator.ConfigureServices(new ServiceCollection()));
        Assert.Contains("Only extension methods declared on IServiceCollection are allowed as service registration expressions.", ex.Message);
    }
}

internal class TestCoreServiceRegistrationOrchestrator : ServiceRegistrationOrchestrator
{
    protected override ILogger StartupLogger => NullLogger.Instance;

    public TestCoreServiceRegistrationOrchestrator()
    {
        ServiceRegistrationExpressions.Add((x, y) => x.AddScoped<ITestCoreService, TestCoreService>());
    }
}

public interface ITestPresentationService { }

public class TestPresentationService : ITestPresentationService { }

internal class TestStartupOrchestrator : StartupOrchestrator<TestCoreServiceRegistrationOrchestrator>
{
    protected override void AddConfigurationProviders(IConfigurationBuilder builder)
    {
        builder.AddInMemoryCollection(new Dictionary<string, string>
        {
            { "BasePath", "test" }
        }!);
    }

    protected override IConfigurationBuilder DefaultConfigurationBuilder { get; set; } = new ConfigurationBuilder();

    public void CreateConfigurationBuilder(IConfigurationBuilder builder)
    {
        DefaultConfigurationBuilder = builder;
    }
}