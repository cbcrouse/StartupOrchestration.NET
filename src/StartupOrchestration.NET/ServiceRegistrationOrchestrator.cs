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
        /// This method is the main entry point for the orchestration of dependency registrations.
        /// It uses the provided <paramref name="serviceCollection"/> to register all the services and dependencies needed for the application.
        /// The registrations are based on a list of <see cref="Expression{Action}"/> objects in the
        /// <see cref="ServiceRegistrationExpressions"/> collection, which specify the service registrations to be performed.
        /// This method logs each registration as it happens using the expression body with the <see cref="StartupLogger"/> property.
        /// The <paramref name="configuration"/> parameter is used to supply configuration values to the application.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> instance to use for registering services.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> instance to use for supplying configuration values to the application.</param>
        public void Orchestrate(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            ServiceRegistrationExpressions.ForEach(x => x.ValidateServiceRegistration());

            foreach (Expression<Action<IServiceCollection, IConfiguration>> expression in ServiceRegistrationExpressions)
            {
                var expressionAsString = expression.Body.ToString();

                try
                {
                    StartupLogger.LogTrace("'{Expression}' was started...", expressionAsString);
                    expression.Compile().Invoke(serviceCollection, configuration);
                    StartupLogger.LogTrace("'{Expression}' completed successfully!", expressionAsString);
                }
                catch (Exception e)
                {
                    StartupLogger.LogTrace(e, "'{Expression}' failed with an unhandled exception.", expressionAsString);
                    throw;
                }
            }
        }
    }
}