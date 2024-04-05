using Throttlr.Models;
using Throttlr.Windows.Configuration;
using Throttlr.Windows.RateLimiting.Fixed;

namespace Throttlr.Types.RateLimiting.Fixed
{
    /// <summary>
    /// The <see cref="FixedBandwidthLimitingThrottlrWindowConfiguration{T}"/> class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedRateLimitingThrottlrWindowConfiguration<T> : ThrottlrWindowConfigurationBase<T>
    {
        /// <summary>
        /// Gets the limiter type.
        /// </summary>
        public override LimiterType LimiterType => LimiterType.RateLimiter;

        /// <summary>
        /// Gets the window type.
        /// </summary>
        /// <returns></returns>
        public override Type GetWindowType() => typeof(FixedRateLimitingThrottlrWindow);
    }
}
