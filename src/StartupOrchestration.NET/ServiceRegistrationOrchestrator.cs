namespace StartupOrchestration.NET
{
    /// <summary>
    /// This abstract class provides an interface for orchestrating dependency registrations in a way that is agnostic
    /// to the presentation layer (e.g. Web API, Desktop App, Console App). The presentation layer can connect to this
    /// orchestrator through generic inheritance in order to provide presentation-specific dependencies.
    /// <para>
    /// The presentation orchestrator is responsible for initializing the <see cref="IServiceCollection"/> and
    /// <see cref="IConfiguration"/> instances that are required by this class.
    /// </para>
    /// </summary>
    public abstract class ServiceRegistrationOrchestrator
    {
        private IServiceCollection _serviceCollection = null!;
        private IConfiguration _configuration = null!;

        /// <summary>
        /// This property is a list of expressions that define service registrations to be added to the DI container during
        /// startup. Each expression is intended to be a call to the AddTransient, AddScoped, or AddSingleton method of the
        /// <see cref="IServiceCollection"/> or a custom extension method that extends the <see cref="IServiceCollection"/>.
        /// The expressions are executed when <see cref="Orchestrate"/> is called.
        /// </summary>
        public List<Expression<Action<IServiceCollection, IConfiguration>>> ServiceRegistrationExpressions { get; } = new();

        /// <summary>
        /// This property provides access to a static instance of the <see cref="ILogger"/> interface that is used to log 
        /// startup registrations that occur before the dependency injection (DI) container has been initialized.
        /// </summary>
        protected abstract ILogger StartupLogger { get; }

        /// <summary>
        /// This function is used to pass the <see cref="IServiceCollection"/> from the presentation layer to the
        /// application layer. This allows the application to take control of service registrations.
        /// </summary>
        /// <param name="collection">A collection of service descriptors that specifies the contracts and implementations
        /// for the services that will be registered by the application.</param>
        public void InitializeServiceCollection(IServiceCollection collection)
        {
            _serviceCollection = collection;
        }

        /// <summary>
        /// This function allows the presentation layer to send the <see cref="IConfiguration"/>
        /// down to the application layer. This allows the application to load configurations,
        /// independent of the presentation.
        /// </summary>
        /// <param name="configuration">Represents a set of key/value application configuration properties.</param>
        public void InitializeConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// This method is the main entry point for the orchestration of dependency registrations.
        /// It leverages the service collection object that was previously set up through the
        /// <see cref="InitializeServiceCollection"/> methods respectively, to register all the
        /// services and dependencies needed for the application. The registrations are based on
        /// the list of <see cref="Expression{Action}"/> objects in the
        /// <see cref="ServiceRegistrationExpressions"/> collection, which specify the service
        /// registrations to be performed. This method logs each registration as it happens using
        /// the expression body with the <see cref="StartupLogger"/> property.
        /// </summary>
        public void Orchestrate()
        {
            ServiceRegistrationExpressions.ForEach(x => x.ValidateServiceRegistration());

            foreach (Expression<Action<IServiceCollection, IConfiguration>> expression in ServiceRegistrationExpressions)
            {
                var expressionAsString = expression.Body.ToString();

                try
                {
                    StartupLogger.LogTrace("'{Expression}' was started...", expressionAsString);
                    expression.Compile().Invoke(_serviceCollection, _configuration);
                    StartupLogger.LogTrace("'{Expression}' completed successfully!", expressionAsString);
                }
                catch (Exception e)
                {
                    StartupLogger.LogTrace(e, "'{Expression}' failed with an unhandled exception.", expression.Body.ToString());
                    throw;
                }
            }
        }
    }
}