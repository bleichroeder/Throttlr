using System.Text.Json;
using Throttlr.Models;

namespace Throttlr.Interfaces
{
    /// <summary>
    /// <see cref="IWindowConfigurationContext{T}"/> interface for fluent <see cref="IThrottlr{T}"/> configuration.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IWindowConfigurationContext<T>
    {
        /// <summary>
        /// Init phase.
        /// </summary>
        interface IInitialization
        {
            /// <summary>
            /// Configures the window type.
            /// </summary>
            /// <param name="windowType"></param>
            /// <returns></returns>
            IInitialization_WindowType WithWindowType(WindowType windowType);
        }

        /// <summary>
        /// Init0 phase.
        /// </summary>
        interface IInitialization_WindowType
        {
            /// <summary>
            /// Configures the window limiter type.
            /// </summary>
            /// <param name="windowDuration"></param>
            /// <returns></returns>
            IConfigure_WindowDuration WithLimiterType(LimiterType limiterType);
        }

        interface IConfigure_WindowDuration
        {
            /// <summary>
            /// Configures the window duration.
            /// </summary>
            /// <param name="windowDuration"></param>
            /// <returns></returns>
            IConfigure_WindowMaximum WithWindowDuration(TimeSpan windowDuration);
        }

        /// <summary>
        /// Configure phase 1.
        /// </summary>
        interface IConfigure_WindowMaximum
        {
            /// <summary>
            /// Configures the maximum requests.
            /// </summary>
            /// <param name="maximumActions"></param>
            /// <returns></returns>
            IConfigure_KeyBuilder WithMaximum(long maximumActions);
        }

        /// <summary>
        /// Configure phase 2.
        /// </summary>
        interface IConfigure_KeyBuilder
        {
            /// <summary>
            /// Configures the key builder.
            /// </summary>
            /// <param name="keyBuilder"></param>
            /// <returns></returns>
            IConfigure_Optional WithKeyBuilder(Func<T, string> keyBuilder);
        }

        /// <summary>
        /// Optional phase.
        /// </summary>
        public interface IConfigure_Optional
        {
            /// <summary>
            /// Configures the name.
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            IConfigure_Optional WithName(string name);

            /// <summary>
            /// Configures the namespace.
            /// </summary>
            /// <param name="nameSpace"></param>
            /// <returns></returns>
            IConfigure_Optional WithNamespace(string nameSpace);

            /// <summary>
            /// Configures the on success function.
            /// </summary>
            /// <param name="onSuccess"></param>
            /// <returns></returns>
            IConfigure_Optional WithOnSuccessFunction(Func<ThrottlResult<T>, Task<bool>> onSuccess);

            /// <summary>
            /// Configures the on failure function.
            /// </summary>
            /// <param name="onFailure"></param>
            /// <returns></returns>
            IConfigure_Optional WithOnFailureFunction(Func<ThrottlResult<T>, Task<bool>> onFailure);

            /// <summary>
            /// Configures the JsonSerializerOptions.
            /// </summary>
            /// <param name="jsonSerializerOptions"></param>
            /// <returns></returns>
            IConfigure_Optional WithJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions);

            /// <summary>
            /// Builds the <see cref="IThrottlrConfiguration{T}"/> instance.
            /// </summary>
            /// <returns></returns>
            IThrottlrConfiguration<T> Build();
        }
    }
}
