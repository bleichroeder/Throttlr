using System.Text.Json;
using Throttlr.Models;

namespace Throttlr.Interfaces
{
    /// <summary>
    /// The <see cref="IThrottlrConfiguration{T}"/> interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IThrottlrConfiguration<T>
    {
        /// <summary>
        /// Gets or sets the ThrottlrName.
        /// </summary>
        /// <remarks>
        /// The name is used to identify the Throttlr configuration.
        /// </remarks>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the RedisNamespace.
        /// </summary>
        /// <remarks>
        /// The namespace is used to group the keys in Redis.
        /// </remarks>
        string Namespace { set; }

        /// <summary>
        /// Gets or sets the LimiterType.
        /// </summary>
        /// <remarks>
        /// The limiter type is used to determine the type of limiter to use.
        /// (i.e. BandwidthLimiting, RateLimiting, etc...)
        /// </remarks>
        LimiterType LimiterType { get; }

        /// <summary>
        /// Gets the window type.
        /// </summary>
        /// <returns>
        /// The <see cref="IThrottlrWindow"/> <see cref="Type"/>.
        /// </returns>
        Type GetWindowType();

        /// <summary>
        /// Gets or sets the KeyBuilder.
        /// </summary>
        /// <remarks>
        /// The key builder is used to build the Redis key for the request.
        /// </remarks>
        Func<T, string> KeyBuilder { set; }

        /// <summary>
        /// On success delegate.
        /// </summary>
        /// <remarks>
        /// This delegate is called when the request is successful (i.e. the request is not throttled).
        /// </remarks>
        Func<ThrottlResult<T>, Task<bool>> OnSuccess { get; set; }

        /// <summary>
        /// On failure delegate.
        /// </summary>
        /// <remarks>
        /// This delegate is called when the request is throttled. (i.e. the request is not successful).
        /// </remarks>
        Func<ThrottlResult<T>, Task<bool>> OnFailure { get; set; }

        /// <summary>
        /// Gets or sets the MaximumRequests.
        /// </summary>
        /// <remarks>
        /// The maximum number of requests allowed within the specified time window.
        /// </remarks>
        long Maximum { get; set; }

        /// <summary>
        /// Gets or sets the TimeWindow.
        /// </summary>
        /// <remarks>
        /// The window of time in which the maximum number of requests are allowed.
        /// </remarks>
        TimeSpan TimeWindow { get; set; }

        /// <summary>
        /// Builds and returns the Redis key using
        /// the keybuilder and specified namespace if it's not null or empty.
        /// </summary>
        /// <returns>
        /// A string representing the Redis key.
        /// </returns>
        string BuildKey(T item);

        /// <summary>
        /// Gets or sets the JsonSerializerOptions.
        /// </summary>
        /// <remarks>
        /// The JSON serializer options are used to serialize and deserialize the request object.
        /// It is important that the same options are used to serialize and deserialize the request object.
        /// </remarks>
        JsonSerializerOptions JsonSerializerOptions { get; set; }
    }
}
