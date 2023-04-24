using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StartupOrchestration.NET.UnitTests.TestClasses;

namespace StartupOrchestration.NET.UnitTests;

public class ServiceRegistrationTests
{
    [Fact]
    public void ValidateServiceRegistration_Throws_WhenExpressionIs_NotMethodCall()
    {
        // Arrange
        Expression<Action<IServiceCollection, IConfiguration>> registrationExpression = null!;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => registrationExpression.ValidateServiceRegistration());
        Assert.Contains("Registration expression must be a call to a method on IServiceCollection.", ex.Message);
    }

    [Fact]
    public void ValidateServiceRegistration_Throws_WhenExpressionIs_InvalidMethodCall()
    {
        // Arrange
        Expression<Action<IServiceCollection, IConfiguration>> registrationExpression = (x,y) => y.ConfigurationExtension();

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => registrationExpression.ValidateServiceRegistration());
        Assert.Contains("Only extension methods declared on IServiceCollection are allowed as service registration expressions.", ex.Message);
    }

    [Fact]
    public void ValidateServiceRegistration_Passes_WhenExpressionIs_ValidMethodCall()
    {
        // Arrange
        Expression<Action<IServiceCollection, IConfiguration>> nonGenericExtension = (x,y) => x.AddTransient(typeof(IService), typeof(Service));
        Expression<Action<IServiceCollection, IConfiguration>> genericExtension = (x,y) => x.AddTransient<IService, Service>();
        Expression<Action<IServiceCollection, IConfiguration>> customExtension = (x,y) => x.ServiceCollectionExtension();

        // Act & Assert
        var exception1 = Record.Exception(() => nonGenericExtension.ValidateServiceRegistration());
        Assert.Null(exception1);
        var exception2 = Record.Exception(() => genericExtension.ValidateServiceRegistration());
        Assert.Null(exception2);
        var exception3 = Record.Exception(() => customExtension.ValidateServiceRegistration());
        Assert.Null(exception3);
    }
}