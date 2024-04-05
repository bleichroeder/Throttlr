using StackExchange.Redis;
using Throttlr.AspNetCore.Demo.CustomThrottlrStrategies;
using Throttlr.Caching;
using Throttlr.DependencyInjection;
using Throttlr.Filters.MinimalApi.Extensions;
using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Utilities;

namespace Throttlr.AspNetCore.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load in our configuration file.
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: false)
                .Build();

            string? redisConnectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";

            // Create a list of rules.
            // We'll include a single rule with a User exception.
            // This user (90babec3-e9aa-42bd-b85c-8df2225afc2b) will have a lower limit and smaller window.
            // This rule will only be applied to the Throttlr named "MinimalAPI-UserOrders"
            // First we'll build a sample rule configuration.
            // This configuration will give the user a limit of one request every 10 seconds.
            ThrottlrRuleConfiguration ruleConfiguration = new()
            {
                RuleName = "User_90babec3-e9aa-42bd-b85c-8df2225afc2b",
                ThrottlrName = "MinimalAPI-UserOrders",
                WindowKeyRegexPattern = "UserOrder:90babec3-e9aa-42bd-b85c-8df2225afc2b",
                MaximumOverride = 1,
                TimeWindowOverride = TimeSpan.FromSeconds(10)
            };

            // We'll use this configuration to build a rule and include it in a rule collection.
            // We can pass this collection of rules into the ConfigureThrottlr() method using UseRules().
            IList<IThrottlrRule> userOrderRules = new List<IThrottlrRule>()
            {
                new ThrottlrRule(ruleConfiguration)
            };

            // Configure Throttlr.
            // Here we're configuring a global Redis connection and global memory cache configuration.
            // These configurations will be shared across all throttlrs unless overridden.
            // Lastly, we add a sliding throttlr for the UserOrder class.
            // We can access this throttlr by injecting IThrottlrFactory into our controllers.
            builder.Services.ConfigureThrottlr(configure =>
            {
                configure.UseGlobalRedisConnection(ConnectionMultiplexer.Connect(redisConnectionString))
                         .UseGlobalMemoryCacheConfiguration(new MemoryCacheConfiguration(TimeSpan.FromHours(1)))
                         .UseRules(userOrderRules)

                         .AddSlidingThrottlr<UserOrder>(throttlr => throttlr.WithLimiterType(LimiterType.RateLimiter)
                                                                            .WithWindowDuration(TimeSpan.FromSeconds(60))
                                                                            .WithMaximum(10)
                                                                            .WithKeyBuilder(order => $"{order.User.UserId}:RequestsPerMinute:{order.Order.OrderId}")
                                                                            .WithNamespace("UserOrder")
                                                                            .WithName("RequestsPerMinute"))

                         .AddSlidingThrottlr<UserOrder>(throttlr => throttlr.WithLimiterType(LimiterType.RateLimiter)
                                                                            .WithWindowDuration(TimeSpan.FromHours(24))
                                                                            .WithMaximum(10)
                                                                            .WithKeyBuilder(order => $"{order.User.UserId}:RequestsPerDay:{order.Order.OrderId}")
                                                                            .WithNamespace("UserOrder")
                                                                            .WithName("RequestsPerDay"))

                         .AddSlidingThrottlr<UserOrder>(throttlr => throttlr.WithLimiterType(LimiterType.BandwidthLimiter)
                                                                            .WithWindowDuration(TimeSpan.FromSeconds(60))
                                                                            .WithMaximum(ByteConvert.FromKilobytes(3820))
                                                                            .WithKeyBuilder(order => $"{order.User.UserId}:BytesPerMinute:{order.Order.OrderId}")
                                                                            .WithNamespace("UserOrder")
                                                                            .WithName("BytesPerMinute"))

                         .AddFixedThrottlr<UserOrder>(throttlr => throttlr.WithLimiterType(LimiterType.BandwidthLimiter)
                                                                          .WithWindowDuration(TimeSpan.FromHours(24))
                                                                          .WithMaximum(ByteConvert.FromKilobytes(3820000))
                                                                          .WithKeyBuilder(order => $"{order.User.UserId}:BytesPerDay:{order.Order.OrderId}")
                                                                          .WithNamespace("UserOrder")
                                                                          .WithName("BytesPerDay"))

                         // This throttlr will be used to demo minimal API integration as well as rule overrides.
                         .AddSlidingThrottlr<UserOrder>(throttlr => throttlr.WithLimiterType(LimiterType.RateLimiter)
                                                                            .WithWindowDuration(TimeSpan.FromSeconds(60))
                                                                            .WithMaximum(10)
                                                                            .WithKeyBuilder(order => $"{order.User.UserId}:RequestsPerMinute:{order.Order.OrderId}")
                                                                            .WithNamespace("UserOrder")
                                                                            .WithName("MinimalAPI-UserOrders"));


            });

            // We can also register rules via the IServiceCollection.
            builder.Services.UseThrottlrRules(() => userOrderRules);

            builder.Services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Sample minimal API with Throttlr endpoint filter.
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
                config.ThrottlrName = "MinimalAPI-UserOrders";
                config.ParameterRetrievalStrategyType = typeof(UserOrderRetrievalStrategy);
            })
            .WithName("GetUserOrder")
            .WithOpenApi();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}