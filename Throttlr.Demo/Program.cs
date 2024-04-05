using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Reflection;
using System.Runtime.InteropServices;
using Throttlr.Interfaces;
using Throttlr.Models;
using Throttlr.Windows.RateLimiting.Sliding;

namespace Throttlr.Demo
{
    /// <summary>
    ///  A simple demo implementation of a sliding throttlr window in a console application.
    /// </summary>
    internal partial class Program
    {
        static async Task Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleCancelKeyPress);

            Assembly assembly = Assembly.GetCallingAssembly();
            Console.WriteLine($"Starting {assembly.GetName().Name} v{assembly.GetName().Version}");
            Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");

            // Load in our configuration file.
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: false)
                .Build();

            // Load in our configuration values.
            int maximum = int.Parse(configuration["Throttlr:Maximum"] ?? "5");
            TimeSpan windowTime = TimeSpan.Parse(configuration["Throttlr:WindowTime"] ?? "00:00:10");

            // Create the redis connection.
            string? connectionString = configuration["Redis:ConnectionString"]
                ?? throw new Exception($"Redis:ConnectionString cannot be null.");

            IConnectionMultiplexer redisConnection = ConnectionMultiplexer.Connect(connectionString, configuration =>
            {
                configuration.AsyncTimeout = 100;
                configuration.ConnectTimeout = 100;
                configuration.SyncTimeout = 100;
                configuration.KeepAlive = 60;
            });

            // Create a demo user.
            User user = new()
            {
                UserId = Guid.Parse("90babec3-e9aa-42bd-b85c-8df2225afc2b") // This user will have a rule applied to it.
                                                                            // Change this GUID to use the throttlr configuration without a rule.
            };

            // Create a demo order.
            Order order = new()
            {
                OrderId = Guid.NewGuid()
            };

            // Create a user order.
            UserOrder userOrder = new()
            {
                User = user,
                Order = order
            };

            // Create a configuration for a sliding throttlr window.
            // This configuration will be used to create a throttlr instance.
            // The maximum value can represent a few things depending on the limiter type:
            // 1. The maximum number of requests allowed within the time window.
            // 2. The maximum number of bytes allowed within the time window.
            // In this case we're using a Bandwidth limiter within a sliding window.
            SlidingBandwidthLimitingThrottlrWindowConfiguration<UserOrder> throttlrConfiguration = new()
            {
                Name = nameof(UserOrder),
                Namespace = nameof(UserOrder),
                KeyBuilder = (item) => $"{item.User.UserId}_{item.Order.OrderId}", // Builds the key for redis.
                Maximum = maximum,
                TimeWindow = windowTime,
                OnSuccess = (result) => // On success callback.
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Executed: {result.Item.User.UserId}_{result.Item.Order.OrderId}");
                    Console.ResetColor();

                    Console.WriteLine(result.Window.Serialize());
                    return Task.FromResult(true);
                },
                OnFailure = (result) => // On failure callback.
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Throttled: {result.Item.User.UserId}_{result.Item.Order.OrderId}");
                    Console.ResetColor();

                    Console.WriteLine(result.Window.Serialize());

                    return Task.FromResult(true);
                }
            };

            // Create a Throttlr instance using the factory.
            IThrottlr<UserOrder> throttlr = ThrottlrFactory.Default.CreateThrottlr(throttlrConfiguration, redisConnection);

            // Create a rule for a UserOrder.
            ThrottlrRuleConfiguration ruleConfiguration = new()
            {
                RuleName = "User_90babec3-e9aa-42bd-b85c-8df2225afc2b",
                ThrottlrName = throttlrConfiguration.Name,
                WindowKeyRegexPattern = "UserOrder:90babec3-e9aa-42bd-b85c-8df2225afc2b",
                MaximumOverride = 1,
                TimeWindowOverride = TimeSpan.FromSeconds(10)
            };

            // Register the rule with the rule cache.
            // The rule will be used if the UserId is 90babec3-e9aa-42bd-b85c-8df2225afc2b.
            ThrottlrRuleCache.Default.AddOrUpdateRule(new ThrottlrRule(ruleConfiguration));

            // Here we'll continually trigger the action.
            // We'll see throttlr acting in the console output as specified in the OnSuccess and OnFailure callbacks.
            while (true)
            {
                // Attempt to perform an action.
                ThrottlResult<UserOrder> result = await throttlr.CanIAsync(userOrder, new ActionData(1));

                // Output some details specific to this result.
                Console.WriteLine($"{nameof(ThrottlResult<UserOrder>)}: {nameof(ActionData)} stored in Redis using key [{result.Key}]");

                // Wait for 1 second.
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        /// <summary>
        /// Handle the SIGINT event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void HandleCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            // Perform cleanup or resource release
            Console.WriteLine("SIGINT received, exiting...");

            // Prevent the process from terminating immediately
            e.Cancel = true;

            // Optionally exit manually
            Environment.Exit(0);
        }
    }
}