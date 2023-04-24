using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace StartupOrchestration.NET.UnitTests.TestClasses;

internal sealed class TestServiceRegistrationOrchestrator : ServiceRegistrationOrchestrator
{
    protected override ILogger StartupLogger => NullLogger.Instance;
}