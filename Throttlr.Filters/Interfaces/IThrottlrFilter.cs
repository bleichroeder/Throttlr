using Microsoft.AspNetCore.Http;
using Throttlr.Interfaces;

namespace Throttlr.Filters.Interfaces
{
    public interface IThrottlrFilter<T>
    {
        static T RetrieveKeyObject(IDictionary<string, object?> executingContext, Type? parameterRetrievalStrategyType, string? inputParameterName)
        {
            T? keyObject;

            // We need to build the key object from the action arguments.
            if (parameterRetrievalStrategyType is not null)
            {
                var parameterRetrievalStrategy = IThrottlrFilter<T>.CreateStrategy<IParameterRetrievalStrategy<T>>(parameterRetrievalStrategyType);

                keyObject = parameterRetrievalStrategy.Create(executingContext);
            }
            else if (!string.IsNullOrEmpty(inputParameterName) && executingContext.TryGetValue(inputParameterName, out object? inputObject))
            {
                // Validate the input parameter value.
                if (inputObject is null)
                    throw new InvalidOperationException($"The input parameter {inputParameterName} value was null.");

                keyObject = (T)inputObject;
            }
            else
            {
                throw new InvalidOperationException("No valid parameter retrieval method found. Verify the target action argument names match the required input parameter names.");
            }

            if (keyObject is null)
            {
                throw new InvalidOperationException($"The {nameof(keyObject)} of type {typeof(T).Name} was null.");
            }

            return keyObject;
        }

        /// <summary>
        /// Attempts to create an instance of the specified <see cref="IContentLengthRetrievalStrategy"/> and determine the keyObject <see cref="T"/> content-length.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="retrievalStrategy"></param>
        /// <returns></returns>
        static async Task<long> RetrieveContentLengthAsync(IDictionary<object, object?> context, IContentLengthRetrievalStrategy retrievalStrategy)
        {
            long contentLength = 0;

            // If we have a pre-execution strat, we'll attempt it now.
            if (retrievalStrategy?.PreExecution is true)
            {
                // Do we have the target length in the context already?
                if (context.TryGetValue(ThrottlrFilterConstants.TARGET_LENGTH_ITEM_KEY, out object? targetLength))
                {
                    // Validate the targetLength value.
                    if (targetLength is null)
                        throw new InvalidOperationException($"The {nameof(ThrottlrFilterConstants.TARGET_LENGTH_ITEM_KEY)} value was null.");

                    // Set it in ActionData.
                    contentLength = (long)targetLength;
                }
                else
                {
                    // Execute the strategy and set the length in the context.
                    contentLength = await retrievalStrategy.GetAsync(context);

                    context.Add(ThrottlrFilterConstants.TARGET_LENGTH_ITEM_KEY, contentLength);
                }
            }

            return contentLength;
        }

        /// <summary>
        /// Sets the response headers using the given <paramref name="window"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="window"></param>
        static void SetResponseHeadersIfNeeded(IHeaderDictionary headerDictionary, IThrottlrWindow window)
        {
            bool isLowerLimit = true;

            if (headerDictionary.TryGetValue(ThrottlrFilterConstants.RATE_LIMIT_REMAINING_HEADER, out var value))
            {
                if (int.TryParse(value, out var previousLimitHeaderValue))
                {
                    isLowerLimit = window.RateLimit_Remaining < previousLimitHeaderValue;
                }
            }

            if (isLowerLimit)
            {
                headerDictionary[ThrottlrFilterConstants.RATE_LIMIT_LIMIT_HEADER] = window.RateLimit_Limit.ToString();
                headerDictionary[ThrottlrFilterConstants.RATE_LIMIT_REMAINING_HEADER] = window.RateLimit_Remaining.ToString();
                headerDictionary[ThrottlrFilterConstants.RATE_LIMIT_RESET_HEADER] = window.RateLimit_Reset.ToString();
                headerDictionary[ThrottlrFilterConstants.AFFECTIVE_TYPE_LIMIT_HEADER] = window.LimiterType.ToString();
            }
        }

        /// <summary>
        /// Gets the custom limit header using the given <paramref name="throttlrName"/>
        /// and the <see cref="ThrottlrFilterConstants.CUSTOM_LIMIT_HEADER_TEMPLATE"/>.
        /// </summary>
        /// <param name="throttlrName"></param>
        /// <returns></returns>
        static string GetCustomLimitHeader(string throttlrName) => string.Format(ThrottlrFilterConstants.CUSTOM_LIMIT_HEADER_TEMPLATE, throttlrName);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TF"></typeparam>
        /// <param name="strategyType"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        static TF CreateStrategy<TF>(Type strategyType) where TF : IThrottlrFilterStrategy
        {
            object? factoryInstance = Activator.CreateInstance(strategyType)
                        ?? throw new InvalidOperationException($"Could not create instance of {strategyType.Name}");

            TF factoryInstanceFactory = (TF)factoryInstance
                ?? throw new InvalidOperationException($"Failed to cast {nameof(factoryInstance)} to {nameof(strategyType)}.");

            return factoryInstanceFactory;
        }
    }
}
