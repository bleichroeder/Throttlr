using System.Text.Json;
using Throttlr.Interfaces;
using Throttlr.Models;

namespace Throttlr.Windows.RateLimiting
{
    /// <summary>
    /// The <see cref="BandwidthLimitingThrottlrWindowBase"/> class.
    /// </summary>
    [Serializable]
    public abstract class BandwidthLimitingThrottlrWindowBase : IThrottlrWindow
    {
        /// <summary>
        /// Gets the LimiterType.
        /// </summary>
        public LimiterType LimiterType => LimiterType.BandwidthLimiter;

        /// <summary>
        /// Gets or sets the ThrottlrName.
        /// </summary>
        public string ThrottlrName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the MaxRequests.
        /// </summary>
        public long MaxActions { get; set; }

        /// <summary>
        /// Gets the RateLimit_Limit.
        /// </summary>
        public long RateLimit_Limit => MaxActions;

        /// <summary>
        /// Gets the RateLimit_Remaining.
        /// </summary>
        public long RateLimit_Remaining
        {
            get
            {

                long actual = MaxActions - AllowedActions.TotalActionBytes;

                return actual < 0 ? 0 : actual;
            }
        }

        /// <summary>
        /// Gets the RateLimit_Reset.
        /// The time remaining in the current window, specified in seconds.
        /// </summary>
        public long RateLimit_Reset
        {
            get
            {
                if (AllowedActions.Count == 0)
                {
                    return (long)TimeWindow.TotalSeconds;
                }

                DateTime oldestRequestTime = AllowedActions.Peek().Timestamp; // Gets the oldest request without dequeuing.
                TimeSpan elapsedSinceOldestRequest = DateTime.UtcNow - oldestRequestTime;
                TimeSpan remaining = TimeWindow - elapsedSinceOldestRequest;
                return (long)remaining.TotalSeconds;
            }
        }

        /// <summary>
        /// Gets or sets the TimeWindow.
        /// </summary>
        public TimeSpan TimeWindow { get; set; }

        /// <summary>
        /// Gets or sets the Requests.
        /// </summary>
        public ActionsQueue<ActionData> AllowedActions { get; set; } = new();

        /// <summary>
        /// Gets the number of requests in the <see cref="SlidingThrottlrWindow"/>.
        /// </summary>
        public long AllowedActionsCount => AllowedActions.Count;

        /// <summary>
        /// Returns true if the requested action is allowed.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsAllowed(ActionData actionData, IThrottlrRule? rule = null);

        /// <summary>
        /// Validates that the current window configuration matches the configuration passed in.
        /// If not we'll want to create a new window.
        /// This can be caused by a configuration change while a window is still active within redis or memory.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool MatchesConfiguration<T>(IThrottlrConfiguration<T> configuration)
            => configuration is null
                ? throw new ArgumentNullException(nameof(configuration))
                : configuration.Maximum == MaxActions &&
                   configuration.TimeWindow == TimeWindow &&
                   configuration.Name == ThrottlrName;

        /// <summary>
        /// Serializes the <see cref="SlidingWindow"/> to JSON.
        /// </summary>
        /// <returns></returns>
        public abstract string Serialize(JsonSerializerOptions jsonSerializerOptions);
    }
}
