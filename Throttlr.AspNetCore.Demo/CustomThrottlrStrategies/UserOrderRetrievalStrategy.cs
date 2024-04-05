using Microsoft.AspNetCore.Mvc.Filters;
using Throttlr.Filters.Interfaces;

namespace Throttlr.AspNetCore.Demo.CustomThrottlrStrategies
{
    /// <summary>
    /// Used for generating a <see cref="UserOrder"/> from the <see cref="IDictionary{TKey, TValue}"/> of arguments.
    /// The <see cref="IDictionary{TKey, TValue}"/> would be the <see cref="ActionExecutingContext.ActionArguments"/>.
    /// </summary>
    public class UserOrderRetrievalStrategy : IParameterRetrievalStrategy<UserOrder>
    {
        /// <summary>
        /// The name of the parameter that contains the user id.
        /// </summary>
        private const string USERID_PARAMETER_NAME = "userid";

        /// <summary>
        /// The name of the parameter that contains the order id.
        /// </summary>
        private const string ORDERID_PARAMETER_NAME = "orderid";

        /// <summary>
        /// Creates a <see cref="UserOrder"/> from the given <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public UserOrder Create(IDictionary<string, object?> args, string[] parameters)
        {
            // Check if the action arguments contain the user id and order id.
            if (args.TryGetValue(USERID_PARAMETER_NAME, out object? userIdValue) is false)
            {
                throw new ArgumentException($"The parameter {USERID_PARAMETER_NAME} was not found in the action arguments.");
            }

            // Check if the action arguments contain the user id and order id.
            if (args.TryGetValue(ORDERID_PARAMETER_NAME, out object? orderIdValue) is false)
            {
                throw new ArgumentException($"The parameter {ORDERID_PARAMETER_NAME} was not found in the action arguments.");
            }

            // Check if the user id and order id are null.
            if (userIdValue is null) throw new NullReferenceException($"The value of parameter {USERID_PARAMETER_NAME} was null.");
            if (orderIdValue is null) throw new NullReferenceException($"The value of parameter {ORDERID_PARAMETER_NAME} was null.");

            // Create and return the UserOrder.
            return new UserOrder()
            {
                User = new User() { UserId = (Guid)userIdValue },
                Order = new Order() { OrderId = (Guid)orderIdValue }
            };
        }
    }
}