using System.Text.Json;
using System.Text.Json.Serialization;
using Throttlr.Interfaces;
using Throttlr.Models;

namespace Throttlr.Windows.RateLimiting.Sliding
{
    /// <summary>
    /// The <see cref="SlidingBandwidthLimitingThrottlrWindow"/> class.
    /// </summary>
    [Serializable]
    public class SlidingBandwidthLimitingThrottlrWindow : BandwidthLimitingThrottlrWindowBase
    {
        /// <summary>
        /// Instantiates a new <see cref="SlidingBandwidthLimitingThrottlrWindow"/>.
        /// </summary>
        /// <param name="maxRequests"></param>
        /// <param name="timeWindow"></param>
        [JsonConstructor]
        public SlidingBandwidthLimitingThrottlrWindow(string throttlrName, long maxActions, TimeSpan timeWindow)
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

            // If the bytes of the single action are greater than the max bytes allowed...
            // We want to enqueue this but also return false.
            if (actionData.Bytes > MaxActions)
            {
                Slide(actionData, MaxActions);

                return false;
            }

            // If the number of requests within the sliding window is less than the maximum allowed, return true
            if (AllowedActions.TotalActionBytes < MaxActions)
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
            if (AllowedActions.TotalActionBytes >= maxActions)
            {
                if (AllowedActions.TryDequeue(out _) is false) return false;
            }

            AllowedActions.Enqueue(actionData);

            return true;
        }

        /// <summary>
        /// Serializes the <see cref="SlidingBandwidthLimitingThrottlrWindow"/> to JSON.
        /// </summary>
        /// <returns></returns>
        public override string Serialize(JsonSerializerOptions jsonSerializerOptions)
            => JsonSerializer.Serialize(this, jsonSerializerOptions);
    }
}
