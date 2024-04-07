<p align="center">
  <img src="logo.png" width="350" title="hover text">
</p>

# Throttlr: Advanced Request Throttling for .NET Applications

Throttlr is a comprehensive, highly configurable C# library designed to empower developers with robust request throttling capabilities.
It offers an extensive suite of features, including rate limiting, bandwidth management, and support for multiple window types, catering to a wide array of throttling needs.
At its core, Throttlr utilizes a multi-layer caching strategy, with Redis as the primary storage for tracking request counts and timings, ensuring high performance and scalability.

#### Key Features:
* Highly Configurable: Tailor Throttlr's behavior to fit your application's specific needs, with extensive customization options.
* Generic Implementation: Easily integrate Throttlr with various types of requests or resources, thanks to its generic design.
* Rate and Bandwidth Limiting: Not just limiting requests per second but also managing the data bandwidth, making it ideal for applications dealing with significant data transfer.
* Multiple Window Types: Choose the best throttling algorithm for your scenario, whether it's a fixed window, sliding window, or another, to balance accuracy and memory usage effectively.
* Resilient Caching Strategy: A primary Redis cache for optimal performance, with an in-memory fallback to ensure continuous operation, even during Redis outages.

Throttlr is the go-to solution for .NET developers looking to maintain stability and performance in their applications under varying traffic loads.
Its resilience, powered by a failover mechanism, guarantees consistent rate limiting, providing a seamless experience even in the face of unexpected infrastructure issues.

Whether you're building a high-traffic web API, a resource-intensive microservice, or any application in between, Throttlr gives you the tools to protect your resources, ensure equitable access, and keep your services running smoothly.

Join the Throttlr community on GitHub, contribute to the project, and help shape the future of request throttling in .NET applications.

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
