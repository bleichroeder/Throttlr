namespace Throttlr.Filters.Interfaces
{
    /// <summary>
    /// The <see cref="IParameterRetrievalStrategy{T}"/> interface.
    /// Used to create a function that retrieves a parameter(s) from a given <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IParameterRetrievalStrategy<T> : IThrottlrFilterStrategy
    {
        /// <summary>
        /// Creates <see cref="T"/> from the given <see cref="IDictionary{TKey, TValue}"/>.
        /// Allows for additional arguments to be passed in.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        T Create(IDictionary<string, object?> args, params string[] additionalArgs);
    }
}