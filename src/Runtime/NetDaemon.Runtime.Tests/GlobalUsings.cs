global using System;
global using System.Reactive.Linq;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using FluentAssertions;
global using Moq;
global using Xunit;
global using NetDaemon.Client.Common;
global using NetDaemon.Client.Common.HomeAssistant.Model;
global using NetDaemon.Client.Internal.Extensions;
global using NetDaemon.Client.Internal.HomeAssistant.Commands;
global using NetDaemon.Client.Common.Settings;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("NetDaemon.Runtime.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
