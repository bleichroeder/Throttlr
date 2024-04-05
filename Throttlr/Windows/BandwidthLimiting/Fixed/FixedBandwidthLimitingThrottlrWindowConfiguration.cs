using Throttlr.Models;
using Throttlr.Windows.Configuration;
using Throttlr.Windows.RateLimiting.Fixed;

namespace Throttlr.Types.RateLimiting.Fixed
{
    /// <summary>
    /// The <see cref="FixedBandwidthLimitingThrottlrWindowConfiguration{T}"/> class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedBandwidthLimitingThrottlrWindowConfiguration<T> : ThrottlrWindowConfigurationBase<T>
    {
        /// <summary>
        /// Gets the limiter type.
        /// </summary>
        public override LimiterType LimiterType => LimiterType.BandwidthLimiter;

        /// <summary>
        /// Gets the window type.
        /// </summary>
        /// <returns></returns>
        public override Type GetWindowType() => typeof(FixedBandwidthLimitingThrottlrWindow);
    }
}
