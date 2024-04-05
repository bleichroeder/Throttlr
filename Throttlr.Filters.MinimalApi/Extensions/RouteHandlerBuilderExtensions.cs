using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Throttlr.Filters.Interfaces;

namespace Throttlr.Filters.MinimalApi.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="RouteHandlerBuilder"/>.
    /// </summary>
    public static class RouteHandlerBuilderExtensions
    {
        /// <summary>
        /// Adds a <see cref="IThrottlrFilter{T}"/> to the specified endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static RouteHandlerBuilder WithThrottlr<T>(this RouteHandlerBuilder builder, Action<ThrottleRequestsFilterConfig<T>> configure)
        {
            var filterConfig = new ThrottleRequestsFilterConfig<T>();
            configure(filterConfig);
            return builder.WithThrottlr(filterConfig);
        }

        /// <summary>
        /// Adds a <see cref="IThrottlrFilter{T}"/> to the specified endpoint.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="filterConfig"></param>
        /// <returns></returns>
        public static RouteHandlerBuilder WithThrottlr<T>(this RouteHandlerBuilder builder, ThrottleRequestsFilterConfig<T> filterConfig)
        {
            // Add the config to the endpoint's metadata
            builder.WithMetadata(filterConfig);

            // Register the filter type; actual filter instances will be created per-request with access to the endpoint's metadata.
            builder.AddEndpointFilter<ThrottleRequestsEndpointFilter<T>>();

            return builder;
        }
    }
}
