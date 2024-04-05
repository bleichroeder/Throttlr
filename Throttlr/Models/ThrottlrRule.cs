using Throttlr.Interfaces;

namespace Throttlr.Models
{

    /// <summary>
    /// The <see cref="ThrottlrRule"/> class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ruleConfiguration"></param>
    /// <param name="ruleAppliesFunction"></param>
    public class ThrottlrRule(ThrottlrRuleConfiguration ruleConfiguration) : IThrottlrRule
    {
        /// <summary>
        /// Gets or sets the <see cref="ThrottlrRuleConfiguration"/>.
        /// </summary>
        public ThrottlrRuleConfiguration RuleConfiguration { get; internal set; } = ruleConfiguration;

        /// <summary>
        /// True if this rule should be applied to this action.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Applies<T>(IThrottlrConfiguration<T> inputConfiguration, T targetItem)
        {
            // If this rule has a target Throttlr we check that it matches.
            if (string.IsNullOrEmpty(RuleConfiguration.ThrottlrName) is false)
                if (RuleConfiguration.ThrottlrName != inputConfiguration.Name)
                    return false;

            // Get the input key.
            string windowKey = inputConfiguration.BuildKey(targetItem);

            // If enabled, we check if the rule Regex matches the input configuration BuildKey() result.
            return RuleConfiguration.IsEnabled && RuleConfiguration.WindowKeyRegex.Match(windowKey).Success;
        }

        /// <summary>
        /// True if this rule should be applied to this window.
        /// </summary>
        /// <param name="window"></param>
        /// <returns></returns>
        public bool Applies(IThrottlrWindow window)
        {
            // If this rule has a target Throttlr we check that it matches.
            if (string.IsNullOrEmpty(RuleConfiguration.ThrottlrName) is false)
                if (RuleConfiguration.ThrottlrName != window.ThrottlrName)
                    return false;

            // If enabled, we check if the rule Regex matches the input configuration BuildKey() result.
            return RuleConfiguration.IsEnabled;
        }
    }
}
