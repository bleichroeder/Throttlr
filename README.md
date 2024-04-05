<p align="center">
  <img src="logo.png" width="350" title="hover text">
</p>

# Throttlr

Throttlr is a C# library designed to facilitate highly configurable, generic request throttling.
It supports basic rate limiting as well as bandwidth limiting using multiple window types.
Its multi-layer caching strategy leverages Redis as its primary storage mechanism for tracking request counts and timings.
In the event of a Redis failure or unavailability, Throttlr seamlessly switches to an in-memory cache as a failover mechanism, ensuring continuous service and consistent rate limiting.
This provides developers with a robust and resilient throttling solution, ideal for applications needing to maintain stability and performance under varying traffic loads.

### Basic Usage
Here we'll configure Throttlr via IServiceCollection for use in controllers as well as minimal APIs.
Multiple Throttlrs are created with varying window and limiter types.
```csharp
builder.Services.ConfigureThrottlr(configure =>
{
    // A global Redis connection is configured and shared across all Throttlrs unless a unique Redis connection is specified.
    // A global memory cache configuration is specified as well. It too will be shared unless a unique configuration is specified.
    // A collection of rules is registered here as well.
    configure.UseGlobalRedisConnection(ConnectionMultiplexer.Connect(redisConnectionString))
             .UseGlobalMemoryCacheConfiguration(new MemoryCacheConfiguration(TimeSpan.FromHours(1)))
             .UseRules(userOrderRules)

            // Here we create a Throttlr which rate limits an action within a sliding window with a duration of 1 minute.
            // The window has a maximum of 10 allowed actions.
            // A KeyBuilder function is defined allowing the Throttlr to dynamically build a key for Redis using the generic type.
            // A Redis namespace is defined, as well as a Throttlr name.
            // A unique Throttlr name allows for multiple Throttrs of the same type to be stored and accessed via the IThrottlrFactory.
             .AddSlidingThrottlr<UserOrder>(throttlr => throttlr.WithLimiterType(LimiterType.RateLimiter)
                                                                .WithWindowDuration(TimeSpan.FromSeconds(60))
                                                                .WithMaximum(10)
                                                                .WithKeyBuilder(order => $"{order.User.UserId}:RequestsPerMinute:{order.Order.OrderId}")
                                                                .WithNamespace("UserOrder")
                                                                .WithName("RequestsPerMinute"))

            // An additional rate limiting Throttlr is created.
             .AddSlidingThrottlr<UserOrder>(throttlr => throttlr.WithLimiterType(LimiterType.RateLimiter)
                                                                .WithWindowDuration(TimeSpan.FromHours(24))
                                                                .WithMaximum(10)
                                                                .WithKeyBuilder(order => $"{order.User.UserId}:RequestsPerDay:{order.Order.OrderId}")
                                                                .WithNamespace("UserOrder")
                                                                .WithName("RequestsPerDay"))

            // A bandwidth-limiting Throttlr is created.
             .AddSlidingThrottlr<UserOrder>(throttlr => throttlr.WithLimiterType(LimiterType.BandwidthLimiter)
                                                                .WithWindowDuration(TimeSpan.FromSeconds(60))
                                                                .WithMaximum(ByteConvert.FromKilobytes(3820))
                                                                .WithKeyBuilder(order => $"{order.User.UserId}:BytesPerMinute:{order.Order.OrderId}")
                                                                .WithNamespace("UserOrder")
                                                                .WithName("BytesPerMinute"))

            // A Throttlr is created using a fixed window.
             .AddFixedThrottlr<UserOrder>(throttlr => throttlr.WithLimiterType(LimiterType.BandwidthLimiter)
                                                              .WithWindowDuration(TimeSpan.FromHours(24))
                                                              .WithMaximum(ByteConvert.FromKilobytes(3820000))
                                                              .WithKeyBuilder(order => $"{order.User.UserId}:BytesPerDay:{order.Order.OrderId}")
                                                              .WithNamespace("UserOrder")
                                                              .WithName("BytesPerDay"))

// We can also register rules via the IServiceCollection.
builder.Services.UseThrottlrRules(() => userOrderRules);

// Additionally, we can register a Throttlr with a minimal API endpoint.
// Specifying an existing ThrottlrName registers the Throttlr to the endpoint.
app.MapGet("/minimal-api-user-order", (Guid userid, Guid orderid, HttpContext httpContext) =>
{
    return new UserOrder()
    {
        User = new User()
        {
            UserId = userid
        },
        Order = new Order()
        {
            OrderId = orderid
        }
    };
})
.WithThrottlr<UserOrder>(config =>
{
    config.ThrottlrName = "BytesPerDay";
    config.ParameterRetrievalStrategyType = typeof(UserOrderRetrievalStrategy);
})
.WithName("GetUserOrder")
.WithOpenApi();

});

```
