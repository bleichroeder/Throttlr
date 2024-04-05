using System.Text.Json;
using System.Text.Json.Serialization;
using Throttlr.Interfaces;
using Throttlr.Models;

namespace Throttlr.Windows.RateLimiting.Fixed
{
    [Serializable]
    public class FixedBandwidthLimitingThrottlrWindow : BandwidthLimitingThrottlrWindowBase
    {

        /// <summary>
        /// Gets or sets the WindowStart.
        /// </summary>
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Instantiates a new <see cref="FixedBandwidthLimitingThrottlrWindow"/>.
        /// </summary>
        /// <param name="maxActions"></param>
        /// <param name="timeWindow"></param>
        [JsonConstructor]
        public FixedBandwidthLimitingThrottlrWindow(string throttlrName, long maxActions, TimeSpan timeWindow)
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

            if (now - WindowStart > TimeWindow)
            {
                // Reset the window and request count
                WindowStart = now;
                AllowedActions.Clear();
            }

            // If the bytes of the single action are greater than the max bytes allowed...
            // We want to enqueue this but also return false.
            if (actionData.Bytes > MaxActions)
            {
                AllowedActions.Enqueue(actionData);
                return false;
            }

            // Otherwise, we want to check if the sum of all allowed actions plus the current action is less than the max bytes allowed.
            if (AllowedActions.AdditionalBytesAllowed(actionData.Bytes, MaxActions))
            {
                AllowedActions.Enqueue(actionData);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Serializes the <see cref="SlidingWindow"/> to JSON.
        /// </summary>
        /// <remarks>
        /// It is important to remeber that 
        /// </remarks>
        /// <returns></returns>
        public override string Serialize(JsonSerializerOptions jsonSerializerOptions)
        => JsonSerializer.Serialize(this, jsonSerializerOptions);
    }
}
