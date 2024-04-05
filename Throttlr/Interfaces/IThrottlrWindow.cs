using System.Text.Json;
using Throttlr.Models;
using Throttlr.Utilities;

namespace Throttlr.Interfaces
{
    /// <summary>
    /// The <see cref="IThrottlrWindow"/> interface.
    /// </summary>
    public interface IThrottlrWindow
    {
        /// <summary>
        /// Gets or sets the LimiterType.
        /// </summary>
        LimiterType LimiterType { get; }

        /// <summary>
        /// Gets or sets the ThrottlrName.
        /// </summary>
        string ThrottlrName { get; set; }

        /// <summary>
        /// Gets or sets the Requests.
        /// </summary>
        ActionsQueue<ActionData> AllowedActions { get; set; }

        /// <summary>
        /// Gets or sets the MaxRequests.
        /// </summary>
        long MaxActions { get; set; }

        /// <summary>
        /// Gets the RateLimit_Limit.
        /// </summary>
        long RateLimit_Limit { get; }

        /// <summary>
        /// Gets RateLimit_Remaining.
        /// </summary>
        long RateLimit_Remaining { get; }

        /// <summary>
        /// Gets the RateLimit_Reset.
        /// </summary>
        long RateLimit_Reset { get; }

        /// <summary>
        /// Gets or sets the RequestCount.
        /// </summary>
        long AllowedActionsCount { get; }

        /// <summary>
        /// Returns true if the <see cref="IThrottlrWindow"/> matches the <see cref="IThrottlrConfiguration{T}"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        bool MatchesConfiguration<T>(IThrottlrConfiguration<T> configuration);

        /// <summary>
        /// True if the requested action is allowed.
        /// </summary>
        /// <param name="actionData"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        bool IsAllowed(ActionData actionData, IThrottlrRule? rule = null);

        /// <summary>
        /// Serializes the <see cref="IThrottlrWindow"/> to a JSON string.
        /// </summary>
        /// <remarks>
        /// The default <see cref="JsonSerializerOptions"/> defined in <see cref="Serialization.DefaultSerializerOptions"/> are used.
        /// </remarks>
        /// <returns></returns>
        string Serialize() => Serialize(Serialization.DefaultSerializerOptions);

        /// <summary>
        /// Serializes the <see cref="IThrottlrWindow"/> to a JSON string.
        /// </summary>
        /// <param name="pretty"></param>
        /// <returns></returns>
        string Serialize(JsonSerializerOptions jsonSerializerOptions);

        /// <summary>
        /// Deserializes the <see cref="IThrottlrWindow"/> from a JSON string.
        /// </summary>
        /// <remarks>
        /// The default <see cref="JsonSerializerOptions"/> are used.
        /// </remarks>
        /// <param name="json"></param>
        /// <param name="jsonSerializer"></param>
        /// <returns></returns>
        static T Deserialize<T>(string json) where T : IThrottlrWindow
            => Deserialize<T>(json, Serialization.DefaultSerializerOptions);

        /// <summary>
        /// Deserializes the <see cref="IThrottlrWindow"/> from a JSON string.
        /// </summary>
        /// <remarks>
        /// The default <see cref="JsonSerializerOptions"/> are used if not specified.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        static T Deserialize<T>(string json, JsonSerializerOptions jsonSerializerOptions) where T : IThrottlrWindow
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(nameof(json));

            T window = JsonSerializer.Deserialize<T>(json, jsonSerializerOptions) ??
                throw new NullReferenceException($"The {nameof(IThrottlrWindow)} was null upon deserialization. " +
                                                 $"Ensure your window is being serialized using the same {nameof(JsonSerializerOptions)}.");

            return window;
        }

        /// <summary>
        /// Deserializes the <see cref="IThrottlrWindow"/> from a JSON string.
        /// </summary>
        /// <remarks>
        /// The default <see cref="JsonSerializerOptions"/> are used if not specified.
        /// </remarks>
        /// <param name="json"></param>
        /// <param name="returnType"></param>
        /// <returns></returns>
        static IThrottlrWindow Deserialize(string json, Type returnType) => Deserialize(json, returnType, Serialization.DefaultSerializerOptions);

        /// <summary>
        /// Deserializes the <see cref="IThrottlrWindow"/> from a JSON string.
        /// </summary>
        /// <param name="json"></param>
        /// <param name="returnType"></param>
        /// <param name="jsonSerializerOptions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        static IThrottlrWindow Deserialize(string json, Type returnType, JsonSerializerOptions jsonSerializerOptions)
        {
            if (string.IsNullOrWhiteSpace(json)) throw new ArgumentNullException(nameof(json));

            object? window = JsonSerializer.Deserialize(json, returnType, jsonSerializerOptions);

            if (window is not null && window is IThrottlrWindow validWindow)
                return validWindow;
            else
                throw new NullReferenceException($"The {nameof(IThrottlrWindow)} was null upon deserialization. " +
                                                 $"Ensure your window is being serialized using the same {nameof(JsonSerializerOptions)}.");
        }
    }
}
