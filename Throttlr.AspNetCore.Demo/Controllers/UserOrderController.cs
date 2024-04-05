using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Throttlr.AspNetCore.Demo.CustomThrottlrStrategies;
using Throttlr.AspNetCore.Demo.ThrottlrStrategies;
using Throttlr.Filters.ContentLengthRetrievalStrategies;
using Throttlr.Filters.Interfaces;
using Throttlr.Filters.Mvc;
using Throttlr.Filters.Utilities;
using Throttlr.Models;

namespace Throttlr.AspNetCore.Demo.Controllers
{
    /// <summary>
    /// UserOrderController used for testing various <see cref="Throttlr{T}"/> implementations.
    /// </summary>
    /// <remarks>
    /// UserOrderController used for testing various <see cref="Throttlr{T}"/> implementations.
    /// </remarks>
    /// <param name="logger"></param>
    /// <param name="throttlrFactory"></param>
    [ApiController]
    [Route("[controller]")]
    public partial class UserOrderController(ILogger<UserOrderController> logger) : ControllerBase
    {
        private readonly ILogger<UserOrderController> _logger = logger;

        /// <summary>
        /// POST method which tests the <see cref="ThrottleRequests{T}"/> attribute filter.
        /// In this example two types of <see cref="Throttlr{T}"/> are implemented.
        /// <br/>
        /// <br/>
        /// 1) Two <see cref="LimiterType.RateLimiter"/> windows are used to limit the number of requests per day and per minute.
        /// <br/>
        /// 2) Two <see cref="LimiterType.BandwidthLimiter"/> windows are used to limit the number of bytes per day and per minute.
        /// <br/>
        /// <br/>
        /// The <see cref="LimiterType.BandwidthLimiter"/> windows use the default <see cref="IContentLengthRetrievalStrategy"/>,
        /// <see cref="ActionExecutedContextContentLengthRetrievalStrategy"/>, to retrieve the content length of the response from the <see cref="ActionExecutedContext"/>.
        /// <br/>
        /// <br/>
        /// No <see cref="IParameterRetrievalStrategy{T}"/>, has been specified and instead the <paramref name="userOrder"/> is used directly.
        /// The filter will attempt to retrieve the <see cref="UserOrder"/> from the action arguments using the name of the matching input parameter.
        /// </summary>
        /// <param name="userOrder"></param>
        /// <returns></returns>
        [HttpPost("AssumeInputParameterName")]
        [ThrottleRequests<UserOrder>(throttlrName: "RequestsPerMinute")]
        [ThrottleRequests<UserOrder>(throttlrName: "RequestsPerMinute")]
        [ThrottleRequests<UserOrder>(throttlrName: "BytesPerDay")]
        [ThrottleRequests<UserOrder>(throttlrName: "BytesPerMinute")]
        public IActionResult PostAssumeInputParameter(UserOrder userOrder)
            => Ok($"User {userOrder.User.UserId} order {userOrder.Order.OrderId} is allowed.");

        /// <summary>
        /// POST method which tests the <see cref="ThrottleRequests{T}"/> attribute filter.
        /// In this example two types of <see cref="Throttlr{T}"/> are implemented.
        /// <br/>
        /// <br/>
        /// 1) Two <see cref="LimiterType.RateLimiter"/> windows are used to limit the number of requests per day and per minute.
        /// <br/>
        /// 2) Two <see cref="LimiterType.BandwidthLimiter"/> windows are used to limit the number of bytes per day and per minute.
        /// <br/>
        /// <br/>
        /// The <see cref="LimiterType.BandwidthLimiter"/> windows use the default <see cref="IContentLengthRetrievalStrategy"/>,
        /// <see cref="ActionExecutedContextContentLengthRetrievalStrategy"/>, to retrieve the content length of the response from the <see cref="ActionExecutedContext"/>.
        /// <br/>
        /// <br/>
        /// No <see cref="IParameterRetrievalStrategy{T}"/>, has been specified and instead the <paramref name="userOrder"/> is used directly.
        /// The filter will attempt to retrieve the <see cref="UserOrder"/> from the action arguments by the name specified.
        /// </summary>
        /// <param name="userOrder"></param>
        /// <returns></returns>
        [HttpPost("SpecifiedInputParameterName")]
        [ThrottleRequests<UserOrder>(inputParameterName: "userOrder", "RequestsPerDay")]
        [ThrottleRequests<UserOrder>(inputParameterName: "userOrder", "RequestsPerMinute")]
        [ThrottleRequests<UserOrder>(inputParameterName: "userOrder", "BytesPerDay")]
        [ThrottleRequests<UserOrder>(inputParameterName: "userOrder", "BytesPerMinute")]
        public IActionResult PostPlaceUserOrder(UserOrder userOrder)
            => Ok($"User {userOrder.User.UserId} order {userOrder.Order.OrderId} is allowed.");

