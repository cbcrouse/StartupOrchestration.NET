using Microsoft.Extensions.Logging;

namespace StartupOrchestration.NET.UnitTests.TestClasses;

internal sealed class TestServiceRegistrationOrchestratorWithLogger : ServiceRegistrationOrchestrator
{
    private readonly Mock<ILogger> _mockLogger = new();

    protected override ILogger StartupLogger => _mockLogger.Object;

    internal Mock<ILogger> GetLogger()
    {
        return _mockLogger;
    }
}