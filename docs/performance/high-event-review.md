# High-Event Home Assistant Performance Review

This review is scoped to realistic high-event Home Assistant workloads: websocket event ingest, command/result latency, state cache updates, Rx fan-out, and service-call behavior. It intentionally ignores changes that only improve cold paths or trim tiny CPU costs that disappear behind network latency.

## How To Reproduce The Measurements

Use the isolated benchmark project:

```bash
dotnet run -c Release --project benchmarks/NetDaemon.PerformanceBenchmarks -- --filter '*'
```

Useful focused runs:

```bash
dotnet run -c Release --project benchmarks/NetDaemon.PerformanceBenchmarks -- --filter '*WebSocketPipeline*'
dotnet run -c Release --project benchmarks/NetDaemon.PerformanceBenchmarks -- --filter '*ResultDispatch*'
dotnet run -c Release --project benchmarks/NetDaemon.PerformanceBenchmarks -- --filter '*QueuedObservable*'
dotnet run -c Release --project benchmarks/NetDaemon.PerformanceBenchmarks -- --filter '*StateChangeJson*'
```

For quick validation without full benchmark confidence:

```bash
dotnet run -c Release --project benchmarks/NetDaemon.PerformanceBenchmarks -- --filter '*' --job Dry
```

The dry run was used to verify that all 16 benchmark cases execute end-to-end. Do not use dry-run timings as decision-grade performance numbers; run the default BenchmarkDotNet job for comparisons that will drive code changes.

Benchmark reports are written under the benchmark project's build output directory so normal runs do not create root-level repo artifacts.

## Ranked Findings

1. **Result dispatch scales with every pending command**

   Evidence: `HomeAssistantConnection.SendCommandAsyncInternal` creates a filtered Rx subscription for each command waiting for a `result` message. Every incoming result flows through the shared `Subject<HassMessage>` and each active predicate checks `Type` and `Id`.

   Why it matters: service calls are network-bound individually, but Home Assistant automations can issue bursts. Under high in-flight command counts, result correlation should be near O(1) per result; the current shape tends toward O(pending commands).

   Recommendation: replace the per-command filtered subscription path with an internal `ConcurrentDictionary<int, TaskCompletionSource<HassMessage>>` keyed by command id. In the receive loop, route `type == "result"` messages directly to the matching completion source and publish non-result/event messages through the existing observable path. Keep public APIs unchanged.

   Risk: medium. This touches command completion, timeout, disposal, and error logging semantics. Add tests for success, HA error result, timeout, cancellation, disposal with pending commands, and out-of-order results.

2. **Websocket receive currently double-deserializes message payloads**

   Evidence: `WebSocketTransportPipeline.ReadMessagesFromPipelineAndSerializeAsync` deserializes the pipe to `JsonElement?`, then deserializes that element again to either `T` or `T[]`.

   Why it matters: this is in the core event ingest path. Coalesced HA event arrays multiply the cost because the whole payload becomes a DOM before the typed messages are created.

   Recommendation: benchmark and prototype a direct parse path that uses `JsonDocument.ParseAsync` only to detect object versus array, then deserializes from the original buffered bytes, or accumulates the websocket message into pooled memory and calls `JsonSerializer.Deserialize<T>`/`Deserialize<T[]>` once. Keep chunked message support and close/cancellation behavior intact.

   Risk: medium. The pipe-based implementation is currently simple and handles chunking well; a replacement must prove lower allocations without regressing large messages or close-frame handling.

3. **Per-app event queues multiply work for all-event subscriptions**

   Evidence: each `AppScopedHaContextProvider` creates a `QueuedObservable<HassEvent>` over `EntityStateCache.AllEvents`. Each scope subscribes to every HA event before app-level filters such as `StateAllChanges` and `Events.Filter<T>` run.

   Why it matters: the design protects apps from each other's slow observers, which is valuable, but high app counts turn one HA event into N channel writes and N queue drains even when most apps only care about a narrow event type.

   Recommendation: keep per-app isolation, but evaluate moving common filters before queueing for common APIs. Candidate: provide internal typed/event-filtered queued streams for `state_changed` and trigger messages so apps that only use state changes do not queue unrelated HA events.

   Risk: medium-high. Event ordering and isolation are user-visible automation semantics. Require latency/drop benchmarks with 1, 10, and 50 app scopes plus one slow observer before changing behavior.

4. **Bounded queue drops are only logged, not observable**

   Evidence: `QueuedObservable` uses a bounded channel with capacity 1024, logs near 90%, and drops events when `TryWrite` fails.

   Why it matters: dropping is a reasonable protection against slow automations, but in real HA event storms this can silently degrade automation correctness beyond logs.

   Recommendation: add internal counters or structured diagnostics for near-full and dropped events. Consider configurable capacity only after benchmarked evidence that realistic workloads exceed 1024 with healthy handlers.

   Risk: low for diagnostics, medium for policy changes. Avoid changing drop/backpressure behavior without compatibility discussion.

5. **Registry update handling does full reloads**

   Evidence: `RegistryCache` subscribes to registry update events and reloads whole entity/device/area/label/floor registries after throttling.

   Why it matters: this is secondary for high-event steady state, but large HA installations can make reload bursts visible, and reloads use network calls that can contend with command traffic.

   Recommendation: leave this behind the core event path. If benchmarks or user traces show registry churn, inspect HA registry update payloads and switch only safe cases to incremental updates.

   Risk: medium. Incremental cache invalidation can be subtle; full reload is robust.

## Benchmark Coverage Added

