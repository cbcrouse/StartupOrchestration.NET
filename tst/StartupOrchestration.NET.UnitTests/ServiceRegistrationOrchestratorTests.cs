﻿using Microsoft.Extensions.Configuration;
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
        var expectedStartedMessage = $"'{expression.Body}' was started...";
        var expectedCompletedMessage = $"'{expression.Body}' completed successfully!";

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
        var expectedFailureMessage = $"'{expression.Body}' failed with an unhandled exception.";

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
}