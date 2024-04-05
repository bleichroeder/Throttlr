using Microsoft.AspNetCore.Mvc.Filters;
using Throttlr.Filters.Interfaces;

namespace Throttlr.AspNetCore.Demo.ThrottlrStrategies
{
    /// <summary>
    /// Used for generating a <see cref="long"/> from the <see cref="IDictionary{TKey, TValue}"/> of arguments.
    /// The <see cref="object"/> would be the <see cref="ActionExecutingContext"/> or <see cref="ActionExecutedContext"/>
    /// depending on the <see cref="IContentLengthRetrievalStrategy.PreExecution"/> value.
    /// </summary>
    public class S3PreExecutionContentLengthRetrievalStrategy : IContentLengthRetrievalStrategy
    {
        /// <summary>
        /// The name of the S3 URL parameter used to retrieve the S3 URL from the request.
        /// </summary>
        private const string S3_URL_PARAM = "s3Url";

        /// <summary>
        /// The S3 URL.
        /// </summary>
        private static string? S3Url = string.Empty;

        /// <summary>
        /// Specifies that the retrieval function should be executed before the action.
        /// If this is <see cref="true"/>, the S3 URL will need to be retrievable via request parameters.
        /// </summary>
        public bool PreExecution => true;

        /// <summary>
        /// Example of a retrieval function that retrieves the content length of an S3 object.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<long> GetAsync(object? context)
        {
            // We'll get the S3Url from the request query parameters.
            if (PreExecution is true)
            {
                // The context should be an ActionExecutingContext.
                if (context is ActionExecutingContext actionExecutingContext)
                {
                    // Try and get the S3 URL from the request parameters.
                    if (actionExecutingContext.HttpContext.Request.Query.ContainsKey(S3_URL_PARAM) is false)
                    {
                        throw new Exception($"No value set for {nameof(ActionExecutingContext.HttpContext.Request.Query)} key {S3_URL_PARAM}; failed to determine S3 URL.");
                    }

                    S3Url = actionExecutingContext.HttpContext.Request.Query[S3_URL_PARAM].ToString();
                }
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
                throw new Exception($"Unable to determine content-length using {nameof(S3PreExecutionContentLengthRetrievalStrategy)}: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}.");
            }

            throw new ArgumentException($"Invalid context type {context?.GetType().Name}.");
        }
    }
}