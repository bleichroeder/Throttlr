using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using Throttlr.Filters.Interfaces;
using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Utilities;

namespace Throttlr.Filters.Mvc
{
    /// <summary>
    /// Throttlr filter implementation.
    /// </summary>
    public class ThrottlrFilter<T> : IThrottlrFilter<T>, IAsyncActionFilter
    {
        private static readonly ILogger<ThrottleRequests<T>> _logger = GetLogger();

        private readonly string _throttlerName;
        private readonly string _inputParameterName;
        private readonly IThrottlrFactory _throttlingService;
        private readonly Type? _contentLengthRetrievalStrategyType;
        private readonly Type? _parameterRetrievalStrategyType;
        private readonly JsonSerializerOptions _serializerOptions = Serialization.DefaultSerializerOptions;

        /// <summary>
        /// Throttlr filter implementation.
        /// </summary>
        /// <param name="throttlerName"></param>
        /// <param name="inputParameterName"></param>
        /// <param name="contentLengthRetrievalStrategyType"></param>
        /// <param name="throttlrFactory"></param>
        public ThrottlrFilter(string throttlerName,
                              string inputParameterName,
                              Type contentLengthRetrievalStrategyType,
                              IThrottlrFactory throttlrFactory)
        {
            _throttlerName = throttlerName;
            _inputParameterName = inputParameterName;
            _contentLengthRetrievalStrategyType = contentLengthRetrievalStrategyType;
            _throttlingService = throttlrFactory;
        }

        /// <summary>
        /// Throttlr filter implementation.
        /// </summary>
        /// <param name="throttlerName"></param>
        /// <param name="parameterRetrievlaStrategyType"></param>
        /// <param name="contentLengthRetrievalStrategyType"></param>
        /// <param name="throttlrFactory"></param>
        public ThrottlrFilter(string throttlerName,
                              Type parameterRetrievlaStrategyType,
                              Type contentLengthRetrievalStrategyType,
                              IThrottlrFactory throttlrFactory)
        {
            _throttlerName = throttlerName;
            _parameterRetrievalStrategyType = parameterRetrievlaStrategyType;
            _contentLengthRetrievalStrategyType = contentLengthRetrievalStrategyType;
            _throttlingService = throttlrFactory;
            _inputParameterName = string.Empty;
        }

        /// <summary>
        /// Creates a logger for the filter.
        /// </summary>
        private static ILogger<ThrottleRequests<T>> GetLogger()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddFilter("Microsoft", LogLevel.Warning)
                       .AddFilter("System", LogLevel.Warning)
                       .AddFilter(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, LogLevel.Debug)
                       .AddConsole();
            });

            return loggerFactory.CreateLogger<ThrottleRequests<T>>();
        }

        /// <summary>
        /// Called before the action executes, after model binding is complete.
        /// </summary>
        /// <param name="executingContext"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task OnActionExecutionAsync(ActionExecutingContext executingContext, ActionExecutionDelegate next)
        {
            // Get the throttlr from the DI container.
            IThrottlr<T> throttlr = _throttlingService.GetThrottlr<T>(_throttlerName) ??
                throw new InvalidOperationException($"No throttler of type {typeof(T).Name} with name {_throttlerName} was found.");

            // Attempt to retrieve the key object from the action arguments.
            T keyObject = IThrottlrFilter<T>.RetrieveKeyObject(executingContext.ActionArguments, _parameterRetrievalStrategyType, _inputParameterName);

            // Create action data.
            ActionData actionData = new();

            // If this is a bandwidth limiting throttlr, we need to retrieve the content length.
            IContentLengthRetrievalStrategy? contentLengthStrategy = null;
            if (throttlr.LimiterType is LimiterType.BandwidthLimiter && _contentLengthRetrievalStrategyType is not null)
            {
                // Create the content length retrieval strategy.
                contentLengthStrategy = IThrottlrFilter<T>.CreateStrategy<IContentLengthRetrievalStrategy>(_contentLengthRetrievalStrategyType);

                // If we need to we'll determine the keyObject's content length and set it.
                actionData.Bytes = await IThrottlrFilter<T>.RetrieveContentLengthAsync(executingContext.HttpContext.Items, contentLengthStrategy);
            }

            // Check if the request is allowed.
            ThrottlResult<T> result = await throttlr.CanIAsync(keyObject, actionData);

            // Get a window for the input object.
            IThrottlrWindow window = result.Window;

            // We want to add the custom headers to the response.
            // We only add the limit for custom headers.
            executingContext.HttpContext.Response.Headers.TryAdd(IThrottlrFilter<T>.GetCustomLimitHeader(_throttlerName), window.RateLimit_Limit.ToString());

            // Now we have to add standard headers to the response.
            // We'll expose Remaining and Rest using whichever limit is lower/closest to throttling.
            // We can check the current header value to see if this remaining value is lower than the current one.
            // If this is the lower limit, or the only limit, we'll set standard headers.
            IThrottlrFilter<T>.SetResponseHeadersIfNeeded(executingContext.HttpContext.Response.Headers, window);

            // If the window is not allowed, return a 429.
            if (window is not null && result.IsAllowed is false)
            {
                string serializedHeaders = JsonSerializer.Serialize(executingContext.HttpContext.Response.Headers, _serializerOptions);

                _logger?.LogDebug("Throttled {key}", result.Key);

                _logger?.LogTrace("[TraceId:{traceId}]:\n{headers}",
                                  executingContext.HttpContext.TraceIdentifier,
                                  serializedHeaders);

                executingContext.Result = new StatusCodeResult((int)HttpStatusCode.TooManyRequests);

                return;
            }

            // Get the executed context.
            ActionExecutedContext executedContext = await next();

            // If we're bandwidth limiting, now we have to update the window with the response size.
            switch (throttlr.LimiterType)
            {
                // For bandwidth limiting we need to update the bandwidth limit with the response size.
                case LimiterType.BandwidthLimiter:
                    {
                        // Are we doing post-execution content length retrieval?
                        if (contentLengthStrategy?.PreExecution is false)
                        {
                            // Let's get the content length here.
                            long contentLength = await IThrottlrFilter<T>.RetrieveContentLengthAsync(executedContext.HttpContext.Items, contentLengthStrategy);

                            // Check if we can complete the action.
                            await throttlr.CanIAsync(keyObject, new ActionData(contentLength));
                        }
                    }
                    break;
            }
        }
    }
}