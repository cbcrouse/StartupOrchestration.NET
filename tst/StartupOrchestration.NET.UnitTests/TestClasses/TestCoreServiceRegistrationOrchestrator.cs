using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace StartupOrchestration.NET.UnitTests.TestClasses;

internal class TestCoreServiceRegistrationOrchestrator : ServiceRegistrationOrchestrator
{
    protected override ILogger StartupLogger => NullLogger.Instance;

    public TestCoreServiceRegistrationOrchestrator()
    {
        ServiceRegistrationExpressions.Add((x, y) => x.AddScoped<ITestCoreService, TestCoreService>());
    }
}