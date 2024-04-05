using Microsoft.AspNetCore.Http;
using Throttlr.Filters.Interfaces;

namespace Throttlr.Filters.Utilities
{
    /// <summary>
    /// Provides helper methods for the <see cref="HttpContext"/>.
    /// </summary>
    public static class HttpContextHelper
    {
        /// <summary>
        /// Creates a new key value pair in the <see cref="HttpContext.Items"/> dictionary.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static HttpContext CreateContextItem(this HttpContext context, string key, object value)
        {
            context.Items.Add(key, value);
            return context;
        }

        /// <summary>
        /// Determines and sets the target length in the <see cref="HttpContext.Items"/> dictionary
        /// using the given <paramref name="strategy"/>.
        /// This value is then accessible in the <see cref="ThrottleRequests{T}"/> filter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="strategy"></param>
        /// <returns></returns>
        public static async Task<HttpContext> SetTargetLengthAsync(this HttpContext context, IContentLengthRetrievalStrategy strategy) =>
            SetTargetLength(context, await strategy.GetAsync(context));

        /// <summary>
        /// Determines and sets the target length in the <see cref="HttpContext.Items"/> dictionary
        /// using the given length retrieval function.
        /// This value is then accessible in the <see cref="ThrottleRequests{T}"/> filter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="lengthRetrievalFunction"></param>
        /// <returns></returns>
        public static HttpContext SetTargetLength(this HttpContext context, Func<long> lengthRetrievalFunction) =>
            SetTargetLength(context, lengthRetrievalFunction());

        /// <summary>
        /// Determines and sets the target length in the <see cref="HttpContext.Items"/> dictionary
        /// using the given length retrieval function.
        /// This value is then accessible in the <see cref="ThrottleRequests{T}"/> filter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="lengthRetrievalFunction"></param>
        /// <returns></returns>
        public static async Task<HttpContext> SetTargetLengthAsync(this HttpContext context, Func<Task<long>> lengthRetrievalFunction) =>
            SetTargetLength(context, await lengthRetrievalFunction());

        /// <summary>
        /// Sets the target length in the <see cref="HttpContext.Items"/> dictionary
        /// using the given length.
        /// This value is then accessible in the <see cref="ThrottleRequests{T}"/> filter.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static HttpContext SetTargetLength(this HttpContext context, long length)
        {
            context.Items[ThrottlrFilterConstants.TARGET_LENGTH_ITEM_KEY] = length;
            return context;
        }
    }
}
