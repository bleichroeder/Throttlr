using System.Text.Json;
using System.Text.Json.Serialization;
using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Utilities;

namespace Throttlr.Windows.RateLimiting.Sliding
{
    /// <summary>
    /// The <see cref="SlidingRateLimitingThrottlrWindow"/> class.
    /// </summary>
    [Serializable]
    public class SlidingRateLimitingThrottlrWindow : RateLimitingThrottlrWindowBase
    {
        /// <summary>
        /// Instantiates a new <see cref="SlidingBandwidthLimitingThrottlrWindow"/>.
        /// </summary>
        /// <param name="maxRequests"></param>
        /// <param name="timeWindow"></param>
        [JsonConstructor]
        public SlidingRateLimitingThrottlrWindow(string throttlrName, long maxActions, TimeSpan timeWindow)
        {
            ThrottlrName = throttlrName;
            MaxActions = maxActions;
            TimeWindow = timeWindow;
            AllowedActions = new ActionsQueue<ActionData>();
        }

        /// <summary>
        /// Returns true if the requested action is allowed.
        /// </summary>
        /// <returns></returns>
        public override bool IsAllowed(ActionData actionData, IThrottlrRule? rule = null)
        {
            // Override the configuration in the window if a rule is provided.
            if (rule is not null)
            {
                TimeWindow = rule.RuleConfiguration.TimeWindowOverride;
                MaxActions = rule.RuleConfiguration.MaximumOverride;
            }

            DateTime now = DateTime.UtcNow;

            // Remove timestamps outside of the sliding window
            while (AllowedActionsCount > 0 && now - AllowedActions.Peek().Timestamp > TimeWindow)
            {
                AllowedActions.Dequeue();
            }

            // If the number of requests within the sliding window is less than the maximum allowed, return true
            if (AllowedActionsCount < MaxActions)
            {
                return Slide(actionData, MaxActions);
            }

            return false;
        }

        /// <summary>
        /// Slide the window.
        /// </summary>
        public bool Slide(ActionData actionData, long maxActions)
        {
            if (AllowedActionsCount >= maxActions)
            {
                if (AllowedActions.TryDequeue(out _) is false) return false;
            }

            AllowedActions.Enqueue(actionData);

            return true;
        }

        /// <summary>
        /// Serializes the <see cref="SlidingRateLimitingThrottlrWindow"/> to JSON.
        /// </summary>
        /// <remarks>
        /// The default <see cref="JsonSerializerOptions"/> defined in <see cref="Serialization.DefaultSerializerOptions"/> are used.
        /// </remarks>
        /// <returns></returns>
        public override string Serialize() => Serialize(Serialization.DefaultSerializerOptions);

        /// <summary>
        /// Serializes the <see cref="SlidingRateLimitingThrottlrWindow"/> to JSON.
        /// </summary>
        /// <returns></returns>
        public override string Serialize(JsonSerializerOptions jsonSerializerOptions)
            => JsonSerializer.Serialize(this, jsonSerializerOptions);
    }
}
