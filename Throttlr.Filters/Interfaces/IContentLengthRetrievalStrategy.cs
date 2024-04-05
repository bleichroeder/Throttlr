namespace Throttlr.Filters.Interfaces
{
    /// <summary>
    /// The <see cref="IContentLengthRetrievalStrategy"/> interface.
    /// Used to create a function that retrieves a parameter(s) from a given <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IContentLengthRetrievalStrategy : IThrottlrFilterStrategy
    {
        /// <summary>
        /// Determines whether the retrieval function should be executed before or after the action.
        /// </summary>
        bool PreExecution { get; }

        /// <summary>
        /// Creates <see cref="T"/> from the given <see cref="IDictionary{TKey, TValue}"/>.
        /// Allows for additional arguments to be passed in.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        Task<long> GetAsync(object? context = null);
    }
}