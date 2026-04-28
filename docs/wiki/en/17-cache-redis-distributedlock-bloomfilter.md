# ADNC Cache, Redis, Distributed Lock, and Bloom Filter Usage

[GitHub repository](https://github.com/alphayu/adnc)

.NET officially provides the `Microsoft.Extensions.Caching.Distributed.IDistributedCache` interface and implementations such as `StackExchange.Redis`. So why does this project add another abstraction layer?

There are two main reasons:

1. The `IDistributedCache` method signatures are relatively generic and often cannot directly cover project requirements in real production environments. Even when used directly, a secondary wrapper is usually still needed.
2. Projects often need other Redis data structures and capabilities. Whether using StackExchange.Redis or CSRedis, unified encapsulation is usually required to reduce coupling.

Therefore, `Adnc.Infra.Caching` is encapsulated based on StackExchange.Redis to manage and use cache, Redis, and distributed locks.

> `Adnc.Infra.Caching` reused and simplified a lot of code from `EasyCaching`, while improving several core capabilities based on `EasyCaching`.

```csharp
//Microsoft.Extensions.Caching.Distributed.IDistributedCache
byte[] Get(string key);
Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken));
void Set(string key, byte[] value, DistributedCacheEntryOptions options);
Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token);
void Refresh(string key);
Task RefreshAsync(string key, CancellationToken token = default(CancellationToken));
void Remove(string key);
Task RemoveAsync(string key, CancellationToken token = default(CancellationToken));
```
## Overall Design

`Adnc.Infra.Redis` / `Adnc.Infra.Redis.Caching` provides three interfaces and two cache interceptors.

#### Interfaces
- IDistributedLocker distributed lock
- IRedisProvider Redis operation interface
- ICacheProvider Cache operation interface

#### Cache Interceptors
- CachingEvictAttribute remove interceptor
- CachingAbleAttribute read interceptor

## appsettings.json configuration example

```json
"Redis": {
    // Cache avalanche prevention. When saving cache, the expiration time adds a random number of seconds no greater than MaxRdSecond.
    "MaxRdSecond": 30,
    // Cache breakdown prevention. LockMs is the lock duration after acquiring the distributed lock; SleepMs is the sleep duration when the lock is not acquired. See the source code for the specific implementation.
    "LockMs": 6000,
    "SleepMs": 300,
    // Cache serialization configuration. Currently only binary is implemented; you can extend it yourself.
    "SerializerName": "binary",
    // Whether cache logging is enabled.
    "EnableLogging": true,
    // Polly timeout. This parameter is used by the cache/database synchronization compensation mechanism. See the source code for the specific implementation.
    "PollyTimeoutSeconds": 11,
    // Cache penetration prevention configuration.
    "PenetrationSetting": {
        // Disable=true allows penetration.
        // Disable=false does not allow penetration.
        "Disable": false,
        // Bloom filter configuration. Cache penetration prevention is implemented through Bloom filters.
        "BloomFilterSetting": {
            // Filter name.
            "Name": "adnc:usr:bloomfilter:cachekeys",
            // Size.
            "Capacity": 10000000,
            // Error rate.
            "ErrorRate": 0.001
            }
    },
    // Redis connection string.
    "Dbconfig": {
        "ConnectionString": "127.0.0.1:13379,password=football,defaultDatabase=11,ssl=false,sslHost=null,connectTimeout=4000,allowAdmin=true"
    }
}
```
# IDistributedLocker
This interface provides a safe distributed lock. The lock is released through Lua script + version number (lockvalue), and the automatic renewal function is implemented.```csharp
public interface IDistributedLocker
{
    /// <summary>
    /// Acquires a distributed lock.
    /// </summary>
    /// <param name="cacheKey">cacheKey.</param>
    /// <param name="timeoutSeconds">The lock duration.</param>
    /// <param name="autoDelay">Whether to renew the lock automatically.</param>
    /// <returns>Success indicates the lock acquisition status, and LockValue is the lock version.</returns>
    Task<(bool Success, string LockValue)> LockAsync(string cacheKey, int timeoutSeconds = 5, bool autoDelay = true);

    /// <summary>
    /// Releases the lock safely.
    /// </summary>
    /// <param name="cacheKey">cacheKey.</param>
    /// <param name="cacheValue">The version.</param>
    /// <returns></returns>
    Task<bool> SafedUnLockAsync(string cacheKey, string cacheValue);
    
    /// <summary>
    /// Acquires a distributed lock.
    /// </summary>
    /// <param name="cacheKey">cacheKey.</param>
    /// <param name="timeoutSeconds">The lock duration.</param>
    /// <param name="autoDelay">Whether to renew the lock automatically.</param>
    /// <returns>Success indicates the lock acquisition status, and LockValue is the lock version.</returns>
    (bool Success, string LockValue) Lock(string cacheKey, int timeoutSeconds = 5, bool autoDelay = true);
    
    /// <summary>
    /// Releases the lock safely.
    /// </summary>
    /// <param name="cacheKey">cacheKey.</param>
    /// <param name="cacheValue">The version.</param>
    /// <returns></returns>
    bool SafedUnLock(string cacheKey, string cacheValue);
}
```
## IDistributedLocker usage
Where distributed locks need to be used, they are injected through the constructor.

```csharp
public class xxxAppService
{
    private readonly IDistributedLocker _locker;
    public xxxAppService(IDistributedLocker locker)
    {
        _locker = locker;
    }
    
    public void Test()
    {
        var cacheKey = "adnc:menus";
        var flag = _locker.Lock(cacheKey);
        if(!flag.Success)
        {
            // Failed to acquire the lock.
            // Your business logic.
            return;
        }

        // Lock acquired successfully.
        try
        {
            // Your business logic.
        }
        catch(Exception ex)
        {
            throw new Exception(ex.Message,ex);
        }
        finally
        {
            // Always release the lock.
            _locker.SafedUnLock(cacheKey,flag.LockValue);
        }
    }
}
```
# IRedisProvider
This interface is implemented based on StackExchange.Redis by default, and this interface provides operation methods for all data types of Redis. IRedisProvidert provides too many methods, so I won’t put all the code here. Post a few methods of bloom interceptor.

```csharp
public interface IRedisProvide
{
    // Serialization method. Binary is implemented by default; you can extend it yourself.
    ICachingSerializer Serializer { get; }
   
    // Other methods.

    #region Bloom Filter
    /// <summary>
    /// Creates an empty Bloom Filter with a single sub-filter for the initial capacity requested and with an upper bound error_rate . 
    /// </summary>
    /// <param name="key"></param>
    /// <param name="errorRate"></param>
    /// <param name="initialCapacity"></param>
    /// <returns></returns>
    Task BloomReserveAsync(string key, double errorRate, int initialCapacity);

    /// <summary>
    /// Adds an item to the Bloom Filter, creating the filter if it does not yet exist.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<bool> BloomAddAsync(string key, string value);

    /// <summary>
    /// Adds one or more items to the Bloom Filter and creates the filter if it does not exist yet. 
    /// This command operates identically to BF.ADD except that it allows multiple inputs and returns multiple values.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    Task<bool[]> BloomAddAsync(string key, IEnumerable<string> values);

    /// <summary>
    /// Determines whether an item may exist in the Bloom Filter or not.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<bool> BloomExistsAsync(string key, string value);

    /// <summary>
    /// Determines if one or more items may exist in the filter or not.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    Task<bool[]> BloomExistsAsync(string key, IEnumerable<string> values);
    #endregions

}     
```
## IRedisProvider usage
Inject through the constructor where redis is needed. Everyone should be familiar with the basic commonly used methods. The following example code demonstrates a more complicated one, how to execute a Lua script.

```csharp
public class xxxAppService
{
    private readonly IRedisProvider _redis;
    public xxxAppService(IRedisProvider redis)
    {
        _redis = redis;
    }
    
    public void Test()
    {
        var cacheKey = "adnc:workerid"

        var scirpt = @"local workerids = redis.call('ZRANGE', @key, @start,@stop)
        redis.call('ZADD',@key,@score,workerids[1])
        return workerids[1]";

        var parameters = new { key = cacheKey, start = 0, stop = 0, score = DateTime.Now.GetTotalMilliseconds() };
        // Execute the Lua script.
        var luaResult = (byte[]) await _redis.ScriptEvaluateAsync(scirpt, parameters);
        var workerId = _redis.Serializer.Deserialize<long>(luaResult);        
    }
}
```
# ICacheProvider
This interface is implemented based on StackExchange.Redis by default and solves cache avalanche/breakdown/penetration/synchronization problems. This interface provides rich cache-related operation methods.

```csharp
public interface ICacheProvider
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name.</value>
    string Name { get; }

    CacheOptions CacheOptions { get; }

    /// <summary>
    /// The serializer.
    /// </summary>
    ICachingSerializer Serializer { get; }

    /// <summary>
    /// Set the specified cacheKey, cacheValue and expiration.
    /// </summary>
    /// <param name="cacheKey">Cache key.</param>
    /// <param name="cacheValue">Cache value.</param>
    /// <param name="expiration">Expiration.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    void Set<T>(string cacheKey, T cacheValue, TimeSpan expiration);

    /// <summary>
    /// Sets the specified cacheKey, cacheValue and expiration async.
    /// </summary>
    /// <returns>The async.</returns>
    /// <param name="cacheKey">Cache key.</param>
    /// <param name="cacheValue">Cache value.</param>
    /// <param name="expiration">Expiration.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    Task SetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);

    /// <summary>
    /// Get the specified cacheKey.
    /// </summary>
    /// <returns>The get.</returns>
    /// <param name="cacheKey">Cache key.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    CacheValue<T> Get<T>(string cacheKey);

    /// <summary>
    /// Get the specified cacheKey async.
    /// </summary>
    /// <returns>The async.</returns>
    /// <param name="cacheKey">Cache key.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    Task<CacheValue<T>> GetAsync<T>(string cacheKey);

    /// <summary>
    /// Remove the specified cacheKey.
    /// </summary>
    /// <param name="cacheKey">Cache key.</param>
    void Remove(string cacheKey);

    /// <summary>
    /// Remove the specified cacheKey async.
    /// </summary>
    /// <returns>The async.</returns>
    /// <param name="cacheKey">Cache key.</param>
    Task RemoveAsync(string cacheKey);

    /// <summary>
    /// Exists the specified cacheKey async.
    /// </summary>
    /// <returns>The async.</returns>
    /// <param name="cacheKey">Cache key.</param>
    Task<bool> ExistsAsync(string cacheKey);

    /// <summary>
    /// Exists the specified cacheKey.
    /// </summary>
    /// <returns>The exists.</returns>
    /// <param name="cacheKey">Cache key.</param>
    bool Exists(string cacheKey);

    /// <summary>
    /// Tries the set.
    /// </summary>
    /// <returns><c>true</c>, if set was tryed, <c>false</c> otherwise.</returns>
    /// <param name="cacheKey">Cache key.</param>
    /// <param name="cacheValue">Cache value.</param>
    /// <param name="expiration">Expiration.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    bool TrySet<T>(string cacheKey, T cacheValue, TimeSpan expiration);

    /// <summary>
    /// Tries the set async.
    /// </summary>
    /// <returns>The set async.</returns>
    /// <param name="cacheKey">Cache key.</param>
    /// <param name="cacheValue">Cache value.</param>
    /// <param name="expiration">Expiration.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    Task<bool> TrySetAsync<T>(string cacheKey, T cacheValue, TimeSpan expiration);

    /// <summary>
    /// Sets all.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <param name="expiration">Expiration.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    void SetAll<T>(IDictionary<string, T> value, TimeSpan expiration);

    /// <summary>
    /// Sets all async.
    /// </summary>
    /// <returns>The all async.</returns>
    /// <param name="value">Value.</param>
    /// <param name="expiration">Expiration.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    Task SetAllAsync<T>(IDictionary<string, T> value, TimeSpan expiration);

    /// <summary>
    /// Removes all.
    /// </summary>
    /// <param name="cacheKeys">Cache keys.</param>
    void RemoveAll(IEnumerable<string> cacheKeys);

    /// <summary>
    /// Removes all async.
    /// </summary>
    /// <returns>The all async.</returns>
    /// <param name="cacheKeys">Cache keys.</param>
    Task RemoveAllAsync(IEnumerable<string> cacheKeys);

    /// <summary>
    /// Get the specified cacheKey, dataRetriever and expiration.
    /// </summary>
    /// <returns>The get.</returns>
    /// <param name="cacheKey">Cache key.</param>
    /// <param name="dataRetriever">Data retriever.</param>
    /// <param name="expiration">Expiration.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    CacheValue<T> Get<T>(string cacheKey, Func<T> dataRetriever, TimeSpan expiration);

    /// <summary>
    /// Gets the specified cacheKey, dataRetriever and expiration async.
    /// </summary>
    /// <returns>The async.</returns>
    /// <param name="cacheKey">Cache key.</param>
    /// <param name="dataRetriever">Data retriever.</param>
    /// <param name="expiration">Expiration.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    Task<CacheValue<T>> GetAsync<T>(string cacheKey, Func<Task<T>> dataRetriever, TimeSpan expiration);

    /// <summary>
    /// Removes cached item by cachekey's prefix.
    /// </summary>
    /// <param name="prefix">Prefix of CacheKey.</param>
    void RemoveByPrefix(string prefix);

    /// <summary>
    /// Removes cached item by cachekey's prefix async.
    /// </summary>
    /// <param name="prefix">Prefix of CacheKey.</param>
    Task RemoveByPrefixAsync(string prefix);

    /// <summary>
    /// Gets the specified cacheKey async.
    /// </summary>
    /// <returns>The async.</returns>
    /// <param name="cacheKey">Cache key.</param>
    /// <param name="type">Object Type.</param>
    Task<object> GetAsync(string cacheKey, Type type);

    /// <summary>
    /// Set the keys  TTL
    /// </summary>
    /// <param name="cacheKeys">Cache keys.</param>
    /// <param name="seconds">Expiration .</param>
    /// <returns></returns>
    Task KeyExpireAsync(IEnumerable<string> cacheKeys, int seconds);
}
```
## ICacheProvider usage
Where cache needs to be used,`ICacheProvider`can be injected through the constructor. In ADNC, cache access is usually uniformly encapsulated through`CacheService`; you can also directly inject and call it in`XXXAppService`.

It is recommended to first operate the cache in`CacheService`mode to centrally manage cross-cutting capabilities such as cache key specifications, expiration policies, Bloom filters, and distributed locks.- use directly

```csharp
public class xxxAppService
{
    private readonly ICacheProvider _cache;
    public xxxAppService(ICacheProvider cache)
    {
        _cache = cache;
    }

    public void Test()
    {
        var cacheKey ="adnc:userinfo:account";
        var value = "alpha2008";
        await _cache.Value.Set(cacheKey, value, TimeSpan.FromSeconds(CachingConsts.OneDay));
    }
}
```
- Inject CacheService in xxxAppService.cs.

```csharp
public class CacheService : AbstractCacheService
{
    private readonly Lazy<ICacheProvider> _cache;

    public CacheService(Lazy<ICacheProvider> cache
    , Lazy<IRedisProvider> redisProvider
    : base(cache, redisProvider, distributedLocker)
    {
    _cache = cache;
    }

    // Cache warm-up method. The project calls this method during startup.
    public override async Task PreheatAsync()
    {
        // Other cache entries that need to be warmed up.
        await GetAllDeptsFromCacheAsync();
        // Other cache entries that need to be warmed up.
    }

    // Bloom filter.
    internal (IBloomFilter CacheKeys, IBloomFilter Accounts) BloomFilters
    {
        get
        {
            var cacheFilter = _bloomFilterFactory.Value.GetBloomFilter(_cache.Value.CacheOptions.PenetrationSetting.BloomFilterSetting.Name);
            var accountFilter = _bloomFilterFactory.Value.GetBloomFilter($"adnc:{nameof(BloomFilterAccount).ToLower()}");
            return (cacheFilter, accountFilter);
        }
    }
}
```
# Cache interceptor usage
Interceptors should be used according to actual business scenarios. Interceptors cannot solve all problems. When the interceptor cannot solve the problem, the cache can only be used through coding in the business logic.
### CachingEvictAttribute Delete the interceptor and use the CachingEvictAttribute attribute<br/>in the IxxxappService interface| Parameters | Description (the following parameters can be used in combination) |
| 

--------------

 | 

---------------------------------------------------------------

 |
| CacheKey | Delete the specified CacheKey |
| CacheKeys | Delete a set of CacheKeys |
| CacheKeyPrefix | To delete a group of CacheKeys containing the prefix, you need to cooperate with the parameter attribute CachingParamAttribute |

#### Example code

```csharp
public interface IUserAppService : IAppService
{
    [CachingEvict(
        CacheKeys = new[] { CachingConsts.MenuRelationCacheKey, CachingConsts.MenuCodesCacheKey 
        }
        , CacheKeyPrefix = CachingConsts.UserValidateInfoKeyPrefix)]
    Task<AppSrvResult> SetRoleAsync([CachingParam] long id,UserSetRoleDto input);  

    [CachingEvict(CacheKeyPrefix = CachingConsts.UserValidateInfoKeyPrefix)]
    Task<AppSrvResult> ChangeStatusAsync([CachingParam] IEnumerable<long> ids, int status);
}

```
### CachingAbleAttribute read interceptor
| Parameters | Description |
| 

--------------

 | 

---------------------------------------------------------------

 |
| CacheKey | Read the specified CacheKey |
| CacheKeyPrefix | To read the Cachkey of the specified prefix, you need to match the parameter attribute CachingParamAttribute |
| Expiration | Expiration time |

#### Example code

```csharp
public interface IDeptAppService : IAppService
{
    [CachingAble(CacheKey = CachingConsts.DetpTreeListCacheKey, Expiration = CachingConsts.OneYear)]
    Task<List<DeptTreeDto>> GetTreeListAsync();
}

public interface IAccountAppService : IAppService
{
    [CachingAble(CacheKeyPrefix = CachingConsts.UserValidateInfoKeyPrefix)]
    Task<UserValidateDto> GetUserValidateInfoAsync([CachingParam] long id);
}
```

# Use of Cachekey bloom filter`Adnc.Infra.Caching`anti-penetration is achieved through Bloom filter. The following describes how to define and use it in the project. I take the Admin microservice as an example.- first step
Create a new BloomFilterCacheKey.cs in the Caching directory of the Adnc.Admin.Application project and inherit AbstractBloomFilter
- Step 2
Override the InitAsync method, which is responsible for initializing the Bloom filter. When the project starts, this method will be automatically called to save the cachekey used in the system into the filter.By default, InitAsync will only be executed once. After the Bloom filter is successfully created, it will not be called when the project is started again. You need to adjust it according to your actual situation.

```csharp
namespace Adnc.Usr.Application.Caching
{
    public class BloomFilterCacheKey : AbstractBloomFilter
    {
        private readonly Lazy<ICacheProvider> _cache;
        private readonly Lazy<IDistributedLocker> _distributedLocker;
        private readonly Lazy<IServiceProvider> _services;

        public BloomFilterCacheKey(Lazy<ICacheProvider> cache
            , Lazy<IRedisProvider> redisProvider
            , Lazy<IDistributedLocker> distributedLocker
            , Lazy<IServiceProvider> services)
            : base(cache, redisProvider, distributedLocker)
        {
            _cache = cache;
            _distributedLocker = distributedLocker;
            _services = services;

            // Automatically get filter configuration from appsettings.xxx.json.
            var setting = cache.Value.CacheOptions.PenetrationSetting.BloomFilterSetting;
            Name = setting.Name;
            ErrorRate = setting.ErrorRate;
            Capacity = setting.Capacity;
        }

        // Filter name, corresponding to the key in Redis.
        public override string Name { get; }
        // Error rate.
        public override double ErrorRate { get; }
        // Capacity.
        public override int Capacity { get; }

        public override async Task InitAsync()
        {
            // Call the base class method to initialize the filter.
            await base.InitAsync(async () =>
            {

                var values = new List<string>()
                {
                    // These are fixed cache keys.
                     CachingConsts.MenuListCacheKey
                    ,CachingConsts.MenuTreeListCacheKey
                    ,CachingConsts.MenuRelationCacheKey
                    ,CachingConsts.MenuCodesCacheKey
                    ,CachingConsts.DetpListCacheKey
                    ,CachingConsts.DetpTreeListCacheKey
                    ,CachingConsts.DetpSimpleTreeListCacheKey
                    ,CachingConsts.RoleListCacheKey
                };

                var ids = new List<long>();
                using (var scope = _services.Value.CreateScope())
                {
                    var repository = scope.ServiceProvider.GetRequiredService<IEfRepository<SysUser>>();
                    ids = await repository.GetAll().Select(x => x.Id).ToListAsync();
                }

                // These are not fixed cache keys; only the prefix is the same.
                //public const string UserValidateInfoKeyPrefix = "adnc:usr:users:validateinfo";
                // For example, after account alpha2008 logs in, its status information is stored in cache with a cache key such as adnc:usr:users:validateinfo:

160000000000.
                if (ids?.Any() == true)
                    values.AddRange(ids.Select(x => string.Concat(CachingConsts.UserValidateInfoKeyPrefix, CachingConsts.LinkChar, x)));

                return values;
            });
        }
    }
}

```

- The third step is to combine BloomFilterCacheKey in cacheservice.cs (mainly for convenience of calling).

```csharp
namespace Adnc.Usr.Application.Caching
{
    public class CacheService : AbstractCacheService
    {
        private readonly Lazy<IBloomFilterFactory> _bloomFilterFactory;
        public CacheService(Lazy<IBloomFilterFactory> bloomFilterFactory)
        {
            _bloomFilterFactory = bloomFilterFactory;
        }
        public override async Task PreheatAsync(){};

        internal (IBloomFilter CacheKeys, IBloomFilter Accounts) BloomFilters
        {
            get
            {
                // Cache key Bloom filter.
                var cacheFilter = _bloomFilterFactory.Value.GetBloomFilter(_cache.Value.CacheOptions.PenetrationSetting.BloomFilterSetting.Name);
                // Account Bloom filter. This is another filter and is unrelated to cache.
                var accountFilter = _bloomFilterFactory.Value.GetBloomFilter($"adnc:{nameof(BloomFilterAccount).ToLower()}");
                return (cacheFilter, accountFilter);
            }
        }
    }
}
```

- Step 4
Dynamically add cachekey to BloomFilterCacheKey filter

```csharp
public class UserAppService : AbstractAppService, IUserAppService
{
    private readonly IEfRepository<SysUser> _userRepository;
    private readonly CacheService _cacheService;

    public async Task<AppSrvResult<long>> CreateAsync(UserCreationDto input)
    {
        // Other business logic.
        user.Id = IdGenerater.GetNextId();

        var cacheKey = _cacheService.ConcatCacheKey(CachingConsts.UserValidateInfoKeyPrefix, user.Id);
        // Add to the BloomFiltersCacheKey filter.
        await _cacheService.BloomFilters.CacheKeys.AddAsync(cacheKey);
        
        await _userRepository.InsertAsync(user);
        // Other business logic.
    }
}
```

# Other bloom filters
In the previous section we saw that there is another accountFilter filter (BloomFilterAccount), which has nothing to do with cache. Please refer to the source code for specific implementation. The definitions of all Bloom filters are the same.BloomFilterAccount is used to determine whether the account exists when logging in.

```csharp
namespace Adnc.Usr.Application.Services
{
    public class AccountAppService : AbstractAppService, IAccountAppService
    {
        private readonly CacheService _cacheService;

        public AccountAppService(CacheService cacheService)
        {
            _cacheService = cacheService;
        }
        
        public async Task<AppSrvResult<UserValidateDto>> LoginAsync(UserLoginDto inputDto)
        {
            // The BloomFilterAccount filter is used here.
            // LoginAsync is currently the only ADNC method that can be accessed without logging in. Normally, checking directly through the database is fine.
            // If malicious users simulate thousands of concurrent calls to this method, the current database configuration will not be able to handle it.
            // If requests are checked by a Bloom filter first, thousands of concurrent requests will not put much pressure on Redis.
            var exists = await _cacheService.BloomFilters.Accounts.ExistsAsync(inputDto.Account.ToLower());
            if(!exists)
                return Problem(HttpStatusCode.BadRequest, "Invalid username or password");

            var user = await _userRepository.FetchAsync(x => x.Account == inputDto.Account);
            if (user == null)
                return Problem(HttpStatusCode.BadRequest, "Invalid username or password");
        }
    }
}
```

# Other application scenarios of Bloom filter
- User registration, real-time determination of whether there is an account with the same name.
- Duplicate data judgment.I use Bloom filters to handle both of the above business scenarios.

## Explore more elegant implementation methods
- Database and cache synchronization
- Bloom filter adds data in real time
- Database synchronization with ES
- Database synchronization with MQ.If your business scenario requires implementing the above functions, Alibaba's open source canal may be a better choice. Using canal can organize business code elegantly and conveniently.

---
If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
