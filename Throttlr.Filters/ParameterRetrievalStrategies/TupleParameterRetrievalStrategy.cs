using Throttlr.Filters.Interfaces;

namespace Throttlr.Filters.ParameterRetrievalStrategies
{
    /// <summary>
    /// For retrieving a Tuple of 2 parameters.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class TupleRetrieval<T1, T2> : IParameterRetrievalStrategy<Tuple<T1, T2>>
    {
        private static readonly IParameterRetrievalStrategy<Tuple<T1, T2>> _instance = new TupleRetrieval<T1, T2>();

        // Private constructor to restrict instantiation from outside
        private TupleRetrieval() { }

        /// <summary>
        /// The instance of the <see cref="TupleRetrieval{T1, T2}"/>.
        /// </summary>
        public static IParameterRetrievalStrategy<Tuple<T1, T2>> Instance => _instance;

        /// <summary>
        /// Creates a Tuple of 2 parameters from the given <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="additionalArgs"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public Tuple<T1, T2> Create(IDictionary<string, object?> args, params string[] additionalArgs)
        {
            if (additionalArgs.Length is not 2)
            {
                throw new ArgumentException("The number of additional arguments must be 2.");
            }

            if (args.TryGetValue(additionalArgs[0], out object? value1) is false)
            {
                throw new ArgumentException($"The parameter {additionalArgs[0]} was not found in the action arguments.");
            }

            if (args.TryGetValue(additionalArgs[1], out object? value2) is false)
            {
                throw new ArgumentException($"The parameter {additionalArgs[1]} was not found in the action arguments.");
            }

            if (value1 is null)
            {
                throw new NullReferenceException($"The value of parameter {additionalArgs[0]} was null.");
            }

            if (value2 is null)
            {
                throw new NullReferenceException($"The value of parameter {additionalArgs[1]} was null.");
            }

            return new((T1)value1, (T2)value2);
        }
    }
}
