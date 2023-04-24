namespace StartupOrchestration.NET
{
    /// <summary>
    /// This class provides a way for the presentation layer to grant control of service registrations to the application layer.
    /// <para>
    /// It allows for the separation of concerns between the presentation layer and the application layer by providing a way
    /// for the application layer to manage service registrations without any presentation-specific dependencies.
    /// </para>
    /// <para>
    /// The class itself is a generic implementation that takes in a <see cref="ServiceRegistrationOrchestrator"/> type parameter. This base class defines
    /// the methods used by the <see cref="StartupOrchestrator{TOrchestrator}"/> to orchestrate the service registrations.
    /// </para>
    /// <para>
    /// The <see cref="StartupOrchestrator{TOrchestrator}"/> class itself is responsible for handling service registration, initialization of the
    /// application configuration and service collection objects, and the orchestration of the service registrations.
    /// </para>
    /// <para>
    /// This class is intended to be inherited from by a startup class in the presentation layer (see <see cref="ConfigureServices"/>).
    /// </para>
    /// </summary>
    /// <typeparam name="TOrchestrator">The type of the <see cref="ServiceRegistrationOrchestrator"/> to be used.</typeparam>
    public abstract class StartupOrchestrator<TOrchestrator> where TOrchestrator : ServiceRegistrationOrchestrator, new()
    {
        /// <summary>
        /// This property is a list of expressions that define service registrations to be added to the DI container during
        /// startup. Each expression is intended to be a call to the AddTransient, AddScoped, or AddSingleton method of the
        /// <see cref="IServiceCollection"/>.
        /// By default, the list is initialized as an empty <see cref="List{T}"/>
        /// where T is <see cref="Expression{Action}"/>. The expressions are executed when <see cref="ConfigureServices"/> is called.
        /// </summary>
        public List<Expression<Action<IServiceCollection, IConfiguration>>> ServiceRegistrationExpressions { get; } = new();

        /// <summary>
        /// This method is responsible for configuring the services for the application. It initializes the
        /// configuration and service collection objects through the respective methods of the generic
        /// StartupOrchestrator instance, then adds the service registrations and executes the
        /// orchestration.
        /// <para>
        /// This method is called by the .NET runtime during application startup if a startup class inheriting
        /// from  <see cref="StartupOrchestrator{TOrchestrator}"/> is configured as the startup class.
        /// It sets up the application's configuration and service collection, initializes the startup orchestrator,
        /// and runs the service registrations orchestrated by the startup orchestrator.
        /// </para>
        /// </summary>
        /// <param name="serviceCollection">Specifies the contract for a collection of service descriptors.</param>
        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            IConfigurationRoot configuration = SetupConfiguration();
            var orchestrator = new TOrchestrator();
            orchestrator.InitializeConfiguration(configuration);
            orchestrator.InitializeServiceCollection(serviceCollection);
            orchestrator.ServiceRegistrationExpressions.AddRange(ServiceRegistrationExpressions);
            orchestrator.Orchestrate();
        }

        /// <summary>
        /// Gets or sets the <see cref="IConfigurationBuilder"/> instance used to configure the application.
        /// This property is optional and can be overridden to provide a custom configuration builder.
        /// If not set, a new instance of <see cref="ConfigurationBuilder"/> is used.
        /// </summary>
        protected virtual IConfigurationBuilder DefaultConfigurationBuilder { get; set; } = new ConfigurationBuilder();

        /// <summary>
        /// This method sets up the application's configuration by creating a new <see cref="IConfigurationBuilder"/>
        /// object and applying configuration sources to it. It then returns the resulting <see cref="IConfigurationRoot"/>
        /// object.
        /// <para>
        /// The method first sets the base path of the builder to the application's base directory, adds the
        /// configured configuration sources, and applies any extensions to the builder. Finally, it builds
        /// and returns the configuration root.
        /// </para>
        /// </summary>
        /// <returns>The resulting <see cref="IConfigurationRoot"/> object.</returns>
        protected virtual IConfigurationRoot SetupConfiguration()
        {
            IConfigurationBuilder builder = DefaultConfigurationBuilder;
            SetBasePath(builder);
            AddConfigurationProviders(builder);
            return builder.Build();
        }

        /// <summary>
        /// Sets the base path for configuration sources to be loaded from. The method sets the base path of the application's
        /// configuration directory to the provided <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The configuration builder to set the base path for.</param>
        protected virtual void SetBasePath(IConfigurationBuilder builder)
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string path = Directory.GetParent(assemblyLocation)!.FullName;
            builder.SetBasePath(path);
        }

        /// <summary>
        /// Adds configuration providers to the given <see cref="IConfigurationBuilder"/> instance, allowing configuration
        /// to be loaded from various sources (e.g. appsettings.json, environment variables, command line arguments, etc.).
        /// This method is abstract and must be implemented by derived classes to provide the specific configuration sources
        /// needed for the application.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> instance to which configuration providers are added.</param>
        protected abstract void AddConfigurationProviders(IConfigurationBuilder builder);
    }
}
