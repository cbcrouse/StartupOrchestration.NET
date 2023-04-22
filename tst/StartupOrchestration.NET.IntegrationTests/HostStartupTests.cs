using StartupOrchestration.NET.IntegrationTests.TestClasses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace StartupOrchestration.NET.IntegrationTests;

public class HostStartupTests
{
    private readonly IServiceProvider _serviceProvider;

    public HostStartupTests()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        var builder = new WebHostBuilder().UseStartup<TestStartupOrchestrator>();

        // Act & Assert
        var t = new TestServer(builder);
        _serviceProvider = t.Services;
    }

    [Fact]
    public void Core_ConfiguredOptionsClass_IsRegistered()
    {
        var options = _serviceProvider.GetService<IOptions<TestOptions>>();
        Assert.NotNull(options?.Value);
        Assert.Equal("1", options.Value.Property1);
        Assert.Equal("2", options.Value.Property2);
    }

    [Fact]
    public void Presentation_Service_IsRegistered()
    {
        var service = _serviceProvider.GetService<IService>();
        Assert.NotNull(service);
        Assert.IsType<Service>(service);
    }
}