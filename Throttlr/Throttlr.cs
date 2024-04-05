using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Collections.Concurrent;
using Throttlr.Caching;
using Throttlr.Interfaces;
using Throttlr.Models;

namespace Throttlr
{
    /// <summary>
    /// The <see cref="Throttlr{T}"/> class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Throttlr<T> : IThrottlr<T>, IDisposable
    {
        private readonly ILogger? _logger;

        private readonly IMemoryCacheConfiguration _memoryCacheConfiguration;
        private readonly IThrottlrConfiguration<T> _throttlrConfiguration;
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly string _throttlrName;

        private ConcurrentDictionary<string, IMemoryCacheItem<IThrottlrWindow>> _inMemoryCache = new();
        private Timer? _memoryCacheCleanupTimer;
        private bool disposedValue;

        /// <summary>
        /// Gets the <see cref="LimiterType"/> of the <see cref="IThrottlrWindow"/>.
        /// </summary>
        public LimiterType LimiterType => _throttlrConfiguration.LimiterType;

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="IThrottlrWindow"/>.
        /// </summary>
        public Type WindowType => _throttlrConfiguration.GetWindowType();

        /// <summary>
        /// Gets the name of the <see cref="Throttlr{T}"/>.
        /// </summary>
        public string ThrottlrName => _throttlrName;

        /// <summary>
        /// Creates a new instance of <see cref="Throttlr{T}"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="redisConnection"></param>
        public Throttlr(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisConnection, ILogger? logger = null)
            : this(configuration, redisConnection, new MemoryCacheConfiguration(TimeSpan.FromHours(1)), logger) { }

        /// <summary>
        /// Creates a new instance of <see cref="Throttlr{T}"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="redisConnection"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Throttlr(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisConnection, IMemoryCacheConfiguration memoryCacheConfiguration, ILogger? logger = null)
        {
            _throttlrConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
            _memoryCacheConfiguration = memoryCacheConfiguration ?? throw new ArgumentNullException(nameof(memoryCacheConfiguration));
            _throttlrName = typeof(T).Name;
            _logger = logger;

            InitializeMemoryCache();
        }

        /// <summary>
        /// Creates a new instance of <see cref="Throttlr{T}"/>.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="redisConnection"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Throttlr(IThrottlrConfiguration<T> configuration, IConnectionMultiplexer redisConnection, IMemoryCacheConfiguration memoryCacheConfiguration, string throttlrName, ILogger? logger = null)
        {
            _throttlrConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _redisConnection = redisConnection ?? throw new ArgumentNullException(nameof(redisConnection));
            _memoryCacheConfiguration = memoryCacheConfiguration ?? throw new ArgumentNullException(nameof(memoryCacheConfiguration));
            _throttlrName = throttlrName;
            _logger = logger;

            InitializeMemoryCache();
        }

        /// <summary>
        /// Checks if the specified item is allowed to be executed.
        /// </summary>
        /// <param name="targetItem"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ThrottlResult<T>> CanIAsync(T targetItem, ActionData actionData)
            => await CanIAsync(targetItem,
                               actionData,
                               _throttlrConfiguration.OnSuccess,
                               _throttlrConfiguration.OnFailure);

        /// <summary>
        /// Checks if the specified item is allowed to be executed.
        /// </summary>
        /// <param name="targetItem"></param>
        /// <param name="actionData"></param>
        /// <param name="onSuccessAction"></param>
        /// <param name="onFailureAction"></param>
        /// <returns></returns>
        public async Task<ThrottlResult<T>> CanIAsync(T targetItem,
                                                      ActionData actionData,
                                                      Func<ThrottlResult<T>, Task<bool>> onSuccessAction,
                                                      Func<ThrottlResult<T>, Task<bool>> onFailureAction)
        {
            IDatabase redisDatabase = _redisConnection.GetDatabase();

            // Build the key for lookup.
            string windowKey = _throttlrConfiguration.BuildKey(targetItem);

            // Get the window from the cache.
            IThrottlrWindow window = await GetWindowAsync(targetItem);

            // Check if there are any rules to be applied.
            ThrottlrRuleCache.Default.TryGetRule(_throttlrConfiguration, targetItem, out IThrottlrRule? rule);

            // Create a new result object.
            ThrottlResult<T> result = new(targetItem, windowKey, window, actionData, rule);

            // Handle the result.
            await ProcessThrottleResult(result, onSuccessAction, onFailureAction);

            // Set the item value in the MemoryCache.
            _inMemoryCache[windowKey] = new MemoryCacheItem<IThrottlrWindow>(result.Window, DateTime.UtcNow.Add(_throttlrConfiguration.TimeWindow));

            // Return the result.
            return result;
        }

        /// <summary>
        /// Gets a <see cref="IThrottlrWindow"/> from the cache. If redis is not available, it will use the in-memory cache.
        /// If no window is found, it will create a new one.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<IThrottlrWindow> GetWindowAsync(T item) => await GetWindowAsync(item, null);

        /// <summary>
        /// Gets a <see cref="IThrottlrWindow"/> from the cache. If redis is not available, it will use the in-memory cache.
        /// If no window is found, it will create a new one.
        /// </summary>
        /// <remarks>
        /// If window creation is required and <paramref name="preSeed"/> is not null, it will be used to pre-seed the window.
        /// See <see cref="CreateWindow(IEnumerable{ActionData}?)"/> for additional information.
        /// </remarks>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<IThrottlrWindow> GetWindowAsync(T item, IEnumerable<ActionData>? preSeed = null)
        {
            string windowKey = _throttlrConfiguration.BuildKey(item);

            IThrottlrWindow? window = null;
            bool isNewWindow = true;

            if (_redisConnection.IsConnected)
            {
                try
                {
                    string? windowString = await _redisConnection.GetDatabase().StringGetAsync(windowKey);

                    if (!string.IsNullOrEmpty(windowString))
                    {
                        IThrottlrWindow currentWindow = IThrottlrWindow.Deserialize(windowString ?? string.Empty, WindowType, _throttlrConfiguration.JsonSerializerOptions);

                        // It needs to match our configuration.
                        if (currentWindow.MatchesConfiguration(_throttlrConfiguration))
                        {
                            window = currentWindow;
                            isNewWindow = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to get {window} from {client}.", typeof(IThrottlrWindow).Name, _redisConnection.ClientName);
                }
            }

            if (window is null)
            {
                if (_inMemoryCache.TryGetValue(windowKey, out IMemoryCacheItem<IThrottlrWindow>? throttlrWindow) && throttlrWindow is not null)
                {
                    _logger?.LogWarning("{window} found in MemoryCache.", nameof(IThrottlrWindow));
                    window = throttlrWindow.Item;
                }
                else
                {
                    window = CreateWindow(preSeed);
                }
            }

            _inMemoryCache[windowKey] = new MemoryCacheItem<IThrottlrWindow>(window, DateTime.UtcNow.Add(_throttlrConfiguration.TimeWindow));

            if (isNewWindow && _redisConnection.IsConnected)
            {
                await _redisConnection.GetDatabase().StringSetAsync(windowKey, window.Serialize(_throttlrConfiguration.JsonSerializerOptions));
            }

            return window;
        }

        /// <summary>
        /// Creates a new instance of the window type.
        /// </summary>
        /// <returns></returns>
        public IThrottlrWindow CreateWindow() => CreateWindow(null);

        /// <summary>
        /// Creates a new instance of the window type.
        /// </summary>
        /// <remarks>
        /// Allows for pre-seeding of the window with the specified <see cref="ActionData"/> collection.
        /// The provided <see cref="ActionData"/> are enqueued in the order they are provided.
        /// </remarks>
        /// <param name="preSeed"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IThrottlrWindow CreateWindow(IEnumerable<ActionData>? preSeed = null)
        {
            // Create a new instance of the window type.
            object? windowInstance = Activator.CreateInstance(WindowType,
                                                              _throttlrConfiguration.Name,
                                                              _throttlrConfiguration.Maximum,
                                                              _throttlrConfiguration.TimeWindow) ??
                throw new InvalidOperationException($"Failed to create instance of {WindowType}.");

            // Cast it to the interface.
            IThrottlrWindow window = windowInstance as IThrottlrWindow ??
                throw new InvalidOperationException($"Failed to cast {nameof(windowInstance)} to {WindowType.Name}.");

            // Pre-Seed if we need to.
            if (preSeed is not null)
            {
                window.AllowedActions.Clear();
                preSeed.ToList().ForEach(window.AllowedActions.Enqueue);
            }

            return window;
        }

        /// <summary>
        /// Handles the success or failure scenario.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="targetItem"></param>
        /// <param name="redisDatabase"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private async Task ProcessThrottleResult(ThrottlResult<T> result,
                                                 Func<ThrottlResult<T>, Task<bool>> onSuccess,
                                                 Func<ThrottlResult<T>, Task<bool>> onFailure)
        {
            // Execute the appropriate delegate.
            bool resultActionExecuted = result.IsAllowed switch
            {
                true => await onSuccess(result),
                false => await onFailure(result)
            };

            // Handle the delegate execution failure scenario.
            if (!resultActionExecuted)
            {
                InvalidOperationException ex = new($"Execution of the specified {(result.IsAllowed ? nameof(_throttlrConfiguration.OnSuccess) : nameof(_throttlrConfiguration.OnFailure))} delegate failed");
                _logger?.LogError(ex, "{message}", ex.Message);

                // We won't throw here, the user should handle this in their delegate(?)
            }

            try
            {
                // Is Redis available?
                if (_redisConnection.IsConnected)
                {
                    // Attempt to cache the window.
                    string windowString = result.Window.Serialize(_throttlrConfiguration.JsonSerializerOptions);
                    await _redisConnection.GetDatabase().StringSetAsync(result.Key, windowString, _throttlrConfiguration.TimeWindow);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to set {window} in {client}.", typeof(IThrottlrWindow).Name, _redisConnection.ClientName);
            }

            // If we have no Redis connection we fall back to the memory cache.
            _logger?.LogWarning("Setting {window} in MemoryCache.", nameof(IThrottlrWindow));
            _inMemoryCache[result.Key] = new MemoryCacheItem<IThrottlrWindow>(result.Window, DateTime.UtcNow.Add(_throttlrConfiguration.TimeWindow));
        }

        /// <summary>
        /// Initializes the memory cache cleanup timer.
        /// </summary>
        private void InitializeMemoryCache()
        {
            _inMemoryCache = new ConcurrentDictionary<string, IMemoryCacheItem<IThrottlrWindow>>();

            if (_memoryCacheConfiguration.CleanupInterval == TimeSpan.Zero)
            {
                _logger?.LogWarning("MemoryCache cleanup is disabled.");
                return;
            }

            _logger?.LogInformation("MemoryCache cleanup is enabled.");

            _memoryCacheCleanupTimer = new Timer(_ => CleanMemoryCache(), null, _memoryCacheConfiguration.CleanupInterval, _memoryCacheConfiguration.CleanupInterval);
        }

        /// <summary>
        /// Cleans up expired <see cref="MemoryCacheItem{T}"/>.
        /// </summary>
        private void CleanMemoryCache()
        {
            IList<string> keysToRemove = _inMemoryCache.Where(item => item.Value.IsExpired).Select(item => item.Key).ToList();
            foreach (var key in keysToRemove) _inMemoryCache.TryRemove(key, out _);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _memoryCacheCleanupTimer?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
