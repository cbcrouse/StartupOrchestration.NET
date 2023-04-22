using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace StartupOrchestration.NET.UnitTests;

public class StartupOrchestratorBaseTests
{
    [Fact]
    public void Orchestrate_Should_Invoke_Service_Registrations()
    {
        // Arrange
        var collection = new ServiceCollection();
        var orchestrator = new TestServiceRegistrationOrchestrator();
        orchestrator.InitializeServiceCollection(collection);
        orchestrator.ServiceRegistrationExpressions.Add((x, y) => x.AddScoped<ITestCoreService, TestCoreService>());

        // Act
        orchestrator.Orchestrate();
        var provider = collection.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<ITestCoreService>());
    }

    [Fact]
    public void RegisterServices_Throws_WhenExpressionIs_InvalidMethodCall()
    {
        // Arrange
        var orchestrator = new TestServiceRegistrationOrchestrator();
        orchestrator.ServiceRegistrationExpressions.Add((x, y) => Expression.Empty());

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => orchestrator.Orchestrate());
        Assert.Contains("Only extension methods declared on IServiceCollection are allowed as service registration expressions.", ex.Message);
    }
}

public interface ITestCoreService { }

public class TestCoreService : ITestCoreService { }

public class TestServiceRegistrationOrchestrator : ServiceRegistrationOrchestrator
{
    protected override ILogger StartupLogger => NullLogger.Instance;
}