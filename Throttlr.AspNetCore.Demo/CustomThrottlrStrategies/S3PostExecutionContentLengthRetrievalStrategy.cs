using Microsoft.AspNetCore.Mvc.Filters;
using Throttlr.Filters.Interfaces;

namespace Throttlr.AspNetCore.Demo.CustomThrottlrStrategies
{
    /// <summary>
    /// Used for generating a <see cref="long"/> from the <see cref="IDictionary{TKey, TValue}"/> of arguments.
    /// The <see cref="object"/> would be the <see cref="ActionExecutingContext"/> or <see cref="ActionExecutedContext"/>
    /// depending on the <see cref="IContentLengthRetrievalStrategy.PreExecution"/> value.
    /// </summary>
    public class S3PostExecutionContentLengthRetrievalStrategy : IContentLengthRetrievalStrategy
    {
        /// <summary>
        /// Gets the key for the S3 URL when stored within the <see cref="HttpContext"/> items dictionary.
        /// </summary>
        public static string S3UrlKey => S3_URL_KEY;

        /// <summary>
        /// The key for the S3 URL when stored within the <see cref="HttpContext"/> items dictionary.
        /// </summary>
        private const string S3_URL_KEY = "S3Url";

        /// <summary>
        /// The S3 URL.
        /// </summary>
        private static string? S3Url = string.Empty;

        /// <summary>
        /// Allows for a default constructor.
        /// </summary>
        public S3PostExecutionContentLengthRetrievalStrategy() { }

        /// <summary>
        /// Allows for the S3 URL to be set in the constructor.
        /// </summary>
        /// <param name="s3Url"></param>
        public S3PostExecutionContentLengthRetrievalStrategy(string s3Url)
        {
            S3Url = s3Url;
        }

        /// <summary>
        /// Specifies that the retrieval function should be executed before the action.
        /// If set to <see cref="false"/>, the S3 URL will be retrieved from the <see cref="HttpContext"/> items dictionary
        /// using the key <see cref="S3_URL_KEY"/>.
        /// </summary>
        public bool PreExecution => false;

        /// <summary>
        /// Example of a retrieval function that retrieves the content length of an S3 object.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<long> GetAsync(object? context)
        {
            // We'll get the S3Url from the context via the HttpContext.Items dictionary.
            if (context is FilterContext filterContext && string.IsNullOrEmpty(S3Url))
            {
                // Try and get the S3 URL from the http context.
                if (filterContext.HttpContext.Items.ContainsKey(S3_URL_KEY) is false)
                {
                    throw new Exception($"No value set for {nameof(ActionExecutedContext.HttpContext.Items)} key {S3_URL_KEY}; failed to determine S3 URL.");
                }

                S3Url = filterContext.HttpContext.Items[S3_URL_KEY]?.ToString();
            }

            // Use an HttpClient to send a HEAD request to the S3 object.
            using HttpClient client = new();
            HttpRequestMessage request = new(HttpMethod.Head, S3Url);
            HttpResponseMessage response = await client.SendAsync(request);

            // If the request was successful, return the content length.
            if (response.IsSuccessStatusCode)
            {
                if (response.Content.Headers.ContentLength.HasValue)
                {
                    return response.Content.Headers.ContentLength.Value;
                }
            }
            else
            {
                // If the request was not successful, throw an exception.
                throw new Exception($"Unable to determine content-length using {nameof(S3PostExecutionContentLengthRetrievalStrategy)}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}.");
            }

            throw new ArgumentException($"Invalid context type {context?.GetType().Name}.");
        }
    }
}