# StartupOrchestration.NET

[![Main Status](https://github.com/cbcrouse/StartupOrchestration.NET/actions/workflows/dotnet.main.status.yml/badge.svg)](https://github.com/cbcrouse/StartupOrchestration.NET/actions/workflows/dotnet.main.status.yml) [![NuGet Downloads](https://img.shields.io/nuget/dt/StartupOrchestration.NET)](https://www.nuget.org/stats/packages/StartupOrchestration.NET?groupby=Version) [![NuGet Version](https://img.shields.io/nuget/v/StartupOrchestration.NET)](https://www.nuget.org/packages/StartupOrchestration.NET) [![codecov](https://codecov.io/gh/cbcrouse/StartupOrchestration.NET/branch/main/graph/badge.svg?token=XVPL3HNHDG)](https://codecov.io/gh/cbcrouse/StartupOrchestration.NET) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=cbcrouse_StartupOrchestration.NET&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=cbcrouse_StartupOrchestration.NET)

## 🔍 Overview

StartupOrchestration.NET is a powerful tool for .NET developers that want to write clean, organized and easily-maintainable code. With its use, developers can separate the presentation layer from the application layer, providing a flexible and extensible way to register services. Additionally, the library's built-in support for startup logging provides an easy way to diagnose and troubleshoot any registration issues during application startup. With StartupOrchestration.NET, developers can focus on the business logic of their applications, without having to worry about the details of service registration. Overall, if you want to write high-quality .NET code, StartupOrchestration.NET is a must-have tool.

---

## ✅ Features

- Enables the application layer to manage service registrations independently of the presentation layer, allowing for a more modular and maintainable codebase.
- Facilitates separation of concerns between the presentation and application layers, preventing any presentation-specific dependencies from creeping into the application layer.
- Provides a generic implementation that can be easily customized for different types of applications, saving development time and effort.
- Offers a flexible interface for orchestrating service registrations that is agnostic to the presentation layer, allowing for more modular design and easier maintainability.
- Reduces duplicated configuration code between the presentation and application layers, improving maintainability and reducing the risk of configuration errors, as well as simplifying secret management by reducing the number of places to store secrets such as connection strings.
- Enables dynamic logging of service registration expressions for easier startup debugging, making it easier to diagnose and resolve any issues during the startup process.
- Can be used with a variety of different presentation layer types, including console apps, web APIs, and MVC applications, providing a high degree of flexibility and adaptability.

---

## 👤 Audience

This library is aimed at developers who are building applications that may grow in size and complexity over time. If you're working on a small, simple application that you don't anticipate growing much, StartupOrchestration&#46;NET may not be necessary for your project.

However, if you're building a more ambitious application or multiple applications that share common services, StartupOrchestration&#46;NET can be a valuable tool for streamlining the initialization and configuration of your application's dependencies and services. This library helps you reduce boilerplate code and makes it easier to maintain your application's dependencies as it grows.

In addition, this library is designed specifically for .NET applications, taking advantage of the unique features and capabilities of the .NET framework. This makes it an ideal choice for .NET developers who want to simplify the development process and ensure that their applications are built to last.

---

## 📦 Getting Started

You can install the package via NuGet. Search for `StartupOrchestration.NET` or run the following command:

```sh
dotnet add package StartupOrchestration.NET
```

---

## 🧑‍💻 Usage

Here's an example of how the `ServiceRegistrationOrchestrator` and `StartupOrchestrator` classes might be used in a .NET solution following a clean architecture structure:

> NOTE: A clean architecture structure is not required to use StartupOrchestration.NET. It is simply an example of how the library can be used in a real-world application.

```css
MyProject/
├─ src/
│  ├─ Application/
│  │  └─ MyApp/
│  │     ├─ MyApp.csproj
│  │     └─ ...
│  ├─ Infrastructure/
│  │  ├─ Startup/
│  │  │  └─ AppStartupOrchestrator.cs
│  │  ├─ Persistence/
│  │  │  ├─ Repositories/
│  │  │  │  └─ MyRepository.cs
│  │  │  └─ MyAppDbContext.cs
│  │  └─ ...
│  ├─ Presentation/
│  │  ├─ MyApp.API/
│  │  │  ├─ MyApp.API.csproj
│  │  │  ├─ WebApiStartup.cs
│  │  │  └─ ...
│  │  ├─ MyApp.Console/
│  │  │  ├─ MyApp.Console.csproj
│  │  │  ├─ ConsoleStartup.cs
│  │  │  └─ ...
│  │  ├─ MyApp.AzureFunction/
│  │  │  ├─ MyApp.AzureFunction.csproj
│  │  │  ├─ FunctionStartup.cs
│  │  │  └─ ...
│  │  └─ ...
└─ tests/
   ├─ MyApp.UnitTests/
   │  ├─ MyApp.UnitTests.csproj
   │  └─ ...
   └─ ...
```

In this example, the `Application` project contains the core business logic and is responsible for registering the required services. The `Infrastructure` project is responsible for implementing the services that the `Application` layer needs, such as database access via `MyDbContext` and `MyRepository`.

In order to decouple the presentation layer from the rest of the application, we use the `ServiceRegistrationOrchestrator` and `StartupOrchestrator` classes.

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

In this example, `AppStartupOrchestrator` inherits from `ServiceRegistrationOrchestrator` and registers the required services for the Application layer. `WebApiStartup` inherits from `StartupOrchestrator<AppStartupOrchestrator>` and uses the `ServiceRegistrationOrchestrator` in the Application layer to register its own required services.

By doing this, the Application layer becomes independent of any specific presentation layer, allowing for flexibility in creating multiple presentation layers to suit different use cases or to change the presentation layer without affecting the core application logic or it's dependencies.

> TIP: You can see how this library is implemented in <https://github.com/cbcrouse/CleanArchitecture> using more complex registrations like AutoMapper.

---

## 🔌 ServiceRegistrationOrchestrator and StartupOrchestrator

The `ServiceRegistrationOrchestrator` class is the main class that is used to manage the service registration process. It provides a way for developers to define and manage the service registration expressions that are used by the Dependency Injection (DI) container to instantiate the services used by the application. The `ServiceRegistrationOrchestrator` class is abstract and must be inherited by the application layer to implement its own registration expressions.

The `StartupOrchestrator` class is an abstract class that is intended to be inherited from a `Startup.cs` class that can be used in the presentation layer to orchestrate the startup process of the application. It allows the developer to define service registration expressions that will be used by the DI container when initializing services. By using the `StartupOrchestrator`, the developer can separate the service registration process from the startup process of the application.

Both classes are important in enabling the separation of concerns between the application and presentation layers, which makes it easier to manage the application's services and startup process. By defining and managing service registration expressions in the application layer, the developer can ensure that the service registration process is decoupled from the presentation layer.

---

## 📄 The Use of Expressions

In `ServiceRegistrationOrchestrator`, each expression in the `ServiceRegistrationExpressions` collection is a delegate that is executed during the `Orchestrate` method call. These expressions are used to register services with the dependency injection container. The benefit of using expressions instead of functions is that the expressions can be evaluated lazily. This means that the expressions are not executed until they are needed, which can improve the performance of the startup process and also avoid timing issues that arise from attempting to register services in various parts of the application.

Additionally, the expressions provide a way to perform dynamic logging of the expression body. This can be useful for debugging the startup process, as it allows developers to see exactly which expressions are being executed, and in what order.

Each expression takes in two parameters: `IServiceCollection` and `IConfiguration`. The IServiceCollection is a collection of service descriptors that is used to register services with the .NET dependency injection container. The IConfiguration is the configuration for the application. The values for these parameters are typically passed down from the presentation layer through the Startup class. By using expressions, the application layer can add services to the collection independently of the presentation layer, allowing for a more modular and organized approach to managing dependencies and configuration.

### Startup Logging

The `StartupLogger` property is used to provide a way to log startup events before the application's logger is available. Logging is an important part of any application, and startup logging is especially useful as it can provide insight into the order in which services are initialized.

Using a logging framework like Serilog, you can log startup events to a file or a database for later analysis. This can be useful in identifying startup bottlenecks, dependencies that are failing to initialize, and more. Additionally, startup logging can provide context for debugging, as you can see the order in which services are being registered and identify any potential issues early on.

#### Example Log Output

```log
[2024:10:13 07:16:48.999 PM] [Verbose] [AppStartupOrchestrator] '"AddOptions(this IServiceCollection)"' was started...
[2024:10:13 07:16:49.043 PM] [Verbose] [AppStartupOrchestrator] '"AddOptions(this IServiceCollection)"' completed successfully!
[2024:10:13 07:16:49.045 PM] [Verbose] [AppStartupOrchestrator] '"AddTransient(this IServiceCollection, Type<Microsoft.Extensions.Options.OptionsFactory`1[TOptions]>)"' was started...
[2024:10:13 07:16:49.046 PM] [Verbose] [AppStartupOrchestrator] '"AddTransient(this IServiceCollection, Type<Microsoft.Extensions.Options.OptionsFactory`1[TOptions]>)"' completed successfully!
[2024:10:13 07:16:49.048 PM] [Verbose] [AppStartupOrchestrator] '"AddTransient(this IServiceCollection, Type<Microsoft.Extensions.Options.OptionsMonitor`1[TOptions]>)"' was started...
[2024:10:13 07:16:49.049 PM] [Verbose] [AppStartupOrchestrator] '"AddTransient(this IServiceCollection, Type<Microsoft.Extensions.Options.OptionsMonitor`1[TOptions]>)"' completed successfully!
[2024:10:13 07:16:49.051 PM] [Verbose] [AppStartupOrchestrator] '"AddOptions<SqlServerOptions>(this IServiceCollection, IConfiguration)"' was started...
[2024:10:13 07:16:49.061 PM] [Verbose] [AppStartupOrchestrator] '"AddOptions<SqlServerOptions>(this IServiceCollection, IConfiguration)"' completed successfully!
[2024:10:13 07:16:49.064 PM] [Verbose] [AppStartupOrchestrator] '"AddSqlServer(this IServiceCollection)"' was started...
[2024:10:13 07:16:49.067 PM] [Verbose] [AppStartupOrchestrator] '"AddSqlServer(this IServiceCollection)"' completed successfully!
[2024:10:13 07:16:49.170 PM] [Verbose] [AppStartupOrchestrator] '"AddAuthorization(this IServiceCollection, IConfiguration)"' was started...
[2024:10:13 07:16:49.181 PM] [Verbose] [AppStartupOrchestrator] '"AddAuthorization(this IServiceCollection, IConfiguration)"' completed successfully!
[2024:10:13 07:16:49.184 PM] [Verbose] [AppStartupOrchestrator] '"AddRazorPagesWithAuthorization(this IServiceCollection, IConfiguration)"' was started...
[2024:10:13 07:16:49.262 PM] [Verbose] [AppStartupOrchestrator] '"AddRazorPagesWithAuthorization(this IServiceCollection, IConfiguration)"' completed successfully!
[2024:10:13 07:16:49.264 PM] [Verbose] [AppStartupOrchestrator] '"AddCascadingAuthenticationState(this IServiceCollection)"' was started...
[2024:10:13 07:16:49.265 PM] [Verbose] [AppStartupOrchestrator] '"AddCascadingAuthenticationState(this IServiceCollection)"' completed successfully!
[2024:10:13 07:16:49.267 PM] [Verbose] [AppStartupOrchestrator] '"AddServerSideBlazor(this IServiceCollection, Action`1)"' was started...
[2024:10:13 07:16:49.293 PM] [Verbose] [AppStartupOrchestrator] '"AddServerSideBlazor(this IServiceCollection, Action`1)"' completed successfully!
[2024:10:13 07:16:49.294 PM] [Verbose] [AppStartupOrchestrator] '"AddMudBlazorServices(this IServiceCollection, IConfiguration)"' was started...
[2024:10:13 07:16:49.303 PM] [Verbose] [AppStartupOrchestrator] '"AddMudBlazorServices(this IServiceCollection, IConfiguration)"' completed successfully!
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7206
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5035
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
```

---

## ✔ Writing Valid Service Registration Expressions

Each expression added to the `ServiceRegistrationExpressions` collection is validated before it is executed. The validation ensures that the expression is a valid extension method declared on `IServiceCollection`. This validation helps prevent runtime errors caused by incorrectly defined registration expressions. Here's some examples of a valid expressions:

```csharp
ServiceRegistrationExpressions.Add((services, config) => services.AddTransient<IMyService, MyService>());
ServiceRegistrationExpressions.Add((services, config) => services.AddTransient(typeof(IMyService), typeof(MyService)));
ServiceRegistrationExpressions.Add((services, config) => services.AddMvcCore());
ServiceRegistrationExpressions.Add((services, config) => services.RegisterOptions<MyOptions>(config));
```

---

## Contributing

Contributions are welcome! If you find a bug, want to suggest a feature, or want to contribute code, please open an issue or submit a pull request.

## License

This package is released under the MIT License. See [LICENSE](./LICENSE) for more information.
