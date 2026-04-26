# ADNC ID Generator: Snowflake Algorithm

[GitHub Repository](https://github.com/alphayu/adnc)

Primary keys can be generated using various methods: auto-incrementing IDs, GUIDs, Redis `INCR`, or the Snowflake algorithm. ADNC adopts the Snowflake algorithm based on [Yitter](https://github.com/yitter/IdGenerator) to balance uniqueness and high performance in distributed environments.

## Introduction to Yitter Snowflake

Traditional Snowflake IDs use 64 bits (1 sign bit, 41 timestamp bits, 10 machine ID bits, and 12 sequence bits). A major drawback is that these IDs exceed the maximum safe integer for JavaScript (`Number.MAX_SAFE_INTEGER`), leading to parsing errors in the frontend.

Yitter optimizes the traditional algorithm by allowing customizable bit lengths for the machine ID and sequence. In the default configuration (6 bits each), IDs generated within 50 years will not exceed the JS safe integer limit.

### Key Features of Yitter

- **Short & JS-Friendly**: Integer IDs that are shorter and safe for JS `Number` types for over 50 years.
- **High Performance**: 2-5 times faster than traditional Snowflake; can generate 500k IDs in 0.1s.
- **Clock Backwards Handling**: Automatically adapts if the system clock drifts backwards (e.g., by 1 second).
- **History Insertion**: Supports generating unique IDs for historical timestamps.
- **Zero External Dependencies**: Does not require external caches or databases (except for dynamic `WorkerId` registration in K8s).

### Handling Clock Backwards

1. Uses reserved sequences from historical timeframes when a clock drift is detected.
2. Supports adapting to drift up to a configurable threshold.

## Configuring Yitter Snowflake

Three core parameters are required:

| Parameter | Description |
| --- | --- |
| `WorkerIdBitLength` | Bits for machine ID (default: 6, supporting 64 instances). |
| `SeqBitLength` | Bits for sequence (default: 6, determines IDs per ms). |
| `WorkerId` | The unique ID of the machine/instance. |

The sum of `WorkerIdBitLength` and `SeqBitLength` must not exceed 22.

### Dynamic WorkerId Allocation

In distributed microservices, `WorkerId` must be unique across instances. ADNC manages this by storing all possible `WorkerId`s in a Redis `zset` (value = WorkerId, score = timestamp).

1. On startup, the instance uses a Lua script to grab the `WorkerId` with the smallest score from Redis.
2. The score is updated to the current timestamp.
3. A background service (`WorkerNodeHostedService`) refreshes the score every minute.
4. When the instance stops, it resets the score (effectively releasing the `WorkerId`).

## Usage Example

```csharp
using Adnc.Infra.IdGenerater.Yitter;

namespace Adnc.XXX.Application.Services
{
    public class xxxAppService : AbstractAppService
    {
        public void CreateEntity()
        {
            var id = IdGenerater.GetNextId(); // Generate Snowflake ID
        }
    }
}
```

---
*If this helps, please Star & Fork.*
