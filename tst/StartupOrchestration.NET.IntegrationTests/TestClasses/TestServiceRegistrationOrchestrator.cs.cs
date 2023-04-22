using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace StartupOrchestration.NET.IntegrationTests.TestClasses
{
    public class TestServiceRegistrationOrchestrator : ServiceRegistrationOrchestrator
    {
        /// <inheritdoc />
        protected override ILogger StartupLogger => NullLogger.Instance;

        public TestServiceRegistrationOrchestrator()
        {
            ServiceRegistrationExpressions.Add((services, config) => services.RegisterConfiguredOptions<TestOptions>(config));
        }
    }
}
