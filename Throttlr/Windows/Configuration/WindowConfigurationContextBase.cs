using System.Text.Json;
using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Types.RateLimiting.Fixed;
using Throttlr.Utilities;
using Throttlr.Windows.RateLimiting.Sliding;

namespace Throttlr.Windows.Configuration
{
    /// <summary>
    /// Fluent API for configuring a <see cref="IThrottlrConfiguration{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WindowConfigurationContextBase<T> : IWindowConfigurationContext<T>.IInitialization,
                                                     IWindowConfigurationContext<T>.IInitialization_WindowType,
                                                     IWindowConfigurationContext<T>.IConfigure_WindowDuration,
                                                     IWindowConfigurationContext<T>.IConfigure_WindowMaximum,
                                                     IWindowConfigurationContext<T>.IConfigure_KeyBuilder,
                                                     IWindowConfigurationContext<T>.IConfigure_Optional
    {
        internal TimeSpan _windowDuration;
        internal long _maximum;
        internal Func<T, string> _keyBuilder = (item) => typeof(T).Name;
        internal string _name = typeof(T).Name;
        internal string _namespace = typeof(T).Name;
        internal LimiterType _limiterType;
        internal WindowType _windowType;
        internal Func<ThrottlResult<T>, Task<bool>> _onSuccess = (result) => Task.FromResult(true);
        internal Func<ThrottlResult<T>, Task<bool>> _onFailure = (result) => Task.FromResult(true);
        internal JsonSerializerOptions _jsonSerializerOptions = Serialization.DefaultSerializerOptions;

        /// <summary>
        /// Init stage of the fluent API.
        /// </summary>
        /// <returns></returns>
        public static IWindowConfigurationContext<T>.IInitialization Start() => new WindowConfigurationContextBase<T>();

        /// <summary>
        /// Init0 stage of the fluent API; sets the <see cref="WindowType"/>.
        /// </summary>
        /// <param name="windowType"></param>
        /// <returns></returns>
        public IWindowConfigurationContext<T>.IInitialization_WindowType WithWindowType(WindowType windowType)
        {
            _windowType = windowType;
            return this;
        }

        /// <summary>
        /// Configure0 stage of the fluent API; sets the <see cref="LimiterType"/>.
        /// </summary>
        /// <param name="limiterType"></param>
        /// <returns></returns>
        public IWindowConfigurationContext<T>.IConfigure_WindowDuration WithLimiterType(LimiterType limiterType)
        {
            _limiterType = limiterType;
            return this;
        }

        /// <summary>
        /// Configure1 stage of the fluent API; sets the <see cref="TimeSpan"/> for the window duration.
        /// </summary>
        /// <param name="windowDuration"></param>
        /// <returns></returns>
        public IWindowConfigurationContext<T>.IConfigure_WindowMaximum WithWindowDuration(TimeSpan windowDuration)
        {
            _windowDuration = windowDuration;
            return this;
        }

        /// <summary>
        /// Configure2 stage of the fluent API; sets the maximum number of requests allowed in the window.
        /// </summary>
        /// <param name="maximum"></param>
        /// <returns></returns>
        public IWindowConfigurationContext<T>.IConfigure_KeyBuilder WithMaximum(long maximum)
        {
            _maximum = maximum;
            return this;
        }

        /// <summary>
        /// Optional stage of the fluent API; sets the key builder function.
        /// </summary>
        /// <param name="keyBuilder"></param>
        /// <returns></returns>
        public IWindowConfigurationContext<T>.IConfigure_Optional WithKeyBuilder(Func<T, string> keyBuilder)
        {
            _keyBuilder = keyBuilder;
            return this;
        }

        /// <summary>
        /// Optional stage of the fluent API; sets the name of the window.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IWindowConfigurationContext<T>.IConfigure_Optional WithName(string name)
        {
            _name = name;
            return this;
        }

        /// <summary>
        /// Optional stage of the fluent API; sets the namespace of the window.
        /// </summary>
        /// <param name="nameSpace"></param>
        /// <returns></returns>
        public IWindowConfigurationContext<T>.IConfigure_Optional WithNamespace(string nameSpace)
        {
            _namespace = nameSpace;
            return this;
        }

        /// <summary>
        /// Optional stage of the fluent API; sets the on success function.
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        public IWindowConfigurationContext<T>.IConfigure_Optional WithOnSuccessFunction(Func<ThrottlResult<T>, Task<bool>> onSuccess)
        {
            _onSuccess = onSuccess;
            return this;
        }

        /// <summary>
        /// Optional stage of the fluent API; sets the on failure function.
        /// </summary>
        /// <param name="onFailure"></param>
        /// <returns></returns>
        public IWindowConfigurationContext<T>.IConfigure_Optional WithOnFailureFunction(Func<ThrottlResult<T>, Task<bool>> onFailure)
        {
            _onFailure = onFailure;
            return this;
        }

        /// <summary>
        /// Optional stage of the fluent API; sets the <see cref="JsonSerializerOptions"/>.
        /// </summary>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        public IWindowConfigurationContext<T>.IConfigure_Optional WithJsonSerializerOptions(JsonSerializerOptions jsonSerializerOptions)
        {
            _jsonSerializerOptions = jsonSerializerOptions;
            return this;
        }

        /// <summary>
        /// Builds the <see cref="IThrottlrConfiguration{T}"/> from the fluent API.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IThrottlrConfiguration<T> Build()
        {
            // Validate the configuration.
            ValidateConfiguration();

            // Build the configuration.
            switch (_windowType)
            {
                case WindowType.Sliding:
                    {
                        return _limiterType switch
                        {
                            LimiterType.RateLimiter => new SlidingRateLimitingThrottlrWindowConfiguration<T>
                            {
                                Name = _name,
                                Namespace = _namespace,
                                KeyBuilder = _keyBuilder,
                                Maximum = _maximum,
                                TimeWindow = _windowDuration,
                                OnSuccess = _onSuccess,
                                OnFailure = _onFailure
                            },
                            LimiterType.BandwidthLimiter => new SlidingBandwidthLimitingThrottlrWindowConfiguration<T>
                            {
                                Name = _name,
                                Namespace = _namespace,
                                KeyBuilder = _keyBuilder,
                                Maximum = _maximum,
                                TimeWindow = _windowDuration,
                                OnSuccess = _onSuccess,
                                OnFailure = _onFailure
                            },
                            _ => throw new InvalidOperationException($"Unsupported {nameof(LimiterType)}.")
                        };
                    }
                case WindowType.Fixed:
                    {
                        return _limiterType switch
                        {
                            LimiterType.RateLimiter => new FixedRateLimitingThrottlrWindowConfiguration<T>
                            {
                                Name = _name,
                                Namespace = _namespace,
                                KeyBuilder = _keyBuilder,
                                Maximum = _maximum,
                                TimeWindow = _windowDuration,
                                OnSuccess = _onSuccess,
                                OnFailure = _onFailure
                            },
                            LimiterType.BandwidthLimiter => new FixedBandwidthLimitingThrottlrWindowConfiguration<T>
                            {
                                Name = _name,
                                Namespace = _namespace,
                                KeyBuilder = _keyBuilder,
                                Maximum = _maximum,
                                TimeWindow = _windowDuration,
                                OnSuccess = _onSuccess,
                                OnFailure = _onFailure
                            },
                            _ => throw new InvalidOperationException($"Unsupported {nameof(LimiterType)}.")
                        };
                    }
                default: throw new InvalidOperationException($"Unsupported {nameof(WindowType)}.");
            }
        }

        /// <summary>
        /// Validates the configuration.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private void ValidateConfiguration()
        {
            // Validate the key builder function.
            if (_keyBuilder is null)
                throw new InvalidOperationException($"A {nameof(_keyBuilder)} function is required.");

            // Validate the maximum value.
            if (_maximum <= 0)
                throw new InvalidOperationException($"The {nameof(_maximum)} value must be greater than 0.");

            // Validate the window duration.
            if (_windowDuration == default)
                throw new InvalidOperationException($"The {nameof(_windowDuration)} must be greater than {default}.");

            // Validate the window type.
            if (Enum.IsDefined(typeof(WindowType), _windowType) is false)
                throw new InvalidOperationException($"Unsupported {nameof(WindowType)}.");
        }
    }
}
