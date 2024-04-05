using System.Text.RegularExpressions;

namespace Throttlr.Interfaces
{
    /// <summary>
    /// The <see cref="IThrottlrRuleConfiguration"/> interface.
    /// </summary>
    public interface IThrottlrRuleConfiguration
    {
        /// <summary>
        /// Gets the rule name.
        /// </summary>
        string RuleName { get; internal set; }

        /// <summary>
        /// Gets the regular expression pattern used when checking
        /// if this rule applies to the input action.
        /// </summary>
        string WindowKeyRegexPattern { get; internal set; }

        /// <summary>
        /// Gets the <see cref="WindowKeyRegexPattern"/> as a <see cref="Regex"/>.
        /// </summary>
        Regex WindowKeyRegex { get; }

        /// <summary>
        /// Gets the maximum value override.
        /// </summary>
        long MaximumOverride { get; set; }

        /// <summary>
        /// Gets the time window override.
        /// </summary>
        TimeSpan TimeWindowOverride { get; set; }

        /// <summary>
        /// Gets the target Throttlr name.
        /// </summary>
        string? ThrottlrName { get; set; }

        /// <summary>
        /// Gets the enabled value.
        /// </summary>
        bool IsEnabled { get; set; }
    }
}
