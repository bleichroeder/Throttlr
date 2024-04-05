using Throttlr.Models;
using Throttlr.Windows.Configuration;

namespace Throttlr.Windows.RateLimiting.Sliding
{
    /// <summary>
    /// The <see cref="SlidingBandwidthLimitingThrottlrWindowConfiguration{T}"/> class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SlidingRateLimitingThrottlrWindowConfiguration<T> : ThrottlrWindowConfigurationBase<T>
    {
        public override LimiterType LimiterType => LimiterType.RateLimiter;

        /// <summary>
        /// Gets the window type.
        /// </summary>
        /// <returns></returns>
        public override Type GetWindowType() => typeof(SlidingRateLimitingThrottlrWindow);
    }
}
