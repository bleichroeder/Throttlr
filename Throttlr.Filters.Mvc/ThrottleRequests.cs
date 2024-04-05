using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Throttlr.Filters.ContentLengthRetrievalStrategies;
using Throttlr.Filters.Interfaces;
using Throttlr.Interfaces;

namespace Throttlr.Filters.Mvc
{
    /// <summary>
    /// Throttle requests based on the input parameter of the method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class ThrottleRequests<T> : Attribute, IFilterFactory
    {
        private readonly string _throttlerName = typeof(T).Name;
        private readonly string _inputParameterName;
        private readonly Type _contentLengthRetrievalStrategyType = typeof(ActionExecutedContextContentLengthRetrievalStrategy);
        private readonly Type? _parameterRetrievalStrategyType;

        /// <summary>
        /// True if the ThrottleRequests attribute is reusable.
        /// </summary>
        public bool IsReusable => false;

        /// <summary>
        /// Throttle requests based on the input parameter of the method.
        /// Throttlr will be retrieved from the DI container via IThrottlrFactory.
        /// The Throttlr name will be the name of the input parameter.
        /// The input paramter name will be assumed as the name of the input parameter.
        /// </summary>
        public ThrottleRequests()
        {
            _throttlerName = typeof(T).Name;
            _inputParameterName = typeof(T).Name;
        }

        /// <summary>
        /// Throttle requests based on the input parameter of the method.
        /// Throttlr will be retrieved from the DI container via IThrottlrFactory by the
        /// specified throttlrName.
        /// </summary>
        /// <param name="throttlrName"></param>
        public ThrottleRequests(string throttlrName)
        {
            _throttlerName = throttlrName;
            _inputParameterName = typeof(T).Name;
        }

        /// <summary>
        /// Throttle requests based on the input parameter of the method.
        /// Throttlr will be retrieved from the DI container via IThrottlrFactory by the
        /// specified throttlr name.
        /// </summary>
        /// <param name="inputParameterName"></param>
        /// <param name="throttlerName"></param>
        public ThrottleRequests(string inputParameterName, string? throttlerName = null)
        {
            _throttlerName = throttlerName ?? typeof(T).Name;
            _inputParameterName = inputParameterName;
        }

        /// <summary>
        /// Throttle requests based on the input parameter of the method.
        /// Uses the specified factory type to create the input object from action arguments.
        /// Throttlr will be retrieved from the DI container via IThrottlrFactory by the
        /// specified throttlr name.
        /// </summary>
        /// <param name="parameterRetrievalStrategyType"></param>
        /// <param name="throttlrName"></param>
        public ThrottleRequests(Type parameterRetrievalStrategyType, string? throttlrName = null)
        {
            _throttlerName = throttlrName ?? typeof(T).Name;
            _parameterRetrievalStrategyType = parameterRetrievalStrategyType;
            _inputParameterName = string.Empty;

            ValidateParameterRetrievalFactoryType(parameterRetrievalStrategyType);
        }

        /// <summary>
        /// Throttle requests based on the input parameter of the method.
        /// Uses the specified content length retrieval delegate to get the response length after the action has been executed.
        /// Throttlr will be retrieved from the DI container via IThrottlrFactory by the
        /// specified throttlr name.
        /// </summary>
        /// <param name="inputParameterName"></param>
        /// <param name="contentLengthRetrievalDelegate"></param>
        /// <param name="throttlerName"></param>
        public ThrottleRequests(string inputParameterName,
                                Type? contentLengthRetrievalStrategyType = null,
                                string? throttlerName = null)
        {
            _throttlerName = throttlerName ?? typeof(T).Name;
            _inputParameterName = inputParameterName;

            if (contentLengthRetrievalStrategyType is not null)
            {
                ValidateContentLengthRetrievalFactoryType(contentLengthRetrievalStrategyType);

                _contentLengthRetrievalStrategyType = contentLengthRetrievalStrategyType;
            }
        }

        /// <summary>
        /// Throttle requests based on the input parameter of the method.
        /// Uses the specified factory type to create the input object from action arguments.
        /// Uses the specified content length retrieval delegate to get the response length after the action has been executed.
        /// </summary>
        /// <param name="parameterRetrievalStrategyType"></param>
        /// <param name="contentLengthRetrievalDelegate"></param>
        /// <param name="throttlerName"></param>
        public ThrottleRequests(Type parameterRetrievalStrategyType, Type? contentLengthRetrievalStrategyType = null, string? throttlerName = null)
        {
            _throttlerName = throttlerName ?? typeof(T).Name;
            _parameterRetrievalStrategyType = parameterRetrievalStrategyType;
            _inputParameterName = string.Empty;

            ValidateStrategies(parameterRetrievalStrategyType, contentLengthRetrievalStrategyType);

            if (contentLengthRetrievalStrategyType is not null)
            {
                _contentLengthRetrievalStrategyType = contentLengthRetrievalStrategyType;
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="ThrottleFilterImplementation"/>.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            IThrottlrFactory throttlingService = serviceProvider.GetRequiredService<IThrottlrFactory>();

            return _parameterRetrievalStrategyType is not null ?
                new ThrottlrFilter<T>(_throttlerName, _parameterRetrievalStrategyType, _contentLengthRetrievalStrategyType, throttlingService)
              : new ThrottlrFilter<T>(_throttlerName, _inputParameterName, _contentLengthRetrievalStrategyType, throttlingService);
        }

        /// <summary>
        /// Validates the specified strategies.
        /// </summary>
        /// <param name="parameterRetrievalStrategyType"></param>
        /// <param name="contentLengthRetrievalStrategyType"></param>
        private static void ValidateStrategies(Type parameterRetrievalStrategyType, Type? contentLengthRetrievalStrategyType)
        {
            ValidateParameterRetrievalFactoryType(parameterRetrievalStrategyType);

            if (contentLengthRetrievalStrategyType is not null)
                ValidateContentLengthRetrievalFactoryType(contentLengthRetrievalStrategyType);
        }

        /// <summary>
        /// Validates the specified parameter retrieval factory type.
        /// </summary>
        /// <param name="parameterRetrievalStrategyType"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidateParameterRetrievalFactoryType(Type parameterRetrievalStrategyType)
        {
            if (typeof(IParameterRetrievalStrategy<T>).IsAssignableFrom(parameterRetrievalStrategyType) is false)
                throw new ArgumentException($"The specified type {parameterRetrievalStrategyType.Name} does not implement {nameof(IParameterRetrievalStrategy<T>)}");
        }

        /// <summary>
        /// Validates the specified content length retrieval factory type.
        /// </summary>
        /// <param name="contentLengthRetrievalStrategyType"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidateContentLengthRetrievalFactoryType(Type contentLengthRetrievalStrategyType)
        {
            if (typeof(IContentLengthRetrievalStrategy).IsAssignableFrom(contentLengthRetrievalStrategyType) is false)
                throw new ArgumentException($"The specified type {contentLengthRetrievalStrategyType.Name} does not implement {nameof(IContentLengthRetrievalStrategy)}");
        }
    }
}