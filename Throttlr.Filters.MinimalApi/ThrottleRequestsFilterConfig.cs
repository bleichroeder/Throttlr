using Throttlr.Filters.ContentLengthRetrievalStrategies;

namespace Throttlr.Filters.MinimalApi
{
    /// <summary>
    /// The configuration for throttling requests using minimal apis.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ThrottleRequestsFilterConfig<T>
    {
        /// <summary>
        /// Gets or sets the name of the throttlr.
        /// </summary>
        public string ThrottlrName { get; set; } = typeof(T).Name;

        /// <summary>
        /// Gets or sets the name of the input parameter.
        /// </summary>
        public string? InputParameterName { get; set; }

        /// <summary>
        /// Gets or sets the type of the content length retrieval strategy.
        /// </summary>
        public Type ContentLengthRetrievalStrategyType { get; set; } = typeof(ActionExecutedContextContentLengthRetrievalStrategy);

        /// <summary>
        /// Gets or sets the type of the parameter retrieval strategy.
        /// </summary>
        public Type? ParameterRetrievalStrategyType { get; set; }
    }
}
