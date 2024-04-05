using Throttlr.Filters.Interfaces;

namespace Throttlr.Filters
{
    /// <summary>
    /// Contains the constants used in the various <see cref="IThrottlrFilter{T}"/> filters.
    /// </summary>
    public static class ThrottlrFilterConstants
    {
        /// <summary>
        /// Gets the target length item key used in the <see cref="HttpContext.Items"/> dictionary.
        /// </summary>
        public const string TARGET_LENGTH_ITEM_KEY = "TargetLength";

        /// <summary>
        /// Gets the rate limit limit header key.
        /// </summary>
        public const string RATE_LIMIT_LIMIT_HEADER = "RateLimit-Limit";

        /// <summary>
        /// Gets the rate limit remaining header key.
        /// </summary>
        public const string RATE_LIMIT_REMAINING_HEADER = "RateLimit-Remaining";

        /// <summary>
        /// Gets the rate limit reset header key.
        /// </summary>
        public const string RATE_LIMIT_RESET_HEADER = "RateLimit-Reset";

        /// <summary>
        /// Gets the affective type limit header key.
        /// </summary>
        public const string AFFECTIVE_TYPE_LIMIT_HEADER = "X-RateLimit-Affective-Type";

        /// <summary>
        /// Gets the custom limit header template.
        /// </summary>
        public const string CUSTOM_LIMIT_HEADER_TEMPLATE = "X-RateLimit-{0}Limit";

    }
}
