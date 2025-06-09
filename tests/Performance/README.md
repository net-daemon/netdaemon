# Performance tests

The following directory contains a simple performance test for NetDaemon runtime.

It has two parts, one fake Home Assistant server that fakes on-boarding and waits for command to start pushing state changes as fast as possible.
The other part is the NetDaemon runtime with three identical apps that listens to the same event state changes as calculate the performance of
number of messages per second processed in each app.

## How to run the performance test

Open two terminal windows. `cd` to the root of the repository in both terminals.

1. Start the fake Home Assistant fake server in `tests/Performance/PerfServer`:

```bash

cd tests/Performance/PerfServer
dotnet run -c Release

```

2. Start the NetDaemon runtime in `tests/Performance/PerfClient`:

```bash
cd tests/Performance/PerfClient
dotnet run -c Release
```

## Expected output

The performance test will start and you will see the number of messages processed per second in the console after one million messages.

## Future improvements

These tests are very basic MVP and can be improved in many ways. Example the tests can be dockerized and run in a CI/CD pipeline to ensure that performance is not degraded over time.



