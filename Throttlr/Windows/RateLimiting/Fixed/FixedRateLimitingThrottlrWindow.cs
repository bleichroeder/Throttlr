using System.Text.Json;
using System.Text.Json.Serialization;
using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Utilities;

namespace Throttlr.Windows.RateLimiting.Fixed
{
    [Serializable]
    public class FixedRateLimitingThrottlrWindow : RateLimitingThrottlrWindowBase
    {

        /// <summary>
        /// Gets or sets the WindowStart.
        /// </summary>
        public DateTime WindowStart { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Instantiates a new <see cref="FixedRateLimitingThrottlrWindow"/>.
        /// </summary>
        /// <param name="maxRequests"></param>
        /// <param name="timeWindow"></param>
        [JsonConstructor]
        public FixedRateLimitingThrottlrWindow(string throttlrName, long maxActions, TimeSpan timeWindow)
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

            if (AllowedActionsCount < MaxActions)
            {
                AllowedActions.Enqueue(actionData);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Serializes the <see cref="FixedRateLimitingThrottlrWindow"/> to JSON.
        /// </summary>
        /// <remarks>
        /// The default <see cref="JsonSerializerOptions"/> defined in <see cref="Serialization.DefaultSerializerOptions"/> are used.
        /// </remarks>
        /// <returns></returns>
        public override string Serialize() => Serialize(Serialization.DefaultSerializerOptions);

        /// <summary>
        /// Serializes the <see cref="FixedRateLimitingThrottlrWindow"/> to JSON.
        /// </summary>
        /// <returns></returns>
        public override string Serialize(JsonSerializerOptions jsonSerializerOptions)
        => JsonSerializer.Serialize(this, jsonSerializerOptions);
    }
}
