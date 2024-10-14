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
                var expressionAsString = GetExpressionAsString(expression);

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

        /// <summary>
        /// Converts an expression representing a service registration into a human-readable string.
        /// The string includes the method name, generic arguments (if any), and the parameters passed to the method.
        /// Extension methods are handled by recognizing the 'this' parameter (e.g., <see cref="IServiceCollection"/>).
        /// This method is used to format log messages for service registration activities.
        /// </summary>
        /// <param name="expression">
        /// The <see cref="Expression{Action}"/> object representing the service registration action to be converted to a string.
        /// This expression is expected to be a method call expression.
        /// </param>
        /// <returns>
        /// A formatted string representing the method call, including the method name, any generic arguments,
        /// and the parameters passed to the method. If the method is an extension method, the 'this' parameter is explicitly marked.
        /// </returns>
        /// <exception cref="InvalidCastException">
        /// Thrown if the expression body is not a <see cref="MethodCallExpression"/>. This method expects the expression body to be a method call.
        /// </exception>
        protected virtual string GetExpressionAsString(Expression<Action<IServiceCollection, IConfiguration>> expression)
        {
            var methodCall = (MethodCallExpression)expression.Body;

            // Get the method name
            string methodName = methodCall.Method.Name;

            // Handle generic arguments (for cases like AddScoped<IMyService, MyService>)
            string genericArgs = string.Empty;
            if (methodCall.Method.IsGenericMethod)
            {
                var genericArguments = methodCall.Method.GetGenericArguments()
                                                        .Select(arg => arg.Name)
                                                        .ToArray();
                genericArgs = $"<{string.Join(", ", genericArguments)}>";
            }

            // Handle normal parameters (for cases like AddScoped(typeof(IMyService), typeof(MyService)))
            bool isExtensionMethod = methodCall.Method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
            var normalArgs = methodCall.Arguments
                                       .Select((arg, index) =>
                                       {
                                           if (index == 0 && isExtensionMethod)
                                           {
                                               return $"this {arg.Type.Name}";
                                           }

                                           if (arg is ConstantExpression constExpr && constExpr.Value != null)
                                           {
                                               return $"{constExpr.Type.Name}<{constExpr.Value}>";
                                           }

                                           if (arg is LambdaExpression lambdaExpr)
                                           {
                                               var parameters = lambdaExpr.Parameters.Select(x => x.Type.Name).ToArray();
                                               var parameterString = string.Join(",", parameters);
                                               return $"({parameterString}) => {lambdaExpr.Body}";
                                           }

                                           if (arg is MethodCallExpression methodCallExpr)
                                           {
                                               return methodCallExpr.Method.ReturnType.Name;
                                           }

                                           if (arg is TypeBinaryExpression typeBinaryExpr)
                                           {
                                               return typeBinaryExpr.Type.Name;
                                           }

                                           return arg.Type.Name;
                                       })
                                       .ToArray();

            string parameters = string.Join(", ", normalArgs.Where(x => x != null));

            return $"{methodName}{genericArgs}({parameters})";
        }
    }
}