- `WebSocketPipelineBenchmarks`: single event, coalesced events, and send serialization.
- `ResultDispatchBenchmarks`: current per-command Rx filtered subscriptions versus ID-indexed `ConcurrentDictionary` dispatch, with both paths awaiting all completions.
- `QueuedObservableBenchmarks`: fan-out cost across 1, 10, and 50 app-scoped queues.
- `StateChangeJsonBenchmarks`: lazy state-change cache path versus forced `HassState` deserialization.

## Implemented Hot-Path Improvements

Baseline commit: `d1eb2dd4 Add high-event performance benchmark baseline`.

Post-change benchmark command:

```bash
dotnet run -c Release --project benchmarks/NetDaemon.PerformanceBenchmarks -- --filter '*'
```

Benchmark reports:

- `benchmarks/NetDaemon.PerformanceBenchmarks/bin/Release/net10.0/BenchmarkDotNet.Artifacts/results/NetDaemon.PerformanceBenchmarks.Benchmarks.ResultDispatchBenchmarks-report-github.md`
- `benchmarks/NetDaemon.PerformanceBenchmarks/bin/Release/net10.0/BenchmarkDotNet.Artifacts/results/NetDaemon.PerformanceBenchmarks.Benchmarks.WebSocketPipelineBenchmarks-report-github.md`
- `benchmarks/NetDaemon.PerformanceBenchmarks/bin/Release/net10.0/BenchmarkDotNet.Artifacts/results/NetDaemon.PerformanceBenchmarks.Benchmarks.StateChangeJsonBenchmarks-report-github.md`
- `benchmarks/NetDaemon.PerformanceBenchmarks/bin/Release/net10.0/BenchmarkDotNet.Artifacts/results/NetDaemon.PerformanceBenchmarks.Benchmarks.QueuedObservableBenchmarks-report-github.md`

### Result Dispatch

`HomeAssistantConnection` now tracks pending result waiters in an ID-indexed `ConcurrentDictionary<int, TaskCompletionSource<HassMessage>>`. The receive loop completes matching `result` messages directly and still publishes every raw `HassMessage` to `OnHassMessage`.

The initial post-change benchmark used a regular `Dictionary` and did not await the completion tasks on the implemented path, so its precise 1/10/100/1000 pending-command percentages should not be reused. After review, the benchmark was corrected to use `ConcurrentDictionary` and to await all completions on both paths.

Corrected local result-dispatch run:

```bash
dotnet benchmarks/NetDaemon.PerformanceBenchmarks/bin/Release/net10.0/NetDaemon.PerformanceBenchmarks.dll -i -f '*ResultDispatchBenchmarks*'
```

| Pending commands | Baseline Rx dispatch | Implemented concurrent dictionary path | Result |
|-----------------:|---------------------:|---------------------------------------:|-------:|
| 1 | 250.2 ns, 880 B | 485.2 ns, 1,800 B | slower for a single waiter |
| 10 | 2,175.3 ns, 6,424 B | 929.4 ns, 3,312 B | 57% faster, 48% lower allocation |
| 100 | 30,939.1 ns, 133,144 B | 7,566.0 ns, 24,880 B | 76% faster, 81% lower allocation |
| 1000 | 1,819.0 us, 8,528,351 B | 80.2 us, 262,848 B | 96% faster, 97% lower allocation |

The tradeoff is intentional: the concurrent dictionary path carries fixed overhead for one pending command, but avoids the per-command Rx subscription scan that dominates as pending command count grows.

Behavior covered by tests:

- concurrent commands complete from matching result IDs when results arrive out of order
- raw `result` messages are still published to `IHomeAssistantHassMessages.OnHassMessage`
- send failures, result wait timeouts, and caller cancellation remove pending result state and do not break later commands
- disposing the connection cancels pending result waiters

### Websocket Receive

`WebSocketClientTransportPipeline` now reads a complete websocket message into a byte buffer, checks whether the payload is a JSON object or array, and deserializes directly to `T` or `T[]`. This removes the previous `JsonElement` intermediate followed by a second typed deserialization.

| Scenario | Baseline | Post-change | Result |
|---------|---------:|------------:|-------:|
| Single event read | 6,638.9 ns, 7.2 KB | 2,054.0 ns, 5,600 B | 69% faster, lower allocation |
| 64 coalesced events | 306,653.5 ns, 259.98 KB | 111,851.9 ns, 291,785 B | 64% faster, about 10% more allocation |
| Send service command | 548.0 ns, 1.07 KB | 605.0 ns, 646 B | effectively unchanged for this receive-side change |

The coalesced-event receive path is the meaningful win here because it is the high-event ingest case. The allocation increase should be watched if this path is tuned further; the latency reduction is large enough to keep this implementation.

### State-Change Benchmark Harness

The initial baseline run failed `StateChangeJsonBenchmarks.ExtractEntityIdAndLazyNewState` because the benchmark stored a `JsonElement` after its owning document had gone out of scope. The benchmark now keeps the owning `JsonDocument` alive for the benchmark lifetime.

Post-fix measurements:

| Scenario | Mean | Allocated |
|---------|-----:|----------:|
| Extract entity id and lazy `new_state` element | 31.28 ns | 48 B |
| Force deserialize `new_state` | 505.98 ns | 952 B |

The fixed benchmark confirms the current lazy state-change path avoids most of the deserialization cost until an observer asks for typed state.

These measurements are local-machine comparisons on this workstation. Treat them as change-direction evidence for this codebase, not as cross-machine absolute performance truth.

## Acceptance Criteria For Future Fixes

- Maintain public API compatibility unless a diagnostic-only option is explicitly accepted.
- Improve hot-path p95 latency, allocation rate, queue drops, or throughput by a meaningful margin in the benchmark harness.
- Preserve websocket chunking/coalescing support, event ordering within an app scope, slow-subscriber isolation, and command timeout/error semantics.
