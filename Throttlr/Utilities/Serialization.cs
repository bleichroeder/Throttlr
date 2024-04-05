using System.Text.Json;
using System.Text.Json.Serialization;

namespace Throttlr.Utilities
{
    /// <summary>
    /// Provides access to serialization helper methods
    /// as well as the default serialization options.
    /// </summary>
    public static class Serialization
    {
        /// <summary>
        /// Gets the default serializer options.
        /// </summary>
        /// <remarks>
        /// The default serializer options are:
        /// <list type="bullet">
        /// <item>
        /// <description>Property name case insensitive</description>
        /// </item>
        /// <item>
        /// <description>Property naming policy is camel case</description>
        /// </item>
        /// <item>
        /// <description>Write indented</description>
        /// </item>
        /// <item>
        /// <description>Converters include a <see cref="JsonStringEnumConverter"/> with camel case naming policy</description>
        /// </item>
        /// </list>
        /// </remarks>
        public static JsonSerializerOptions DefaultSerializerOptions => new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        /// <summary>
        /// Gets the default serializer options, with the option to set the indentation.
        /// </summary>
        /// <param name="pretty"></param>
        /// <returns></returns>
        public static JsonSerializerOptions GetDefaultSerializerOptions(bool pretty = false,
                                                                        bool camelCase = true,
                                                                        bool propertyNamingCaseInsensitive = true)
        {
            var defaultOptions = DefaultSerializerOptions;
            defaultOptions.WriteIndented = pretty;
            defaultOptions.PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null;
            defaultOptions.PropertyNameCaseInsensitive = propertyNamingCaseInsensitive;
            return defaultOptions;
        }
    }
}
