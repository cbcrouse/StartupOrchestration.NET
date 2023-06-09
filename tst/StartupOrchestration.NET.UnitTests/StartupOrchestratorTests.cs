﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StartupOrchestration.NET.UnitTests.TestClasses;

namespace StartupOrchestration.NET.UnitTests;

public class StartupOrchestratorTests
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
    public void ConfigureServices_DefaultConfigurationBuilder_SetsBasePath()
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
    public void ConfigureServices_DefaultConfigurationBuilder_KeepsPresetConfiguration()
    {
        // Arrange
        var orchestrator = new TestStartupOrchestrator();
        var builder = new ConfigurationBuilder();
        var kvp = new KeyValuePair<string, string?>(Guid.NewGuid().ToString(), "Value");
        builder.AddInMemoryCollection(new List<KeyValuePair<string, string?>>{ kvp });
        orchestrator.CreateConfigurationBuilder(builder);

        // Act
        orchestrator.ConfigureServices(new ServiceCollection());
        var configuration = builder.Build();

        // Assert
        Assert.NotEmpty(builder.Sources);
        Assert.Equal(expected:kvp.Value, actual: configuration[kvp.Key]);
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