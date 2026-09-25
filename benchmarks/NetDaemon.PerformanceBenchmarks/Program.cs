using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

var artifactsPath = Path.Combine(AppContext.BaseDirectory, "BenchmarkDotNet.Artifacts");
var config = DefaultConfig.Instance.WithArtifactsPath(artifactsPath);

BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
