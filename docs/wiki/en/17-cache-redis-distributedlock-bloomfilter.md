# ADNC Cache: Redis, Distributed Locks, and Bloom Filters

[GitHub Repository](https://github.com/alphayu/adnc)

While .NET provides `IDistributedCache`, it is often too generic for real-world production needs. Projects frequently require access to specific Redis data structures, distributed locks, and protection against cache stampede, breakdown, and penetration. `Adnc.Infra.Caching` (based on `StackExchange.Redis`) provides a unified wrapper for these capabilities.

## Core Design

`Adnc.Infra.Caching` offers three main interfaces and AOP-based interceptors.

### Interfaces
- `IDistributedLocker`: For safe distributed locking with auto-renewal.
- `IRedisProvider`: Direct Redis operations for all data types.
- `ICacheProvider`: High-level cache management with built-in protection logic.

### AOP Interceptors
- `CachingEvictAttribute`: For cache invalidation.
- `CachingAbleAttribute`: For read-through caching.

## Configuration (appsettings.json)

```json
"Redis": {
    "MaxRdSecond": 30, // Prevents cache stampede (randomized expiration)
    "LockMs": 6000,    // Lock duration for preventing cache breakdown
    "SleepMs": 300,    // Retry interval if lock is held
    "SerializerName": "binary",
    "EnableLogging": true,
    "PenetrationSetting": {
        "Disable": false,
        "BloomFilterSetting": {
            "Name": "adnc:usr:bloomfilter:cachekeys",
            "Capacity": 10000000,
            "ErrorRate": 0.001
        }
    },
    "Dbconfig": {
        "ConnectionString": "127.0.0.1:6379,password=pwd,defaultDatabase=0"
    }
}
```

## Distributed Locking (`IDistributedLocker`)

Provides a safe lock released via Lua scripts with versioning (`LockValue`) and supports automatic renewal (watchdog).

```csharp
public async Task ProcessTask()
{
    var lockResult = await _locker.LockAsync("lock_key");
    if (!lockResult.Success) return;

    try {
        // Business Logic
    }
    finally {
        await _locker.SafedUnLockAsync("lock_key", lockResult.LockValue);
    }
}
```

## Redis Operations (`IRedisProvider`)

Supports all native Redis data types and Bloom Filters.

```csharp
// Executing Lua Scripts
var script = "return redis.call('GET', @key)";
var result = await _redis.ScriptEvaluateAsync(script, new { key = "my_key" });
```

## Cache Provider (`ICacheProvider`)

The preferred way to access cache. It handles common caching pitfalls (stampede, breakdown, penetration) internally.

```csharp
// Direct Usage
await _cache.SetAsync("key", value, TimeSpan.FromHours(1));

// Using CacheService (Recommended)
// Wrapping cache access logic to ensure consistent naming and policies.
```

## Using Cache Interceptors

Interceptors simplify caching logic using attributes on service interfaces.

### `CachingEvict` (Invalidation)
```csharp
[CachingEvict(CacheKeyPrefix = "user_info:")]
Task<AppSrvResult> UpdateUserAsync([CachingParam] long id, ...);
```

### `CachingAble` (Read-through)
```csharp
[CachingAble(CacheKeyPrefix = "user_info:", Expiration = 3600)]
Task<UserDto> GetUserAsync([CachingParam] long id);
```

## Cache Penetration & Bloom Filters

ADNC prevents cache penetration (requesting non-existent data) using Bloom Filters.

1. **Define a Bloom Filter**: Inherit from `AbstractBloomFilter` and override `InitAsync`.
2. **Initialization**: On startup, populate the filter with existing keys (e.g., from DB).
3. **Usage**:
   - When creating an entity, add its key to the Bloom Filter.
   - When querying, the `ICacheProvider` or interceptor checks the Bloom Filter first.

```csharp
public class BloomFilterCacheKey : AbstractBloomFilter
{
    public override async Task InitAsync()
    {
        await base.InitAsync(async () => {
            return await _repo.GetAll().Select(x => x.Id.ToString()).ToListAsync();
        });
    }
}
```

## Summary

By combining `ICacheProvider`, `IDistributedLocker`, and Bloom Filters, ADNC builds a robust caching architecture that is resilient to high concurrency and protects the underlying database.

---
*If this helps, please Star & Fork.*
