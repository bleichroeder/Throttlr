using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;
using Throttlr.Filters.Interfaces;
using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Utilities;

namespace Throttlr.Filters.MinimalApi
{
    public class ThrottleRequestsEndpointFilter<T> : IThrottlrFilter<T>, IEndpointFilter
    {
        private readonly ILogger _logger;
        private readonly string _methodName;
        private ThrottleRequestsFilterConfig<T>? _throttleRequestsFilterConfig;

        public ThrottleRequestsEndpointFilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ThrottleRequestsEndpointFilter<T>>();
            _methodName = GetType().Name;
        }

        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            // Get the throttlr config from endpoint metadata.
            _throttleRequestsFilterConfig = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<ThrottleRequestsFilterConfig<T>>()
                ?? throw new InvalidOperationException($"The {nameof(ThrottleRequestsFilterConfig<T>)} was not found in endpoint metadata.");

            // Get the throttlr from the DI container.
            IThrottlr<T> throttlr = context.HttpContext.RequestServices
                .GetRequiredService<IThrottlrFactory>()
                .GetThrottlr<T>(_throttleRequestsFilterConfig.ThrottlrName);

            var args = context.Arguments;

            // Attempt to retrieve the key object from the action arguments.
            IDictionary<string, object?> actionArguments = ConvertArgumentsToActionArguments(context);
            T keyObject = IThrottlrFilter<T>.RetrieveKeyObject(actionArguments,
                                                               _throttleRequestsFilterConfig.ParameterRetrievalStrategyType,
                                                               _throttleRequestsFilterConfig.InputParameterName);

            // Create action data.
            ActionData actionData = new();

            // If this is a bandwidth limiting throttlr, we need to retrieve the content length.
            IContentLengthRetrievalStrategy? contentLengthStrategy = null;
            if (throttlr.LimiterType is LimiterType.BandwidthLimiter && _throttleRequestsFilterConfig.ContentLengthRetrievalStrategyType is not null)
            {
                // Create the content length retrieval strategy.
                contentLengthStrategy = IThrottlrFilter<T>.CreateStrategy<IContentLengthRetrievalStrategy>(_throttleRequestsFilterConfig.ContentLengthRetrievalStrategyType);

                // If we need to we'll determine the keyObject's content length and set it.
                actionData.Bytes = await IThrottlrFilter<T>.RetrieveContentLengthAsync(context.HttpContext.Items, contentLengthStrategy);
            }

            // Check if the request is allowed.
            ThrottlResult<T> result = await throttlr.CanIAsync(keyObject, actionData);

            // Get a window for the input object.
            IThrottlrWindow window = result.Window;

            // We want to add the custom headers to the response.
            // We only add the limit for custom headers.
            context.HttpContext.Response.Headers.TryAdd(IThrottlrFilter<T>.GetCustomLimitHeader(_throttleRequestsFilterConfig.ThrottlrName), window.RateLimit_Limit.ToString());

            // Now we have to add standard headers to the response.
            // We'll expose Remaining and Rest using whichever limit is lower/closest to throttling.
            // We can check the current header value to see if this remaining value is lower than the current one.
            // If this is the lower limit, or the only limit, we'll set standard headers.
            IThrottlrFilter<T>.SetResponseHeadersIfNeeded(context.HttpContext.Response.Headers, window);

            // If the window is not allowed, return a 429.
            if (window is not null && result.IsAllowed is false)
            {
                string serializedHeaders = JsonSerializer.Serialize(context.HttpContext.Response.Headers, Serialization.DefaultSerializerOptions);

                _logger?.LogDebug("Throttled {key}", result.Key);

                _logger?.LogTrace("[TraceId:{traceId}]:\n{headers}",
                                  context.HttpContext.TraceIdentifier,
                                  serializedHeaders);

                return Results.StatusCode(429);
            }

            // Get the executed context.
            object? endpointResult = await next(context);

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
                            long contentLength = await IThrottlrFilter<T>.RetrieveContentLengthAsync(context.HttpContext.Items, contentLengthStrategy);

                            // Check if we can complete the action.
                            await throttlr.CanIAsync(keyObject, new ActionData(contentLength));
                        }
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// Converts the <see cref="EndpointFilterInvocationContext.Arguments"/> collection to <see cref="Dictionary{String, Object?}"/>. 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static Dictionary<string, object?> ConvertArgumentsToActionArguments(EndpointFilterInvocationContext context)
        {
            var methodInfo = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<MethodInfo>()
                ?? throw new InvalidOperationException($"The {nameof(MethodInfo)} was not found in endpoint metadata.");

            var parameters = methodInfo.GetParameters();
            var arguments = context.Arguments;

            Dictionary<string, object?> argsDictionary = [];

            for (int i = 0; i < parameters.Length; i++)
            {
                var parameterName = parameters[i].Name;
                var argumentValue = arguments[i];
                if (parameterName is not null)
                {
                    argsDictionary[parameterName] = argumentValue;
                }
            }

            return argsDictionary;
        }
    }
}
