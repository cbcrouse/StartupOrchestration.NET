using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StartupOrchestration.NET.UnitTests.TestClasses;

namespace StartupOrchestration.NET.UnitTests;

public class ServiceRegistrationOrchestratorTests
{
    [Fact]
    public void Orchestrate_Should_Invoke_Service_Registrations()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var orchestrator = new TestServiceRegistrationOrchestrator();
        orchestrator.ServiceRegistrationExpressions.Add((x, y) => x.AddScoped<ITestCoreService, TestCoreService>());

        // Act
        orchestrator.Orchestrate(serviceCollection, configuration);
        var provider = serviceCollection.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider.GetService<ITestCoreService>());
    }

    [Fact]
    public void RegisterServices_Throws_WhenExpressionIs_InvalidMethodCall()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var orchestrator = new TestServiceRegistrationOrchestrator();
        orchestrator.ServiceRegistrationExpressions.Add((x, y) => Expression.Empty());

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => orchestrator.Orchestrate(serviceCollection, configuration));
        Assert.Contains("Only extension methods declared on IServiceCollection are allowed as service registration expressions.", ex.Message);
    }

    [Fact]
    public void RegisterServices_Throws_WhenExpression_Fails()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var orchestrator = new TestServiceRegistrationOrchestrator();
        orchestrator.ServiceRegistrationExpressions.Add((x, y) => x.ThrowInvalidOperationException());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => orchestrator.Orchestrate(serviceCollection, configuration));
    }

    [Fact]
    public void Orchestrate_LogsSuccess_WithStartupLogger()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var orchestrator = new TestServiceRegistrationOrchestratorWithLogger();
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.AddScoped<ITestCoreService, TestCoreService>();
        orchestrator.ServiceRegistrationExpressions.Add(expression);
        var expectedStartedMessage = "'AddScoped<ITestCoreService, TestCoreService>(this IServiceCollection)' was started...";
        var expectedCompletedMessage = "'AddScoped<ITestCoreService, TestCoreService>(this IServiceCollection)' completed successfully!";

        // Act
        using var scope = new StringWriter();
        Console.SetOut(scope);
        orchestrator.Orchestrate(serviceCollection, configuration);

        // Assert
        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedStartedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedCompletedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Orchestrate_LogsFailure_WithStartupLogger()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var orchestrator = new TestServiceRegistrationOrchestratorWithLogger();
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.ThrowInvalidOperationException();
        orchestrator.ServiceRegistrationExpressions.Add(expression);
        var expectedFailureMessage = "'ThrowInvalidOperationException(this IServiceCollection)' failed with an unhandled exception.";

        // Act
        using var scope = new StringWriter();
        Console.SetOut(scope);
        Assert.Throws<InvalidOperationException>(() => orchestrator.Orchestrate(serviceCollection, configuration));

        // Assert
        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedFailureMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Orchestrate_LogsServiceRegistration_WithNoParameters()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var orchestrator = new TestServiceRegistrationOrchestratorWithLogger();
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.AddService();
        orchestrator.ServiceRegistrationExpressions.Add(expression);
        var expectedStartedMessage = "'AddService(this IServiceCollection)' was started...";
        var expectedCompletedMessage = "'AddService(this IServiceCollection)' completed successfully!";

        // Act
        orchestrator.Orchestrate(serviceCollection, configuration);

        // Assert
        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedStartedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedCompletedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Orchestrate_LogsServiceRegistration_WithGenericParameters()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var orchestrator = new TestServiceRegistrationOrchestratorWithLogger();
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.AddScoped<ITestCoreService, TestCoreService>();
        orchestrator.ServiceRegistrationExpressions.Add(expression);
        var expectedStartedMessage = "'AddScoped<ITestCoreService, TestCoreService>(this IServiceCollection)' was started...";
        var expectedCompletedMessage = "'AddScoped<ITestCoreService, TestCoreService>(this IServiceCollection)' completed successfully!";

        // Act
        orchestrator.Orchestrate(serviceCollection, configuration);

        // Assert
        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedStartedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedCompletedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Orchestrate_LogsServiceRegistration_WithNormalParameters()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var orchestrator = new TestServiceRegistrationOrchestratorWithLogger();
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.AddScoped(typeof(ITestCoreService), typeof(TestCoreService));
        orchestrator.ServiceRegistrationExpressions.Add(expression);
        var expectedStartedMessage = "'AddScoped(this IServiceCollection, Type<StartupOrchestration.NET.UnitTests.TestClasses.ITestCoreService>, Type<StartupOrchestration.NET.UnitTests.TestClasses.TestCoreService>)' was started...";
        var expectedCompletedMessage = "'AddScoped(this IServiceCollection, Type<StartupOrchestration.NET.UnitTests.TestClasses.ITestCoreService>, Type<StartupOrchestration.NET.UnitTests.TestClasses.TestCoreService>)' completed successfully!";

        // Act
        orchestrator.Orchestrate(serviceCollection, configuration);

        // Assert
        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedStartedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedCompletedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }

    [Fact]
    public void Orchestrate_LogsServiceRegistration_WithLambdaExpression()
    {
        // Arrange
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        var orchestrator = new TestServiceRegistrationOrchestratorWithLogger();

        // Lambda expression: AddScoped<ITestCoreService>(services => new TestCoreService())
        Expression<Action<IServiceCollection, IConfiguration>> expression = (x, y) => x.AddScoped<ITestCoreService>(services => new TestCoreService());
        orchestrator.ServiceRegistrationExpressions.Add(expression);

        // Expected messages for logging
        var expectedStartedMessage = "'AddScoped<ITestCoreService>(this IServiceCollection, (IServiceProvider) => new TestCoreService())' was started...";
        var expectedCompletedMessage = "'AddScoped<ITestCoreService>(this IServiceCollection, (IServiceProvider) => new TestCoreService())' completed successfully!";

        // Act
        orchestrator.Orchestrate(serviceCollection, configuration);

        // Assert
        // Verify that the logger logged the "started" message with the lambda expression details
        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedStartedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);

        // Verify that the logger logged the "completed" message with the lambda expression details
        orchestrator.GetLogger().Verify(logger => logger.Log(
            LogLevel.Trace,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains(expectedCompletedMessage)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once);
    }
}