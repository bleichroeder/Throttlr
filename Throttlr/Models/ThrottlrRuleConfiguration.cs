using System.Text.RegularExpressions;
using Throttlr.Interfaces;

namespace Throttlr.Models
{
    /// <summary>
    /// The <see cref="ThrottlrRuleConfiguration"/> class.
    /// </summary>
    public class ThrottlrRuleConfiguration : IThrottlrRuleConfiguration
    {
        public string RuleName { get; set; } = "Unnamed Rule";

        /// <summary>
        /// Gets or sets the regular expression used when checking
        /// if this rule applies to the input action.
        /// </summary>
        public string WindowKeyRegexPattern { get; set; } = ".*";

        /// <summary>
        /// Gets or sets the maximum value override.
        /// </summary>
        public long MaximumOverride { get; set; }

        /// <summary>
        /// Gets or sets the time window override.
        /// </summary>
        public TimeSpan TimeWindowOverride { get; set; }

        /// <summary>
        /// Gets or sets the target Throttlr name.
        /// </summary>
        /// <remarks>
        /// If this is left unset, this rule will run on all throttlrs.
        /// </remarks>
        public string? ThrottlrName { get; set; }

        /// <summary>
        /// Gets or sets the enabled value.
        /// </summary>
        /// <remarks>
        /// If false, this rule will not be used.
        /// </remarks>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets the <see cref="WindowKeyRegexPattern"/> as a <see cref="Regex"/>.
        /// </summary>
        public Regex WindowKeyRegex => new(WindowKeyRegexPattern);

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottlrRuleConfiguration"/> class.
        /// </summary>
        public ThrottlrRuleConfiguration() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ThrottlrRuleConfiguration"/> class.
        /// </summary>
        /// <param name="ruleName"></param>
        /// <param name="windowKeyRegex"></param>
        /// <param name="maximumOverride"></param>
        /// <param name="timeWindowOverride"></param>
        /// <param name="throttlrName"></param>
        public ThrottlrRuleConfiguration(string ruleName,
                                         string windowKeyRegex,
                                         long maximumOverride,
                                         TimeSpan timeWindowOverride,
                                         string? throttlrName)
        {
            RuleName = ruleName;
            WindowKeyRegexPattern = windowKeyRegex;
            MaximumOverride = maximumOverride;
            TimeWindowOverride = timeWindowOverride;
            ThrottlrName = throttlrName;
            IsEnabled = true;
        }
    }
}
