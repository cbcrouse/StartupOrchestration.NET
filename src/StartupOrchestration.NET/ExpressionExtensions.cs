using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection;

namespace StartupOrchestration.NET
{
    /// <summary>
    /// Provides extension methods for working with <see cref="Expression"/> objects.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Adds the specified service registration expression to the list of service registration expressions.
        /// The expression must be a call to the <see cref="IServiceCollection"/> interface to register a service or dependency.
        /// </summary>
        /// <param name="registrationExpression">The service registration expression to add.</param>
        /// <exception cref="ArgumentException">Thrown when the registration expression is not a call to the <see cref="IServiceCollection"/> interface.</exception>
        public static void ValidateServiceRegistration(this Expression<Action<IServiceCollection, IConfiguration>> registrationExpression)
        {
            if (registrationExpression?.Body is not MethodCallExpression methodCallExpression)
            {
                throw new ArgumentException("Registration expression must be a call to a method on IServiceCollection.", nameof(registrationExpression));
            }

            if (!methodCallExpression.IsValidExtension())
            {
                throw new ArgumentException("Only extension methods declared on IServiceCollection are allowed as service registration expressions.", nameof(registrationExpression));
            }
        }

        private static bool IsValidExtension(this MethodCallExpression methodCallExpression)
        {
            return methodCallExpression.Method.IsExtensionMethod() &&
                   typeof(IServiceCollection).IsAssignableFrom(methodCallExpression.Method.GetParameters()[0].ParameterType);
        }

        public static bool IsExtensionMethod(this MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
        }
    }
}
