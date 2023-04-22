# StartupOrchestration.NET

StartupOrchestration.NET is a .NET library that provides an interface for orchestrating dependency registrations in a way that is agnostic to the presentation layer. It allows for the separation of concerns between the presentation layer and the application layer by providing a way for the application layer to manage service registrations without any presentation-specific dependencies.

## Features

- Enables application layer to manage service registrations independently of the presentation layer.
- Enables separation of concerns between the presentation layer and the application layer.
- Provides a generic implementation that can be easily customized for different types of applications.
- Provides a flexible interface for orchestrating service registrations that is agnostic to the presentation layer.
- Designed to be inherited from by a startup class in the presentation layer.

## Installation

You can install the package via NuGet. Search for `StartupOrchestration.NET` or run the following command:

```sh
dotnet add package StartupOrchestration.NET
```

## Usage

To use `StartupOrchestration.NET`, you need to implement the `ServiceRegistrationOrchestrator` class in your application layer, and inherit from `StartupOrchestrator<TOrchestrator>` in your presentation layer.

Here's an example of how you can use `StartupOrchestration.NET` in your application:

```csharp
// Application layer
public class AppStartupOrchestrator : ServiceRegistrationOrchestrator
{
    /// <inheritdoc />
    protected override ILogger StartupLogger => NullLogger.Instance;

    public AppStartupOrchestrator()
    {
        // Register shared services
        ServiceRegistrationExpressions.Add((services, config) => services.RegisterConfiguredOptions<RepositoryOptions>(config));
        ServiceRegistrationExpressions.Add((services, config) => services.AddTransient<IMyRepository, MyRepository>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddTransient<IEmailService, EmailService>());
    }
}

// Presentation layer
public class WebApiStartup : StartupOrchestrator<AppStartupOrchestrator>
{
    public WebApiStartup() : base(configuration)
    {
        ServiceRegistrationExpressions.Add((services, config) => services.RegisterConfiguredOptions<SwaggerOptions>(config));
        ServiceRegistrationExpressions.Add((services, config) => services.AddTransient<IService, Service>());
        ServiceRegistrationExpressions.Add((services, config) => services.AddAuthorization());
        ServiceRegistrationExpressions.Add((services, config) => services.AddSwagger());
    }

    protected override void AddConfigurationProviders(IConfigurationBuilder builder)
    {
        builder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    }
}

public static class ServiceCollectionExtensions
{
    // Helper method for registering options classes using the IOptions pattern
    public static void RegisterConfiguredOptions<T>(this IServiceCollection servicesCollection, IConfiguration configuration) where T : class, new()
    {
        string sectionKey = typeof(T).Name;
        IConfigurationSection section = configuration.GetSection(sectionKey.Replace("Options", ""));

        var options = new T();
        section.Bind(options);

        servicesCollection.Configure<T>(section);
    }
}
```

Here, `AppStartupOrchestrator` is the implementation of `ServiceRegistrationOrchestrator` in the application layer, and `WebApiStartup` is the implementation of `StartupOrchestrator<TOrchestrator>` in the presentation layer.

## Contributing

Contributions are welcome! If you find a bug, want to suggest a feature, or want to contribute code, please open an issue or submit a pull request.

## License

This package is released under the MIT License. See [LICENSE](./LICENSE) for more information.