        /// <summary>
        /// GET method which tests the <see cref="ThrottleRequests{T}"/> attribute filter.
        /// In this example two types of <see cref="Throttlr{T}"/> are implemented.
        /// <br/>
        /// <br/>
        /// 1) Two <see cref="LimiterType.RateLimiter"/> windows are used to limit the number of requests per day and per minute.
        /// <br/>
        /// 2) Two <see cref="LimiterType.BandwidthLimiter"/> windows are used to limit the number of bytes per day and per minute.
        /// <br/>
        /// <br/>
        /// Additionally, the <see cref="LimiterType.BandwidthLimiter"/> windows employ a custom <see cref="IContentLengthRetrievalStrategy"/>,
        /// <see cref="S3PostExecutionContentLengthRetrievalStrategy"/>, to retrieve the content length of the response by performing a HEAD request to an S3 bucket.
        /// <br/>
        /// <br/>
        /// All of the implemented <see cref="Throttlr{T}"/> employ the <see cref="IParameterRetrievalStrategy{T}"/>, <see cref="UserOrderRetrievalStrategy"/>,
        /// to retrieve a <see cref="UserOrder"/> from the values of the specified input parameters.
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        [HttpGet("S3MediaDownload-PostExecution")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), "RequestsPerDay")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), "RequestsPerMinute")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), typeof(S3PostExecutionContentLengthRetrievalStrategy), "BytesPerDay")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), typeof(S3PostExecutionContentLengthRetrievalStrategy), "BytesPerMinute")]
        public IActionResult GetS3MediaPostExecution(Guid userid, Guid orderid)
        {
            _logger.LogInformation("Processing {userOrder} for user {userid} [OrderId:{orderid}]", nameof(UserOrder), userid, orderid);

            // Here we would determine the URL to the file in S3.
            // Typically this would be done using the userId and orderId or other parameters/logic.
            // Doesn't matter how we get this, but for this example it is hard-coded.
            string s3Url = "https://globalnightlight.s3.amazonaws.com/npp_202012/npp_202012_catalog.json";

            // We can set the URL for the S3 HEAD request within the HttpContext for access within the S3ContentLengthRetrievalStrategy.
            // The S3ContentLengthRetrievalStrategy will retrieve the content length of the response by performing a HEAD request to the S3 bucket.
            HttpContext.CreateContextItem(S3PostExecutionContentLengthRetrievalStrategy.S3UrlKey, s3Url);

            // Redirect to the media file.
            // Before the response is sent, the ThrottleRequests filter will check the bandwidth limit.
            // If the limit is exceeded, a 429 Too Many Requests response will be sent.
            return Redirect(s3Url);
        }

        /// <summary>
        /// GET method which tests the <see cref="ThrottleRequests{T}"/> attribute filter.
        /// In this example two types of <see cref="Throttlr{T}"/> are implemented.
        /// <br/>
        /// <br/>
        /// 1) Two <see cref="LimiterType.RateLimiter"/> windows are used to limit the number of requests per day and per minute.
        /// <br/>
        /// 2) Two <see cref="LimiterType.BandwidthLimiter"/> windows are used to limit the number of bytes per day and per minute.
        /// <br/>
        /// <br/>
        /// Additionally, the <see cref="LimiterType.BandwidthLimiter"/> windows employ a custom <see cref="IContentLengthRetrievalStrategy"/>,
        /// <see cref="S3PreExecutionContentLengthRetrievalStrategy"/>, to retrieve the content length of the response by performing a HEAD request to an S3 bucket.
        /// <br/>
        /// <br/>
        /// All of the implemented <see cref="Throttlr{T}"/> employ the <see cref="IParameterRetrievalStrategy{T}"/>, <see cref="UserOrderRetrievalStrategy"/>,
        /// to retrieve a <see cref="UserOrder"/> from the values of the specified input parameters.
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        [HttpGet("S3MediaDownload-PreExecution")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), "RequestsPerDay")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), "RequestsPerMinute")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), typeof(S3PreExecutionContentLengthRetrievalStrategy), "BytesPerDay")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), typeof(S3PreExecutionContentLengthRetrievalStrategy), "BytesPerMinute")]
        public async Task<IActionResult> GetS3MediaPreExecution(Guid userid, Guid orderid, string s3Url = "https://globalnightlight.s3.amazonaws.com/npp_202012/npp_202012_catalog.json")
        {
            // If we're here, we know the file can be served.
            _logger.LogInformation("Processing {userOrder} for user {userid} [OrderId:{orderid}]", nameof(UserOrder), userid, orderid);

            // Download the bytes.
            byte[] mediaBytes = await System.IO.File.ReadAllBytesAsync(s3Url);

            // Return the media file.
            // We could also redirect to the media.
            // What we do doesn't matter at this point as we've already determined the media can be served.
            return File(mediaBytes, "application/json");
        }

        /// <summary>
        /// GET method which tests the <see cref="ThrottleRequests{T}"/> attribute filter.
        /// In this example two types of <see cref="Throttlr{T}"/> are implemented.
        /// <br/>
        /// <br/>
        /// 1) Two <see cref="LimiterType.RateLimiter"/> windows are used to limit the number of requests per day and per minute.
        /// <br/>
        /// 2) Two <see cref="LimiterType.BandwidthLimiter"/> windows are used to limit the number of bytes per day and per minute.
        /// <br/>
        /// <br/>
        /// The <see cref="LimiterType.BandwidthLimiter"/> windows use the default <see cref="IContentLengthRetrievalStrategy"/>,
        /// <see cref="ActionExecutedContextContentLengthRetrievalStrategy"/>, to retrieve the content length of the response from the <see cref="ActionExecutedContext"/>.
        /// <br/>
        /// <br/>
        /// All of the implemented <see cref="Throttlr{T}"/> employ the <see cref="IParameterRetrievalStrategy{T}"/>, <see cref="UserOrderRetrievalStrategy"/>,
        /// to retrieve a <see cref="UserOrder"/> from the values of the specified input parameters.
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="orderid"></param>
        /// <returns></returns>
        [HttpGet]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), "RequestsPerDay")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), "RequestsPerMinute")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), "BytesPerDay")]
        [ThrottleRequests<UserOrder>(typeof(UserOrderRetrievalStrategy), "BytesPerMinute")]
        public IActionResult Get(Guid userid, Guid orderid)
            => Ok($"User {userid} order {orderid} is allowed.");
    }
}