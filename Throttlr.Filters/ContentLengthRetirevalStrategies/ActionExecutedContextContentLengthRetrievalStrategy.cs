using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;
using System.Text.Json;
using Throttlr.Filters.Interfaces;

namespace Throttlr.Filters.ContentLengthRetrievalStrategies
{
    /// <summary>
    /// A content-length retrieval strategy that retrieves the content-length from an <see cref="ActionExecutedContext"/>.
    /// </summary>
    public class ActionExecutedContextContentLengthRetrievalStrategy : IContentLengthRetrievalStrategy
    {
        /// <summary>
        /// Determines whether the retrieval function should be executed before or after the action.
        /// </summary>
        /// <remarks>
        /// This strategy should be executed after the action which means that the <see cref="ActionExecutedContext"/> should be used.
        /// </remarks>
        public bool PreExecution => false;

        /// <summary>
        /// Creates a function that retrieves the response content-length from an <see cref="ActionExecutedContext"/>.
        /// </summary>
        /// <param name="executedContext"></param>
        /// <returns></returns>
        public async Task<long> GetAsync(object? executedContext = null) => await Task.Run(() => DefaultStrategy((ActionExecutedContext?)executedContext));

        /// <summary>
        /// Default implementation for retrieving the content length from an <see cref="ActionExecutedContext"/>.
        /// </summary>
        private static Func<ActionExecutedContext?, long> DefaultStrategy => (executedContext)
            => executedContext?.Result switch
            {
                ObjectResult objectResult => Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(objectResult.Value)),
                FileContentResult fileContentResult => fileContentResult.FileContents.Length,
                ContentResult contentResult => Encoding.UTF8.GetByteCount(contentResult.Content ?? string.Empty),
                _ => 0,
            };
    }
}
