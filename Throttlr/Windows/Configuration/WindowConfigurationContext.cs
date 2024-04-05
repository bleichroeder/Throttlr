using System.Text.Json;
using Throttlr.Interfaces;
using Throttlr.Models;

namespace Throttlr.Windows.Configuration
{
    /// <summary>
    /// Fluent API for configuring a <see cref="IThrottlrConfiguration{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class WindowConfigurationContext<T> : IWindowConfigurationContext<T>.IInitialization,
                                                          IWindowConfigurationContext<T>.IInitialization_WindowType,
                                                          IWindowConfigurationContext<T>.IConfigure_WindowDuration,
                                                          IWindowConfigurationContext<T>.IConfigure_WindowMaximum,
                                                          IWindowConfigurationContext<T>.IConfigure_KeyBuilder,
                                                          IWindowConfigurationContext<T>.IConfigure_Optional
    {
        public abstract IWindowConfigurationContext<T>.IInitialization_WindowType WithWindowType(WindowType windowType);

        public abstract IWindowConfigurationContext<T>.IConfigure_WindowDuration WithLimiterType(LimiterType limiterType);

        public abstract IWindowConfigurationContext<T>.IConfigure_WindowMaximum WithWindowDuration(TimeSpan windowDuration);

        public abstract IWindowConfigurationContext<T>.IConfigure_KeyBuilder WithMaximum(long maximum);

        public abstract IWindowConfigurationContext<T>.IConfigure_Optional WithKeyBuilder(Func<T, string> keyBuilder);

        public abstract IWindowConfigurationContext<T>.IConfigure_Optional WithName(string name);

        public abstract IWindowConfigurationContext<T>.IConfigure_Optional WithNamespace(string nameSpace);

        public abstract IWindowConfigurationContext<T>.IConfigure_Optional WithOnSuccessFunction(Func<ThrottlResult<T>, Task<bool>> onSuccess);

        public abstract IWindowConfigurationContext<T>.IConfigure_Optional WithOnFailureFunction(Func<ThrottlResult<T>, Task<bool>> onFailure);

        public abstract IWindowConfigurationContext<T>.IConfigure_Optional WithJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions);

        public abstract IThrottlrConfiguration<T> Build();
    }
}
